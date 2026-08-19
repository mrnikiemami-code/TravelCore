using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Notification.Infrastructure;

/// <summary>
/// Notification-owned DbContext. Owns PostgreSQL schema <c>notification</c>.
/// T004 is schema foundation only — no product tables.
/// </summary>
public sealed class NotificationDbContext : DbContext
{
    public const string SchemaName = "notification";

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}
