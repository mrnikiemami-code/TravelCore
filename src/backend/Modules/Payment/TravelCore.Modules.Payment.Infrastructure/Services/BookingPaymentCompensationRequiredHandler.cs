using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Payment consumer of Booking compensation-required. Event is a trigger; amount comes from snapshot.
/// </summary>
internal sealed class BookingPaymentCompensationRequiredHandler : IBookingPaymentCompensationRequiredHandler
{
    private readonly PaymentDbContext _db;
    private readonly RefundGetOrCreateService _getOrCreate;
    private readonly RefundInitiationService _initiation;
    private readonly IClock _clock;

    public BookingPaymentCompensationRequiredHandler(
        PaymentDbContext db,
        RefundGetOrCreateService getOrCreate,
        RefundInitiationService initiation,
        IClock clock)
    {
        _db = db;
        _getOrCreate = getOrCreate;
        _initiation = initiation;
        _clock = clock;
    }

    public async Task HandleAsync(
        BookingPaymentCompensationRequiredIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var refund = await _getOrCreate.GetOrCreateAsync(PaymentId.From(message.PaymentId), cancellationToken);
        if (refund.Status != RefundStatus.Succeeded)
        {
            await _initiation.InitiateAsync(refund.Id, cancellationToken);
            refund = await _db.Refunds
                .Include(item => item.Attempts)
                .Include(item => item.Amount)
                .SingleAsync(item => item.Id == refund.Id, cancellationToken);
        }

        if (refund.Status == RefundStatus.Succeeded)
        {
            RefundSucceededOutboxWriter.EnqueueIfSucceeded(
                _db,
                refund,
                _clock.GetCurrentInstant(),
                VerificationApplyStatus.Unchanged);
        }

        if (await _db.CompensationInbox.AnyAsync(x => x.PaymentId == message.PaymentId, cancellationToken)
            || _db.CompensationInbox.Local.Any(x => x.PaymentId == message.PaymentId))
        {
            await _db.SaveChangesAsync(cancellationToken);
            return;
        }

        _db.CompensationInbox.Add(
            PaymentCompensationInboxRecord.Create(message.PaymentId, _clock.GetCurrentInstant()));
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
