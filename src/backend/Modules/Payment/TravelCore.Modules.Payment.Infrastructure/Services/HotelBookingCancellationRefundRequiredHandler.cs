using Microsoft.EntityFrameworkCore;
using NodaTime;
using Npgsql;
using TravelCore.Modules.Payment.Contracts;
using TravelCore.Modules.Payment.Domain;

namespace TravelCore.Modules.Payment.Infrastructure.Services;

/// <summary>
/// Payment consumer of HotelBooking confirmed-cancellation full-refund-required.
/// Event is a trigger; amount comes from PaymentExecutionSnapshot. One Refund per Payment.
/// </summary>
internal sealed class HotelBookingCancellationRefundRequiredHandler : IHotelBookingCancellationRefundRequiredHandler
{
    private readonly PaymentDbContext _db;
    private readonly RefundGetOrCreateService _getOrCreate;
    private readonly RefundInitiationService _initiation;
    private readonly IClock _clock;

    public HotelBookingCancellationRefundRequiredHandler(
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
        HotelBookingCancellationRefundRequiredIntegrationEvent message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (await _db.HotelBookingCancellationRefundInbox.AnyAsync(
                x => x.HotelBookingCancellationId == message.HotelBookingCancellationId,
                cancellationToken)
            || _db.HotelBookingCancellationRefundInbox.Local.Any(
                x => x.HotelBookingCancellationId == message.HotelBookingCancellationId))
        {
            return;
        }

        var payment = await _db.Payments
            .Include(item => item.Attempts)
            .Include(item => item.ExecutionSnapshot)
            .SingleOrDefaultAsync(item => item.Id == PaymentId.From(message.PaymentId), cancellationToken)
            ?? throw new InvalidOperationException("Payment was not found.");

        if (payment.HotelBooking?.HotelBookingId != message.HotelBookingId)
        {
            throw new InvalidOperationException("Hotel cancellation Payment does not belong to the HotelBooking.");
        }

        if (payment.Status != PaymentStatus.Succeeded)
        {
            throw new InvalidOperationException("Hotel cancellation refund requires a Succeeded Payment.");
        }

        if (payment.ExecutionSnapshot is null)
        {
            throw new InvalidOperationException("Hotel cancellation refund requires PaymentExecutionSnapshot.");
        }

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

        _db.HotelBookingCancellationRefundInbox.Add(
            PaymentHotelBookingCancellationRefundInboxRecord.Create(
                message.HotelBookingCancellationId,
                message.PaymentId,
                _clock.GetCurrentInstant()));
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
