using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.Booking.Infrastructure;

/// <summary>
/// Explicit Booking-owned migrator. Not auto-run on host startup.
/// </summary>
public static class BookingMigrator
{
    public static Task MigrateAsync(
        BookingDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
