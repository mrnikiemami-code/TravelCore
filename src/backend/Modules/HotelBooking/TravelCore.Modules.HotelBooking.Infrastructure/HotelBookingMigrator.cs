using Microsoft.EntityFrameworkCore;

namespace TravelCore.Modules.HotelBooking.Infrastructure;

/// <summary>
/// Explicit HotelBooking-owned migrator. Not auto-run on host startup.
/// </summary>
public static class HotelBookingMigrator
{
    public static Task MigrateAsync(
        HotelBookingDbContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Database.MigrateAsync(cancellationToken);
    }
}
