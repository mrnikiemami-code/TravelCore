using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Domain;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Infrastructure.Services;

/// <summary>
/// Admin Price queries/mutations (TC-P12-T006 / P12-R6). Pricing-owned; no Tour/Booking/Payment types.
/// </summary>
public sealed class PriceAdminService : IPriceAdminService
{
    private const int MaxListTake = 200;

    private readonly PricingDbContext _db;

    public PriceAdminService(PricingDbContext db)
    {
        _db = db;
    }

    public async Task<PriceResponse> CreateAsync(
        CreatePriceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var price = Price.Create(
            request.TargetType,
            request.TargetId,
            MapComponentDefinitions(request.Components, nameof(request.Components)),
            MapOccupancyRuleDefinitions(request.OccupancyRules, nameof(request.OccupancyRules)));
        _db.Prices.Add(price);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(price);
    }

    public async Task<PriceResponse?> GetAsync(
        Guid priceId,
        CancellationToken cancellationToken = default)
    {
        var price = await FindAsync(priceId, cancellationToken);
        return price is null ? null : Map(price);
    }

    public async Task<IReadOnlyList<PriceResponse>> ListAsync(
        string? targetType = null,
        Guid? targetId = null,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        if (take < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(take), take, "take must be >= 1.");
        }

        take = Math.Min(take, MaxListTake);
        var query = _db.Prices.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(targetType))
        {
            var parsed = PriceTargetType.Parse(targetType);
            query = query.Where(x => x.TargetType == parsed);
        }

        if (targetId is Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));
            }

            query = query.Where(x => x.TargetId == id);
        }

        var items = await query
            .OrderBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        return items.Select(Map).ToList();
    }

    public async Task<PriceResponse> UpdateAsync(
        Guid priceId,
        UpdatePriceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var price = await RequireAsync(priceId, cancellationToken);
        price.ReplaceDefinition(
            MapComponentDefinitions(request.Components, nameof(request.Components)),
            MapOccupancyRuleDefinitions(request.OccupancyRules, nameof(request.OccupancyRules)));
        await _db.SaveChangesAsync(cancellationToken);
        return Map(price);
    }

    public async Task<PriceResponse> AddComponentAsync(
        Guid priceId,
        PriceComponentInput request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var price = await RequireAsync(priceId, cancellationToken);
        var definition = MapComponentDefinition(request, nameof(request));
        price.AddComponent(definition.Kind, definition.Money, definition.SortOrder, definition.Code, definition.Label);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(price);
    }

    public async Task<PriceResponse> ReplaceComponentsAsync(
        Guid priceId,
        ReplacePriceComponentsRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var price = await RequireAsync(priceId, cancellationToken);
        price.ReplaceComponents(MapComponentDefinitions(request.Components, nameof(request.Components)));
        await _db.SaveChangesAsync(cancellationToken);
        return Map(price);
    }

    public async Task<PriceResponse> AddOccupancyRuleAsync(
        Guid priceId,
        PriceOccupancyRuleInput request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var price = await RequireAsync(priceId, cancellationToken);
        var definition = MapOccupancyRuleDefinition(request, nameof(request));
        price.AddOccupancyRule(
            definition.MarketPriceType,
            definition.PassengerCategory,
            definition.OccupancyCategory,
            definition.Money,
            definition.SortOrder);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(price);
    }

    public async Task<PriceResponse> ReplaceOccupancyRulesAsync(
        Guid priceId,
        ReplacePriceOccupancyRulesRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var price = await RequireAsync(priceId, cancellationToken);
        price.ReplaceOccupancyRules(
            MapOccupancyRuleDefinitions(request.OccupancyRules, nameof(request.OccupancyRules)) ?? []);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(price);
    }

    private async Task<Price?> FindAsync(Guid priceId, CancellationToken cancellationToken)
    {
        if (priceId == Guid.Empty)
        {
            throw new ArgumentException("PriceId cannot be empty.", nameof(priceId));
        }

        var id = PriceId.From(priceId);
        return await _db.Prices.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private async Task<Price> RequireAsync(Guid priceId, CancellationToken cancellationToken)
        => await FindAsync(priceId, cancellationToken)
           ?? throw new InvalidOperationException("Price was not found.");

    private static IReadOnlyList<PriceComponentDefinition> MapComponentDefinitions(
        IReadOnlyList<PriceComponentInput>? inputs,
        string paramName)
    {
        if (inputs is null || inputs.Count == 0)
        {
            throw new ArgumentException("Price requires at least one component.", paramName);
        }

        return inputs.Select((input, index) => MapComponentDefinition(input, $"{paramName}[{index}]")).ToList();
    }

    private static IReadOnlyList<PriceOccupancyRuleDefinition>? MapOccupancyRuleDefinitions(
        IReadOnlyList<PriceOccupancyRuleInput>? inputs,
        string paramName)
    {
        if (inputs is null)
        {
            return null;
        }

        return inputs.Select((input, index) => MapOccupancyRuleDefinition(input, $"{paramName}[{index}]")).ToList();
    }

    private static PriceComponentDefinition MapComponentDefinition(PriceComponentInput input, string paramName)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new PriceComponentDefinition(
            ParseEnum<PriceComponentKind>(input.Kind, paramName),
            ToMoney(input.Money, paramName),
            input.SortOrder,
            input.Code,
            input.Label);
    }

    private static PriceOccupancyRuleDefinition MapOccupancyRuleDefinition(
        PriceOccupancyRuleInput input,
        string paramName)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new PriceOccupancyRuleDefinition(
            ParseEnum<TourMarketPriceType>(input.MarketPriceType, paramName),
            ParseEnum<PassengerCategory>(input.PassengerCategory, paramName),
            ParseEnum<OccupancyCategory>(input.OccupancyCategory, paramName),
            ToMoney(input.Money, paramName),
            input.SortOrder);
    }

    private static MoneyValue ToMoney(MoneyInput input, string paramName)
    {
        ArgumentNullException.ThrowIfNull(input);
        try
        {
            return PricingMoney.Create(input.Amount, input.CurrencyCode);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException(ex.Message, paramName, ex);
        }
    }

    private static TEnum ParseEnum<TEnum>(string? value, string paramName)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var parsed)
            || !Enum.IsDefined(parsed))
        {
            throw new ArgumentException($"Unsupported {typeof(TEnum).Name} '{value}'.", paramName);
        }

        return parsed;
    }

    private static PriceResponse Map(Price price) =>
        new(
            price.Id.Value,
            price.TargetType.Value,
            price.TargetId,
            price.Currency.Value,
            price.ComponentsOrdered.Select(MapComponent).ToList(),
            price.OccupancyRulesOrdered.Select(MapOccupancyRule).ToList());

    private static PriceComponentResponse MapComponent(PriceComponent component) =>
        new(
            component.Id.Value,
            component.Kind.ToString(),
            new MoneyResponse(component.Money.Amount, component.Money.Currency.Value),
            component.SortOrder,
            component.Code,
            component.Label);

    private static PriceOccupancyRuleResponse MapOccupancyRule(PriceOccupancyRule rule) =>
        new(
            rule.Id.Value,
            rule.MarketPriceType.ToString(),
            rule.PassengerCategory.ToString(),
            rule.OccupancyCategory.ToString(),
            new MoneyResponse(rule.Money.Amount, rule.Money.Currency.Value),
            rule.SortOrder);
}
