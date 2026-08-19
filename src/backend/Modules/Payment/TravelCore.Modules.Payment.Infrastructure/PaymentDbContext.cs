using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Payment-owned DbContext. Owns PostgreSQL schema <c>payment</c>.
/// Same-schema FKs Refund→Payment and RefundAttempt→Refund are allowed (P20-R6).
/// </summary>
public sealed class PaymentDbContext : DbContext
{
    public const string SchemaName = "payment";

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<PaymentAggregate> Payments => Set<PaymentAggregate>();

    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();

    public DbSet<PaymentInitiationIdempotencyRecord> InitiationIdempotency =>
        Set<PaymentInitiationIdempotencyRecord>();

    public DbSet<PaymentReconciliationIssue> ReconciliationIssues => Set<PaymentReconciliationIssue>();

    public DbSet<PaymentOutboxMessage> OutboxMessages => Set<PaymentOutboxMessage>();

    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<RefundAttempt> RefundAttempts => Set<RefundAttempt>();

    public DbSet<RefundReconciliationIssue> RefundReconciliationIssues => Set<RefundReconciliationIssue>();

    public DbSet<PaymentCompensationInboxRecord> CompensationInbox => Set<PaymentCompensationInboxRecord>();

    public DbSet<PaymentHotelBookingCancellationRefundInboxRecord> HotelBookingCancellationRefundInbox =>
        Set<PaymentHotelBookingCancellationRefundInboxRecord>();

    public DbSet<PaymentFlightBookingCancellationRefundInboxRecord> FlightBookingCancellationRefundInbox =>
        Set<PaymentFlightBookingCancellationRefundInboxRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}
