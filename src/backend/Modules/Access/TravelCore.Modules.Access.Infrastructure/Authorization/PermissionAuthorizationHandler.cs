using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Infrastructure.Services;

namespace TravelCore.Modules.Access.Infrastructure.Authorization;

/// <summary>
/// Server-side Access-backed permission check for authenticated Identity subjects.
/// Cookie authentication alone never grants permissions.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAccessAuthorizationEvaluator _evaluator;

    public PermissionAuthorizationHandler(IAccessAuthorizationEvaluator evaluator)
    {
        _evaluator = evaluator;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var idValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var accountId) || accountId == Guid.Empty)
        {
            return;
        }

        var decision = await _evaluator.EvaluateAsync(new EvaluateAccessRequest
        {
            SubjectType = "Identity",
            SubjectId = accountId,
            PermissionCode = requirement.PermissionCode
        });

        if (decision.Allowed)
        {
            context.Succeed(requirement);
        }
    }
}
