using System.Text.RegularExpressions;
using TravelCore.ArchitectureTests.Support;
using Xunit;

namespace TravelCore.ArchitectureTests;

/// <summary>
/// TC-P12-T001 / P12-R1: Pricing is an independent module owning schema <c>pricing</c>.
/// TC-P12-T002 / P12-R2: Pricing reuses TravelCore.Money; no parallel money types; no FX/Payment.
/// TC-P12-T003 / P12-R3: Price + PriceComponent with polymorphic TargetType+TargetId (TourDeparture initial);
/// no Tour.Domain/Infrastructure refs; no Booking/Payment/FX.
/// TC-P12-T004 / P12-R4: Quote owned by Pricing (snapshot + expiration); no Booking/Payment/Customer/Passenger.
/// TC-P12-T005 / P12-R5: Occupancy/passenger category pricing baseline in Pricing model; no Booking passenger entity.
/// TC-P12-T006 / P12-R6: Admin Pricing operational API owned by Pricing (not Tour Admin); no Booking/Payment/Quote workflow.
/// </summary>
public sealed class PricingBoundaryGuardrailTests
{
    private static readonly string RepoRoot = ProjectGraph.FindRepoRoot();
    private static readonly IReadOnlyList<ProjectModel> Projects = ProjectGraph.LoadAll(RepoRoot);

    [Fact]
    public void PricingProjects_Exist_WithOwnedSchemaConstant()
    {
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Pricing.Contracts");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Pricing.Domain");
        Assert.Contains(Projects, p => p.Name == "TravelCore.Modules.Pricing.Infrastructure");

