using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Pricing.Contracts;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking-owned Quote consumption boundary (TC-P19-T005 / P19-R5).
/// Obtains authoritative Quote facts from Pricing.Contracts; Booking stores an immutable copy.
/// Public/client callers may supply only a Quote identifier — not monetary values.
/// </summary>
public sealed class BookingQuoteService
{
    private readonly BookingDbContext _db;
    private readonly IAuthoritativeQuoteQuery _quotes;

    public BookingQuoteService(BookingDbContext db, IAuthoritativeQuoteQuery quotes)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(quotes);
        _db = db;
        _quotes = quotes;
    }

    public async Task AcceptQuoteAsync(
        BookingId bookingId,
        Guid quoteId,
        Instant now,
        CancellationToken cancellationToken = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        await AcquireBookingLockAsync(bookingId.Value, cancellationToken);

        var booking = await _db.Bookings
            .Include(x => x.MonetarySnapshot)
            .ThenInclude(x => x!.Components)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");

        var quote = await _quotes.GetByIdAsync(quoteId, cancellationToken)
            ?? throw new InvalidOperationException("Quote was not found.");

        booking.AcceptQuote(MapFacts(quote), now);
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    internal static AuthoritativeQuoteFacts MapFacts(AuthoritativeQuote quote)
    {
        ArgumentNullException.ThrowIfNull(quote);
        var components = quote.Components
            .Select(component =>
            {
                if (!Enum.TryParse<BookingMonetaryComponentKind>(component.Kind, ignoreCase: false, out var kind))
                {
                    throw new InvalidOperationException("Quote component kind is not an accepted Pricing contract kind.");
                }

                return new AuthoritativeQuoteComponentFact(
                    kind,
                    new MoneyValue(component.Money.Amount, component.Money.CurrencyCode),
                    component.SortOrder,
                    component.Code,
                    component.Label);
            })
            .ToList();

        return AuthoritativeQuoteFacts.Create(
            PricingQuoteReference.From(quote.QuoteId),
            quote.SourcePriceId,
            quote.TargetType,
            quote.TargetId,
            Instant.FromDateTimeOffset(quote.CreatedAt),
            Instant.FromDateTimeOffset(quote.ExpiresAt),
            components);
    }

    private Task AcquireBookingLockAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var bytes = bookingId.ToByteArray();
        var key1 = BitConverter.ToInt32(bytes, 0);
        var key2 = BitConverter.ToInt32(bytes, 4);
        return _db.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({key1}, {key2})",
            cancellationToken);
    }
}
