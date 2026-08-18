using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Payment.Domain;
using PaymentAggregate = TravelCore.Modules.Payment.Domain.Payment;

namespace TravelCore.Modules.Payment.Infrastructure;

/// <summary>
/// Payment-owned DbContext. Owns PostgreSQL schema <c>payment</c>.
/// T002: payments + payment_attempts; no provider/refund tables.
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}
