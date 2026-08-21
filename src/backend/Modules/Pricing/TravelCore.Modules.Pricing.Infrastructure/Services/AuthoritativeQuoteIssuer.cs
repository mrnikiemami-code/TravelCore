using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Pricing.Contracts;
using TravelCore.Modules.Pricing.Domain;

namespace TravelCore.Modules.Pricing.Infrastructure.Services;

/// <summary>
/// Issues a new Pricing Quote from the live TourDeparture Price (TC-P19-T008).
/// Pricing remains Quote owner. Booking consumes the issued facts; this is not a public HTTP mutate.
/// </summary>
public sealed class AuthoritativeQuoteIssuer : IAuthoritativeQuoteIssuer
{
    private readonly PricingDbContext _db;

    public AuthoritativeQuoteIssuer(PricingDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<AuthoritativeQuote?> IssueForTourDepartureAsync(
        Guid tourDepartureId,
        DateTimeOffset nowUtc,
        Guid? commercialContextAgencyOfferId = null,
        CancellationToken cancellationToken = default)
    {
        if (tourDepartureId == Guid.Empty)
        {
            throw new ArgumentException("TourDepartureId cannot be empty.", nameof(tourDepartureId));
        }

        if (commercialContextAgencyOfferId == Guid.Empty)
        {
            throw new ArgumentException(
                "CommercialContextAgencyOfferId cannot be empty Guid.",
                nameof(commercialContextAgencyOfferId));
        }

        var now = Instant.FromDateTimeOffset(nowUtc);
        var price = await _db.Prices
            .Where(x => x.TargetType == PriceTargetType.TourDeparture && x.TargetId == tourDepartureId)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (price is null)
        {
            return null;
        }

        var expiresAt = now.Plus(Duration.FromTimeSpan(AuthoritativeQuoteIssuePolicy.TimeToLive));
        var quote = Quote.CreateFromPrice(
            price,
            now,
            expiresAt,
            commercialContextAgencyOfferId: commercialContextAgencyOfferId);
        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync(cancellationToken);
        return AuthoritativeQuoteQuery.Map(quote);
    }
}
