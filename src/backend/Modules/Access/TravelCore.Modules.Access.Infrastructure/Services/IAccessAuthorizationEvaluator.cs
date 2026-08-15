using TravelCore.Modules.Access.Contracts;

namespace TravelCore.Modules.Access.Infrastructure.Services;

/// <summary>
/// Access-owned authorization evaluation. Deny-by-default. No UI authority.
/// </summary>
public interface IAccessAuthorizationEvaluator
{
    Task<EvaluateAccessResponse> EvaluateAsync(EvaluateAccessRequest request, CancellationToken cancellationToken = default);
}
