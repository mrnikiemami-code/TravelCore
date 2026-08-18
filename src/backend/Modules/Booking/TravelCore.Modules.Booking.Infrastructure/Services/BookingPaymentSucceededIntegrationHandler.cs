using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Booking.Domain;
using TravelCore.Modules.Payment.Contracts;

namespace TravelCore.Modules.Booking.Infrastructure.Services;

/// <summary>
/// Booking consumer of PaymentSucceeded. Event is a trigger; confirmation revalidates via query.
/// </summary>
internal sealed class BookingPaymentSucceededIntegrationHandler : IPaymentSucceededIntegrationHandler
{
    private readonly BookingDbContext _db;
    private readonly BookingPaymentConfirmationService _confirm;
    private readonly IClock _clock;

    public BookingPaymentSucceededIntegrationHandler(
        BookingDbContext db,
        BookingPaymentConfirmationService confirm,
        IClock clock)
    {
        _db = db;
        _confirm = confirm;
        _clock = clock;
    }

    public async Task HandleAsync(
        PaymentSucceededIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var existing = await _db.PaymentSuccessInbox
            .SingleOrDefaultAsync(x => x.PaymentId == message.PaymentId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        await _confirm.ConfirmIfEligibleAsync(
            BookingId.From(message.BookingId),
            _clock.GetCurrentInstant(),
            cancellationToken);

        _db.PaymentSuccessInbox.Add(
            PaymentSuccessInboxRecord.Create(message.PaymentId, _clock.GetCurrentInstant()));
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            _db.ChangeTracker.Clear();
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException postgres
                && postgres.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return true;
            }
        }

        return false;
    }
}
