using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using PermissionEntity = TravelCore.Modules.Access.Domain.Permission;
using RoleEntity = TravelCore.Modules.Access.Domain.Role;

namespace TravelCore.Modules.Access.Infrastructure.Services;

public sealed class AccessTaxonomyService
{
    private readonly AccessDbContext _db;
    private readonly IClock _clock;

    public AccessTaxonomyService(AccessDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<PermissionResponse> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = request.Code.Trim().ToLowerInvariant();
        if (await _db.Permissions.AnyAsync(x => x.Code == code, ct))
        {
            throw new InvalidOperationException("Permission code already exists.");
        }

        var entity = PermissionEntity.Create(request.Code, request.DisplayName, _clock.GetCurrentInstant());
        _db.Permissions.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapPermission(entity);
    }

    public async Task<IReadOnlyList<PermissionResponse>> ListPermissionsAsync(CancellationToken ct)
    {
        var items = await _db.Permissions.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);
        return items.Select(MapPermission).ToList();
    }

    public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var code = request.Code.Trim().ToLowerInvariant();
        if (await _db.Roles.AnyAsync(x => x.Code == code, ct))
        {
            throw new InvalidOperationException("Role code already exists.");
        }

        var entity = RoleEntity.Create(request.Code, request.DisplayName, _clock.GetCurrentInstant());
        _db.Roles.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapRole(entity);
    }

    public async Task<IReadOnlyList<RoleResponse>> ListRolesAsync(CancellationToken ct)
    {
        var items = await _db.Roles.AsNoTracking().OrderBy(x => x.Code).ToListAsync(ct);
        return items.Select(MapRole).ToList();
    }

    public async Task<RoleResponse?> GetRoleAsync(Guid id, CancellationToken ct)
    {
        var roleId = RoleId.From(id);
        var entity = await _db.Roles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == roleId, ct);
        return entity is null ? null : MapRole(entity);
    }

    public async Task<RoleResponse> GrantPermissionAsync(Guid roleIdValue, Guid permissionIdValue, CancellationToken ct)
    {
        var roleId = RoleId.From(roleIdValue);
        var permissionId = PermissionId.From(permissionIdValue);

        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == roleId, ct)
            ?? throw new KeyNotFoundException("Role was not found.");

        if (!await _db.Permissions.AnyAsync(x => x.Id == permissionId, ct))
        {
            throw new InvalidOperationException("Permission does not exist.");
        }

        role.GrantPermission(permissionId, _clock.GetCurrentInstant());
        await _db.SaveChangesAsync(ct);
        return MapRole(role);
    }

    public async Task<RoleResponse> RevokePermissionAsync(Guid roleIdValue, Guid permissionIdValue, CancellationToken ct)
    {
        var roleId = RoleId.From(roleIdValue);
        var permissionId = PermissionId.From(permissionIdValue);

        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == roleId, ct)
            ?? throw new KeyNotFoundException("Role was not found.");

        role.RevokePermission(permissionId);
        await _db.SaveChangesAsync(ct);
        return MapRole(role);
    }

    private static PermissionResponse MapPermission(PermissionEntity p) => new()
    {
        Id = p.Id.Value,
        Code = p.Code,
        DisplayName = p.DisplayName,
        CreatedAt = p.CreatedAt
    };

    private static RoleResponse MapRole(RoleEntity r) => new()
    {
        Id = r.Id.Value,
        Code = r.Code,
        DisplayName = r.DisplayName,
        CreatedAt = r.CreatedAt,
        PermissionIds = r.Permissions.Select(x => x.PermissionId.Value).ToList()
    };
}
