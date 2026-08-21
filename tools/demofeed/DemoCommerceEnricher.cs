using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Infrastructure;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Infrastructure;

namespace TravelCore.Tools.DemoFeed;

/// <summary>
/// TC-P33-T005 I1 — minimum honest commercial scenario via owner services:
/// TourProduct → Published TourDeparture → Price (TargetType=TourDeparture).
/// Idempotent ledger under demofeed media root. No Booking/Payment. No FE hardcodes.
/// </summary>
internal static class DemoCommerceEnricher
{
    private const string TourCode = "demofeed-tour-teh-1";
    private const string LedgerFileName = "commerce-ledger.json";
    private const decimal DemoBaseAmount = 1290m;
    private const string DemoCurrency = "USD";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public static async Task<int> EnrichAsync(IServiceProvider root, string[] args, CancellationToken ct)
    {
        var ensureSchema = args.Any(a => a.Equals("--ensure-schema", StringComparison.OrdinalIgnoreCase));
        if (ensureSchema)
        {
            var schemaExit = await EnsureSchemaAsync(root, ct);
            if (schemaExit != 0)
            {
                return schemaExit;
            }
        }

        await using var scope = root.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var tours = sp.GetRequiredService<ITourProductService>();
        var departures = sp.GetRequiredService<ITourDepartureAdminService>();
        var published = sp.GetRequiredService<ITourDeparturePublicQuery>();
        var prices = sp.GetRequiredService<IPriceAdminService>();
        var publicPricing = sp.GetRequiredService<IPublicPricingQuery>();

        var product = await tours.GetByCodeAsync(TourCode, cancellationToken: ct);
        if (product is null)
        {
            Console.Error.WriteLine(
                $"TourProduct '{TourCode}' not found. Run: seed tours --ensure-schema --connection \"...\"");
            return 4;
        }

        if (!string.Equals(product.CatalogStatus, "Published", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine(
                $"TourProduct '{TourCode}' catalog status is '{product.CatalogStatus}' (expected Published).");
            return 4;
        }

        var ledgerPath = Path.Combine(DemoFeedHost.ResolveMediaRoot(), LedgerFileName);
        var ledger = LoadLedger(ledgerPath);
        var applied = 0;
        var skipped = 0;

        var departureKey = $"commerce:{TourCode}:departure";
        var priceKey = $"commerce:{TourCode}:price";

        Guid departureId;
        if (ledger.TryGetValue(departureKey, out var existingDeparture)
            && Guid.TryParse(existingDeparture, out departureId)
            && await departures.GetAsync(departureId, ct) is { } existingDep
            && string.Equals(existingDep.Status, "Published", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine($"SKIP departure ledger hit: {departureKey} → {departureId:D}");
            skipped++;
        }
        else
        {
            var created = await departures.CreateAsync(new CreateTourDepartureRequest(product.Id), ct);
            departureId = created.Id;

            // Deterministic demo window: start = UTC today + 30 days, end = +37 days.
            var start = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(30));
            var end = start.AddDays(7);
            await departures.SetScheduleAsync(
                departureId,
                new SetTourDepartureScheduleRequest(
                    start.ToString("yyyy-MM-dd"),
                    end.ToString("yyyy-MM-dd"),
                    "Asia/Tehran"),
                ct);
            await departures.SetCapacityAsync(
                departureId,
                new SetTourDepartureCapacityRequest(MinimumPax: 1, MaximumPax: 20),
                ct);
            await departures.SetStatusAsync(
                departureId,
                new SetTourDepartureStatusRequest("Published"),
                ct);

            ledger[departureKey] = departureId.ToString("D");
            SaveLedger(ledgerPath, ledger);
            Console.WriteLine($"APPLIED Published TourDeparture {departureId:D} for {TourCode}");
            applied++;
        }

        Guid priceId;
        if (ledger.TryGetValue(priceKey, out var existingPrice)
            && Guid.TryParse(existingPrice, out priceId)
            && await prices.GetAsync(priceId, ct) is not null)
        {
            Console.WriteLine($"SKIP price ledger hit: {priceKey} → {priceId:D}");
            skipped++;
        }
        else
        {
            var price = await prices.CreateAsync(
                new CreatePriceRequest(
                    TargetType: "TourDeparture",
                    TargetId: departureId,
                    Components:
                    [
                        new PriceComponentInput(
                            Kind: "Base",
                            Money: new MoneyInput(DemoBaseAmount, DemoCurrency),
                            SortOrder: 0,
                            Code: "DEMOFEED-BASE",
                            Label: "DEMOFEED sample base (non-production)"),
                    ],
                    OccupancyRules:
                    [
                        new PriceOccupancyRuleInput(
                            MarketPriceType: "Public",
                            PassengerCategory: "Adult",
                            OccupancyCategory: "DoubleRoom",
                            Money: new MoneyInput(DemoBaseAmount, DemoCurrency),
                            SortOrder: 0),
                    ]),
                ct);

            priceId = price.Id;
            ledger[priceKey] = priceId.ToString("D");
            SaveLedger(ledgerPath, ledger);
            Console.WriteLine(
                $"APPLIED Price {priceId:D} → TourDeparture {departureId:D} ({DemoBaseAmount} {DemoCurrency})");
            applied++;
        }

        // Public API consumption checks (same process / owner queries).
        var publishedList = await published.GetPublishedByTourProductAsync(product.Id, ct);
        var summary = await publicPricing.GetByTourDepartureIdAsync(departureId, ct);

        Console.WriteLine("--- validation ---");
        Console.WriteLine($"TourProduct: {TourCode} id={product.Id:D}");
        Console.WriteLine($"Published departures for product: {publishedList.Count}");
        Console.WriteLine($"Departure in list: {publishedList.Any(d => d.Id == departureId)}");
        Console.WriteLine($"Public price summary: {(summary is null ? "MISSING" : $"{summary.Currency} components={summary.Components.Count}")}");
        Console.WriteLine($"Ledger: {ledgerPath}");
        Console.WriteLine($"Stats: applied={applied} skipped={skipped}");

        if (publishedList.All(d => d.Id != departureId) || summary is null)
        {
            Console.Error.WriteLine("Public consumption validation FAILED.");
            return 5;
        }

        Console.WriteLine("I1 commercial scenario ready (no Booking/Payment).");
        return 0;
    }

    public static async Task<int> EnsureSchemaAsync(IServiceProvider root, CancellationToken ct)
    {
        var baseExit = await TourDemoSeed.EnsureSchemaAsync(root, ct);
        if (baseExit != 0)
        {
            return baseExit;
        }

        await using var scope = root.CreateAsyncScope();
        var pricing = scope.ServiceProvider.GetRequiredService<PricingDbContext>();
        Console.WriteLine("Migrating Pricing (owner migrator)...");
        await PricingMigrator.MigrateAsync(pricing, ct);
        Console.WriteLine("Owner schemas ready (+ Pricing).");
        return 0;
    }

    private static Dictionary<string, string> LoadLedger(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return data is null
                ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, string>(data, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static void SaveLedger(string path, Dictionary<string, string> ledger)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(ledger, JsonOptions));
    }
}
