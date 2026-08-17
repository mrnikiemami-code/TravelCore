namespace TravelCore.Modules.Pricing.Contracts;

/// <summary>
/// Admin Pricing operational contracts (TC-P12-T006 / P12-R6).
/// Owned by Pricing — not Tour Admin. No Quote workflow, Booking, Payment, Checkout, or FX.
/// </summary>
public sealed record MoneyInput(decimal Amount, string CurrencyCode);

public sealed record MoneyResponse(decimal Amount, string CurrencyCode);

public sealed record PriceComponentInput(
    string Kind,
    MoneyInput Money,
    int SortOrder = 0,
    string? Code = null,
    string? Label = null);

public sealed record PriceOccupancyRuleInput(
    string MarketPriceType,
    string PassengerCategory,
    string OccupancyCategory,
    MoneyInput Money,
    int SortOrder = 0);

public sealed record PriceComponentResponse(
    Guid Id,
    string Kind,
    MoneyResponse Money,
    int SortOrder,
    string? Code,
    string? Label);

public sealed record PriceOccupancyRuleResponse(
    Guid Id,
    string MarketPriceType,
    string PassengerCategory,
    string OccupancyCategory,
    MoneyResponse Money,
    int SortOrder);

public sealed record PriceResponse(
    Guid Id,
    string TargetType,
    Guid TargetId,
    string CurrencyCode,
    IReadOnlyList<PriceComponentResponse> Components,
    IReadOnlyList<PriceOccupancyRuleResponse> OccupancyRules);

public sealed record CreatePriceRequest(
    string TargetType,
    Guid TargetId,
    IReadOnlyList<PriceComponentInput> Components,
    IReadOnlyList<PriceOccupancyRuleInput>? OccupancyRules = null);

public sealed record UpdatePriceRequest(
    IReadOnlyList<PriceComponentInput> Components,
    IReadOnlyList<PriceOccupancyRuleInput>? OccupancyRules = null);

public sealed record ReplacePriceComponentsRequest(
    IReadOnlyList<PriceComponentInput> Components);

public sealed record ReplacePriceOccupancyRulesRequest(
    IReadOnlyList<PriceOccupancyRuleInput> OccupancyRules);

/// <summary>
/// Pricing-owned Admin Price surface. Target is a polymorphic logical Guid only.
/// </summary>
public interface IPriceAdminService
{
    Task<PriceResponse> CreateAsync(
        CreatePriceRequest request,
        CancellationToken cancellationToken = default);

    Task<PriceResponse?> GetAsync(
        Guid priceId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PriceResponse>> ListAsync(
        string? targetType = null,
        Guid? targetId = null,
        int take = 50,
        CancellationToken cancellationToken = default);

    Task<PriceResponse> UpdateAsync(
        Guid priceId,
        UpdatePriceRequest request,
        CancellationToken cancellationToken = default);

    Task<PriceResponse> AddComponentAsync(
        Guid priceId,
        PriceComponentInput request,
        CancellationToken cancellationToken = default);

    Task<PriceResponse> ReplaceComponentsAsync(
        Guid priceId,
        ReplacePriceComponentsRequest request,
        CancellationToken cancellationToken = default);

    Task<PriceResponse> AddOccupancyRuleAsync(
        Guid priceId,
        PriceOccupancyRuleInput request,
        CancellationToken cancellationToken = default);

    Task<PriceResponse> ReplaceOccupancyRulesAsync(
        Guid priceId,
        ReplacePriceOccupancyRulesRequest request,
        CancellationToken cancellationToken = default);
}
