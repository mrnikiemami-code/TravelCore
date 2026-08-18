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

/// <summary>
/// Pricing-owned Quote issuance for trusted Booking consumption (TC-P19-T008 / P19-R8).
/// Issues a new Quote from the live Price — not a Booking amount, not Payment, not a public mutate API.
/// Time-to-live is Pricing-owned; Booking copies ExpiresAt onto CapacityHold rather than inventing a timeout.
/// </summary>
public static class AuthoritativeQuoteIssuePolicy
{
    public static readonly TimeSpan TimeToLive = TimeSpan.FromHours(2);
}

public interface IAuthoritativeQuoteIssuer
{
    Task<AuthoritativeQuote?> IssueForTourDepartureAsync(
        Guid tourDepartureId,
        DateTimeOffset nowUtc,
        CancellationToken cancellationToken = default);
}
