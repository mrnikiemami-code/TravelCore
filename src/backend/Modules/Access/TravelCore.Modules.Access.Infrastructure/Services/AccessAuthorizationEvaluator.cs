using Microsoft.EntityFrameworkCore;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;

namespace TravelCore.Modules.Access.Infrastructure.Services;

/// <summary>
/// Evaluates whether a permission is allowed. Deny-by-default.
/// Uses subject→role assignments (T007) and/or explicit RoleIds for probes.
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

        var roleIds = new HashSet<RoleId>();

        if (request.RoleIds is { Count: > 0 })
        {
            foreach (var id in request.RoleIds.Where(x => x != Guid.Empty).Distinct())
            {
                roleIds.Add(RoleId.From(id));
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SubjectType) && request.SubjectId is Guid subjectId && subjectId != Guid.Empty)
        {
            if (Enum.TryParse<AccessSubjectKind>(request.SubjectType, ignoreCase: true, out var kind)
                && Enum.IsDefined(kind))
            {
                var assigned = await _db.SubjectRoleAssignments.AsNoTracking()
                    .Where(x => x.SubjectKind == kind && x.SubjectId == subjectId)
                    .Select(x => x.RoleId)
                    .ToListAsync(cancellationToken);
                foreach (var roleId in assigned)
                {
                    roleIds.Add(roleId);
                }
            }
        }

        if (roleIds.Count == 0)
        {
            return Deny(permissionCode, "Deny-by-default: no roles resolved for evaluation.");
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
            return Deny(permissionCode, "Resolved roles do not grant the requested permission.");
        }

        return new EvaluateAccessResponse
        {
            Allowed = true,
            PermissionCode = permissionCode,
            Decision = "Allow",
            Reason = "Granted via subject/role assignment and Role→Permission taxonomy."
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