        Assert.Equal("pricing", TravelCore.Modules.Pricing.Infrastructure.PricingDbContext.SchemaName);
    }

    [Fact]
    public void PricingDomain_MustProjectReference_TravelCoreMoney()
    {
        var pricingDomain = Projects.Single(p => p.Name == "TravelCore.Modules.Pricing.Domain");
        var refs = pricingDomain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .ToList();

        Assert.Contains("TravelCore.Money", refs);
    }

    [Fact]
    public void PricingInfrastructure_MustProjectReference_TravelCoreMoney()
    {
        var pricingInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Pricing.Infrastructure");
        var refs = pricingInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .ToList();

        Assert.Contains("TravelCore.Money", refs);
    }

    [Fact]
    public void PricingModule_MustNotInvent_ParallelMoneyTypes()
    {
        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        Assert.True(Directory.Exists(pricingRoot), pricingRoot);

        var hits = Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    // Forbid parallel Money/Currency primitives; PricingMoney/PricingCurrency factories are allowed.
                    return Regex.IsMatch(
                               x.line,
                               @"\b(class|record|struct|enum)\s+(Money|CurrencyCode|TomanMoney)\b")
                           || Regex.IsMatch(
                               x.line,
                               @"\bnamespace\s+TravelCore\.Modules\.Pricing\.(Money|Currency)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Pricing must reuse TravelCore.Money — must not invent parallel Money/CurrencyCode types:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PricingModule_MustNotIntroduce_Fx_Or_Payment_Yet()
    {
        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        var hits = Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(class|record|enum|struct|interface)\s+(ExchangeRate|FxRate|Payment|PaymentIntent|FxConversion|CurrencyConversion)\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "TC-P12 must not introduce FX/Payment types (P12-R5 deferred):\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PricingDomain_Exposes_Quote_With_Snapshot_And_Expiration()
    {
        var domainRoot = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Domain");

        Assert.True(File.Exists(Path.Combine(domainRoot, "Quote.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "QuoteSnapshotComponent.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "QuoteId.cs")));

        var quoteText = File.ReadAllText(Path.Combine(domainRoot, "Quote.cs"));
        Assert.Contains("SourcePriceId", quoteText, StringComparison.Ordinal);
        Assert.Contains("ExpiresAt", quoteText, StringComparison.Ordinal);
        Assert.Contains("SnapshotComponents", quoteText, StringComparison.Ordinal);
        Assert.Contains("CreateFromPrice", quoteText, StringComparison.Ordinal);
        Assert.DoesNotContain("CustomerId", quoteText, StringComparison.Ordinal);
        Assert.DoesNotContain("PassengerId", quoteText, StringComparison.Ordinal);
        Assert.DoesNotContain("PaymentId", quoteText, StringComparison.Ordinal);
        Assert.DoesNotContain("BookingId", quoteText, StringComparison.Ordinal);
        Assert.DoesNotContain("ReservationId", quoteText, StringComparison.Ordinal);
    }

    [Fact]
    public void Pricing_Quote_MustNot_Couple_Booking_Payment_Or_Customer()
    {
        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        var hits = Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(class|record|enum|struct|interface)\s+(Customer|Passenger|Payment|PaymentIntent|Booking|Reservation|Checkout)\b")
                        || Regex.IsMatch(
                            x.line,
                            @"\b(CustomerId|PassengerId|PaymentId|BookingId|ReservationId)\s*\{");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Quote baseline must not couple Customer/Passenger/Payment/Booking:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PricingDomain_Exposes_Price_And_PriceComponent_With_Polymorphic_Target()
    {
        var domainRoot = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Domain");

        Assert.True(File.Exists(Path.Combine(domainRoot, "Price.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "PriceComponent.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "PriceTargetType.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "PriceComponentKind.cs")));

        var priceText = File.ReadAllText(Path.Combine(domainRoot, "Price.cs"));
        Assert.Contains("TargetType", priceText, StringComparison.Ordinal);
        Assert.Contains("TargetId", priceText, StringComparison.Ordinal);
        Assert.Contains("PriceComponent", priceText, StringComparison.Ordinal);

        var targetText = File.ReadAllText(Path.Combine(domainRoot, "PriceTargetType.cs"));
        Assert.Contains("TourDeparture", targetText, StringComparison.Ordinal);

        var kindText = File.ReadAllText(Path.Combine(domainRoot, "PriceComponentKind.cs"));
        Assert.Contains("Base", kindText, StringComparison.Ordinal);
        Assert.Contains("Fee", kindText, StringComparison.Ordinal);
        Assert.Contains("Tax", kindText, StringComparison.Ordinal);
    }

    [Fact]
    public void PricingDomain_Exposes_Occupancy_And_Passenger_Pricing_Rules()
    {
        var domainRoot = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Domain");

        Assert.True(File.Exists(Path.Combine(domainRoot, "PriceOccupancyRule.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "PriceOccupancyRuleDefinition.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "PassengerCategory.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "OccupancyCategory.cs")));
        Assert.True(File.Exists(Path.Combine(domainRoot, "TourMarketPriceType.cs")));

        var priceText = File.ReadAllText(Path.Combine(domainRoot, "Price.cs"));
        Assert.Contains("OccupancyRules", priceText, StringComparison.Ordinal);
        Assert.Contains("AddOccupancyRule", priceText, StringComparison.Ordinal);
        Assert.Contains("PassengerCategory", priceText, StringComparison.Ordinal);
        Assert.Contains("OccupancyCategory", priceText, StringComparison.Ordinal);
        Assert.Contains("TourMarketPriceType", priceText, StringComparison.Ordinal);
    }

    [Fact]
    public void PricingInfrastructure_PriceComponent_Uses_MoneyOwnedMapping()
    {
        var configPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Infrastructure",
            "Persistence",
            "PriceComponentConfiguration.cs");

        Assert.True(File.Exists(configPath), configPath);
        var text = File.ReadAllText(configPath);
        Assert.Contains("OwnsRequiredMoney", text, StringComparison.Ordinal);
        Assert.Contains("amount", text, StringComparison.Ordinal);
        Assert.Contains("currency_code", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Pricing_Source_MustNot_Using_Tour_Domain_Or_Infrastructure()
    {
        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        var hits = Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"using\s+TravelCore\.Modules\.Tour\.(Domain|Infrastructure)\b|TravelCore\.Modules\.Tour\.(Domain|Infrastructure)\.");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Pricing must not reference Tour.Domain/Infrastructure:\n" + string.Join('\n', hits));
    }

    [Fact]
    public void PricingInfrastructure_MustExpose_MoneyOwnedMapping_Helper()
    {
        var helperPath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Infrastructure",
            "Persistence",
            "MoneyOwnedMapping.cs");

        Assert.True(File.Exists(helperPath), helperPath);
        var text = File.ReadAllText(helperPath);
        Assert.Contains("numeric(24,8)", text, StringComparison.Ordinal);
        Assert.Contains("OwnsRequiredMoney", text, StringComparison.Ordinal);
        Assert.Contains("CurrencyCode.Parse", text, StringComparison.Ordinal);
    }

    [Fact]
    public void PricingInfrastructure_MustNotProjectReference_TourBookingPayment()
    {
        var pricingInfra = Projects.Single(p => p.Name == "TravelCore.Modules.Pricing.Infrastructure");
        var violations = pricingInfra.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.StartsWith("TravelCore.Modules.Tour.", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Booking.", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Payment.", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            violations.Count == 0,
            "Pricing.Infrastructure must not project-reference Tour/Booking/Payment:\n"
            + string.Join('\n', violations));
    }

    [Fact]
    public void PricingDomain_MustNotProjectReference_PeerBusinessModules()
    {
        var pricingDomain = Projects.Single(p => p.Name == "TravelCore.Modules.Pricing.Domain");
        var forbidden = pricingDomain.ProjectReferences
            .Select(r => Path.GetFileNameWithoutExtension(r)!)
            .Where(name =>
                name.Contains(".Infrastructure", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Tour.", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Booking.", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("TravelCore.Modules.Payment.", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(
            forbidden.Count == 0,
            "Pricing.Domain must stay free of Tour/Booking/Payment and peer Infrastructure:\n"
            + string.Join('\n', forbidden));
    }

    [Fact]
    public void PricingModule_DoesNotOwn_TourProduct_TourDeparture_Booking_Payment_Types()
    {
        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        Assert.True(Directory.Exists(pricingRoot), pricingRoot);

        // Comments may mention TourDeparture as a future logical Guid reference; forbid ownership types only.
        var hits = Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x =>
                {
                    var trimmed = x.line.TrimStart();
                    if (trimmed.StartsWith("//", StringComparison.Ordinal)
                        || trimmed.StartsWith("///", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    return Regex.IsMatch(
                        x.line,
                        @"\b(class|record|enum|struct|interface)\s+(TourProduct|TourDeparture|Booking|Payment|PaymentIntent|Reservation|Checkout)\b")
                        || Regex.IsMatch(
                            x.line,
                            @"\b(IBookingService|IPaymentService|ICheckoutService|DbSet<\s*(TourProduct|TourDeparture|Booking|Payment))\b");
                }))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Pricing must not own TourProduct/TourDeparture/Booking/Payment types:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PricingModule_Forbids_TourSchemaFk_And_SharedDbContext()
    {
        var pricingRoot = Path.Combine(RepoRoot, "src", "backend", "Modules", "Pricing");
        var hits = Directory.EnumerateFiles(pricingRoot, "*.cs", SearchOption.AllDirectories)
            .Where(p => !IsGeneratedOrBin(p))
            .SelectMany(path => File.ReadAllLines(path)
                .Select((line, i) => (path, line, i))
                .Where(x => Regex.IsMatch(
                    x.line,
                    @"principalSchema:\s*""tour""|HasOne<.*Tour|TravelCore\.Modules\.Tour\.(Domain|Infrastructure)|TourDbContext|shared\s+DbContext",
                    RegexOptions.IgnoreCase)))
            .Select(x => $"{Path.GetRelativePath(RepoRoot, x.path)}:{x.i + 1}:{x.line.Trim()}")
            .ToList();

        Assert.True(
            hits.Count == 0,
            "Pricing must not introduce Tour schema FK/nav or share Tour DbContext:\n"
            + string.Join('\n', hits));
    }

    [Fact]
    public void PricingModule_Maps_Admin_Price_Endpoints()
    {
        var modulePath = Path.Combine(
            RepoRoot,
            "src",
            "backend",
            "Modules",
            "Pricing",
            "TravelCore.Modules.Pricing.Infrastructure",
            "PricingModule.cs");
        Assert.True(File.Exists(modulePath), modulePath);
        var text = File.ReadAllText(modulePath);
        Assert.Contains("IPriceAdminService", text, StringComparison.Ordinal);
        Assert.Contains("MapPricingAdminEndpoints", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ITourDepartureAdminService", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Booking_And_Payment_Modules_DoNotExist_Yet()
    {
        // Guardrail documents absence: Pricing must not invent project refs to missing commerce peers.
        var booking = Path.Combine(RepoRoot, "src", "backend", "Modules", "Booking");
        var payment = Path.Combine(RepoRoot, "src", "backend", "Modules", "Payment");
        Assert.False(Directory.Exists(booking), "Booking module must not exist in P12 scaffolding.");
        Assert.False(Directory.Exists(payment), "Payment module must not exist in P12 scaffolding.");
    }

    private static bool IsGeneratedOrBin(string path) =>
        path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
        || path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
}
