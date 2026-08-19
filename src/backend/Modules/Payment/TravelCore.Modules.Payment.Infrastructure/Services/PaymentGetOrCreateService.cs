using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Database-backed GetOrCreate: one Tour Booking -> one Payment, one HotelBooking -> one Payment (P21-R6).
/// </summary>
internal sealed class PaymentGetOrCreateService
{
    private readonly PaymentDbContext _db;
    private readonly IClock _clock;

    public PaymentGetOrCreateService(PaymentDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<PaymentAggregate> GetOrCreateAsync(
        BookingReference booking,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindTourAsync(booking, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = PaymentAggregate.Create(booking, _clock.GetCurrentInstant());
        _db.Payments.Add(created);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return created;
        }
        catch (DbUpdateException)
        {
            _db.Entry(created).State = EntityState.Detached;
            foreach (var attempt in created.Attempts)
            {
                _db.Entry(attempt).State = EntityState.Detached;
            }

            return await FindTourAsync(booking, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent Payment create did not converge.");
        }
    }

    public async Task<PaymentAggregate> GetOrCreateAsync(
        HotelBookingPaymentReference hotelBooking,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindHotelAsync(hotelBooking, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = PaymentAggregate.CreateForHotel(hotelBooking, _clock.GetCurrentInstant());
        _db.Payments.Add(created);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return created;
        }
        catch (DbUpdateException)
        {
            _db.Entry(created).State = EntityState.Detached;
            foreach (var attempt in created.Attempts)
            {
                _db.Entry(attempt).State = EntityState.Detached;
            }

            return await FindHotelAsync(hotelBooking, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent HotelBooking Payment create did not converge.");
        }
    }

    public async Task<PaymentAggregate> GetOrCreateAsync(
        FlightBookingPaymentReference flightBooking,
        CancellationToken cancellationToken = default)
    {
        var existing = await FindFlightAsync(flightBooking, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = PaymentAggregate.CreateForFlight(flightBooking, _clock.GetCurrentInstant());
        _db.Payments.Add(created);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return created;
        }
        catch (DbUpdateException)
        {
            _db.Entry(created).State = EntityState.Detached;
            foreach (var attempt in created.Attempts)
            {
                _db.Entry(attempt).State = EntityState.Detached;
            }

            return await FindFlightAsync(flightBooking, cancellationToken)
                ?? throw new InvalidOperationException("Concurrent FlightBooking Payment create did not converge.");
        }
    }

    private Task<PaymentAggregate?> FindTourAsync(BookingReference booking, CancellationToken cancellationToken) =>
        _db.Payments
            .Include(item => item.Attempts)
            .SingleOrDefaultAsync(item => item.Booking == booking, cancellationToken);

    private Task<PaymentAggregate?> FindHotelAsync(
        HotelBookingPaymentReference hotelBooking,
        CancellationToken cancellationToken) =>
        _db.Payments
            .Include(item => item.Attempts)
            .SingleOrDefaultAsync(item => item.HotelBooking == hotelBooking, cancellationToken);

    private Task<PaymentAggregate?> FindFlightAsync(
        FlightBookingPaymentReference flightBooking,
        CancellationToken cancellationToken) =>
        _db.Payments
            .Include(item => item.Attempts)
            .SingleOrDefaultAsync(item => item.FlightBooking == flightBooking, cancellationToken);
}
