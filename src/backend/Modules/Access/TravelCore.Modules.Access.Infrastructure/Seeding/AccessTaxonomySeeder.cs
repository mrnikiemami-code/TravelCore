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

        // Agency presentation baseline (T011) — commerce-free capability gate.
        foreach (var (code, displayName) in AccessPermissionCatalog.AgencyPresentationBaseline)
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

        var agency = await db.Roles.FirstOrDefaultAsync(x => x.Code == AccessPermissionCatalog.AgencyRoleCode, cancellationToken);
        if (agency is null)
        {
            agency = RoleEntity.Create(
                AccessPermissionCatalog.AgencyRoleCode,
                AccessPermissionCatalog.AgencyRoleDisplayName,
                now);
            db.Roles.Add(agency);
            await db.SaveChangesAsync(cancellationToken);
            agency = await db.Roles.FirstAsync(x => x.Code == AccessPermissionCatalog.AgencyRoleCode, cancellationToken);
        }

        foreach (var (code, _) in AccessPermissionCatalog.AgencyPresentationBaseline)
        {
            agency.GrantPermission(permissionIds[code], now);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
