using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure.Services;

/// <summary>
/// Trusted read of an existing Pricing Quote for Booking consumption (TC-P19-T005).
/// Does not mutate Quote/Price and does not expose Booking types.
/// </summary>
public sealed class AuthoritativeQuoteQuery : IAuthoritativeQuoteQuery
{
    private readonly PricingDbContext _db;

    public AuthoritativeQuoteQuery(PricingDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<AuthoritativeQuote?> GetByIdAsync(
        Guid quoteId,
        CancellationToken cancellationToken = default)
    {
        if (quoteId == Guid.Empty)
        {
            throw new ArgumentException("QuoteId cannot be empty.", nameof(quoteId));
        }

        var quote = await _db.Quotes
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == QuoteId.From(quoteId), cancellationToken);

        return quote is null ? null : Map(quote);
    }

    internal static AuthoritativeQuote Map(Quote quote)
    {
        ArgumentNullException.ThrowIfNull(quote);
        var components = quote.SnapshotComponentsOrdered
            .Select(component => new AuthoritativeQuoteComponent(
                component.Kind.ToString(),
                new MoneyResponse(component.Money.Amount, component.Money.Currency.Value),
                component.SortOrder,
                component.Code,
                component.Label))
            .ToList();

        return new AuthoritativeQuote(
            quote.Id.Value,
            quote.SourcePriceId.Value,
            quote.SnapshotTargetType?.Value,
            quote.SnapshotTargetId,
            quote.CreatedAt.ToDateTimeOffset(),
            quote.ExpiresAt.ToDateTimeOffset(),
            quote.Currency.Value,
            quote.Total.Amount,
            components,
            quote.CommercialContextAgencyOfferId);
    }
}
