namespace TravelCore.Modules.Access.Infrastructure.Authorization;

/// <summary>
/// Authorization requirement evaluated against Access (not cookie claims).
/// </summary>
public sealed class PermissionRequirement : Microsoft.AspNetCore.Authorization.IAuthorizationRequirement
{
    public PermissionRequirement(string permissionCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);
        PermissionCode = permissionCode.Trim().ToLowerInvariant();
    }

    public string PermissionCode { get; }
}
