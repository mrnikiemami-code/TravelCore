using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Booking.Domain;
using BookingAggregate = TravelCore.Modules.Booking.Domain.Booking;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking-owned contact/passenger application boundary (TC-P19-T004 / P19-R4).
/// Does not log passenger/contact payloads. Does not clone Party/Identity.
/// </summary>
public sealed class BookingPeopleService
{
    private readonly BookingDbContext _db;

    public BookingPeopleService(BookingDbContext db)
    {
        _db = db;
    }

    public async Task SetContactAsync(
        BookingId bookingId,
        BookingContactSnapshot contact,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadBookingAsync(bookingId, cancellationToken);
        booking.SetContact(contact);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<BookingPassenger> AddPassengerAsync(
        BookingId bookingId,
        string givenName,
        string familyName,
        TravelerCategory category,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadBookingAsync(bookingId, cancellationToken);
        var heldSeats = await ActiveHeldSeatsAsync(bookingId, cancellationToken);
        var passenger = booking.AddPassenger(givenName, familyName, category, heldSeats);
        await _db.SaveChangesAsync(cancellationToken);
        return passenger;
    }

    public async Task UpdatePassengerAsync(
        BookingId bookingId,
        BookingPassengerId passengerId,
        string givenName,
        string familyName,
        TravelerCategory category,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadBookingAsync(bookingId, cancellationToken);
        booking.UpdatePassenger(passengerId, givenName, familyName, category);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemovePassengerAsync(
        BookingId bookingId,
        BookingPassengerId passengerId,
        CancellationToken cancellationToken = default)
    {
        var booking = await LoadBookingAsync(bookingId, cancellationToken);
        booking.RemovePassenger(passengerId);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task<BookingAggregate> LoadBookingAsync(BookingId bookingId, CancellationToken cancellationToken)
    {
        return await _db.Bookings
            .Include(x => x.Passengers)
            .SingleOrDefaultAsync(x => x.Id == bookingId, cancellationToken)
            ?? throw new InvalidOperationException("Booking was not found.");
    }

    private async Task<int?> ActiveHeldSeatsAsync(BookingId bookingId, CancellationToken cancellationToken)
    {
        var hold = await _db.CapacityHolds
            .SingleOrDefaultAsync(
                x => x.BookingId == bookingId && x.Status == CapacityHoldStatus.Active,
                cancellationToken);
        return hold?.SeatCount;
    }
}
