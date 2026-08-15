using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Access.Domain;
using PermissionEntity = TravelCore.Modules.Access.Domain.Permission;
using RoleEntity = TravelCore.Modules.Access.Domain.Role;

namespace TravelCore.Modules.Access.Infrastructure.Seeding;

/// <summary>
/// Seeds the explicit minimal Admin permission catalog + admin role (idempotent).
/// </summary>
public static class AccessTaxonomySeeder
{
    public static async Task SeedAdminBaselineAsync(
        AccessDbContext db,
        IClock clock,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(clock);

        var now = clock.GetCurrentInstant();
        var permissionIds = new Dictionary<string, PermissionId>(StringComparer.Ordinal);

        foreach (var (code, displayName) in AccessPermissionCatalog.AdminBaseline)
        {
            var existing = await db.Permissions.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
            if (existing is null)
            {
                existing = PermissionEntity.Create(code, displayName, now);
                db.Permissions.Add(existing);
            }

            permissionIds[code] = existing.Id;
        }

        await db.SaveChangesAsync(cancellationToken);

        var admin = await db.Roles.FirstOrDefaultAsync(x => x.Code == AccessPermissionCatalog.AdminRoleCode, cancellationToken);
        if (admin is null)
        {
            admin = RoleEntity.Create(
                AccessPermissionCatalog.AdminRoleCode,
                AccessPermissionCatalog.AdminRoleDisplayName,
                now);
            db.Roles.Add(admin);
            await db.SaveChangesAsync(cancellationToken);
            // Reload with collection tracking after insert
            admin = await db.Roles.FirstAsync(x => x.Code == AccessPermissionCatalog.AdminRoleCode, cancellationToken);
        }

        foreach (var permissionId in permissionIds.Values)
        {
            admin.GrantPermission(permissionId, now);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
