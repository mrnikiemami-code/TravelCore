namespace TravelCore.Modules.Pricing.Contracts;

/// <summary>
/// Trusted Pricing read contract for an already-created Quote (TC-P19-T005 / P19-R5).
/// Read-only — not Quote mutation, not live Price, not Booking/Payment authority.
/// Contracts stay free of NodaTime and TravelCore.Money; instants are UTC DateTimeOffset.
/// </summary>
public sealed record AuthoritativeQuoteComponent(
    string Kind,
    MoneyResponse Money,
    int SortOrder,
    string? Code,
    string? Label);

public sealed record AuthoritativeQuote(
    Guid QuoteId,
    Guid SourcePriceId,
    string? TargetType,
    Guid? TargetId,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    string Currency,
    decimal TotalAmount,
    IReadOnlyList<AuthoritativeQuoteComponent> Components);

public interface IAuthoritativeQuoteQuery
{
    Task<AuthoritativeQuote?> GetByIdAsync(Guid quoteId, CancellationToken cancellationToken = default);
}
