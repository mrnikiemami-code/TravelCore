using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;

namespace TravelCore.Modules.Access.Infrastructure.Services;

/// <summary>
/// Evaluates whether a permission is allowed given Access taxonomy (and optional RoleIds).
/// Subject→role assignment persistence/API is T007 — without RoleIds, decision is deny.
/// </summary>
public sealed class AccessAuthorizationEvaluator : IAccessAuthorizationEvaluator
{
    private readonly AccessDbContext _db;

    public AccessAuthorizationEvaluator(AccessDbContext db)
    {
        _db = db;
    }

    public async Task<EvaluateAccessResponse> EvaluateAsync(
        EvaluateAccessRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.PermissionCode);

        var permissionCode = request.PermissionCode.Trim().ToLowerInvariant();
        if (permissionCode.Length > Permission.CodeMaxLength)
        {
            return Deny(permissionCode, "Permission code is invalid.");
        }

        // Subject assignment is T007. Subject fields alone never grant access in T006.
        if (request.RoleIds is null || request.RoleIds.Count == 0)
        {
            return Deny(
                permissionCode,
                "Deny-by-default: no RoleIds provided and subject assignments are not owned by T006.");
        }

        var roleIds = request.RoleIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Select(RoleId.From)
            .ToArray();

        if (roleIds.Length == 0)
        {
            return Deny(permissionCode, "Deny-by-default: no valid RoleIds.");
        }

        var permission = await _db.Permissions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == permissionCode, cancellationToken);
        if (permission is null)
        {
            return Deny(permissionCode, "Permission is unknown.");
        }

        var allowed = await _db.RolePermissions.AsNoTracking()
            .AnyAsync(
                x => roleIds.Contains(x.RoleId) && x.PermissionId.Equals(permission.Id),
                cancellationToken);

        if (!allowed)
        {
            return Deny(permissionCode, "Roles do not grant the requested permission.");
        }

        return new EvaluateAccessResponse
        {
            Allowed = true,
            PermissionCode = permissionCode,
            Decision = "Allow",
            Reason = "Granted via Role→Permission taxonomy."
        };
    }

    private static EvaluateAccessResponse Deny(string permissionCode, string reason) => new()
    {
        Allowed = false,
        PermissionCode = permissionCode,
        Decision = "Deny",
        Reason = reason
    };
}
