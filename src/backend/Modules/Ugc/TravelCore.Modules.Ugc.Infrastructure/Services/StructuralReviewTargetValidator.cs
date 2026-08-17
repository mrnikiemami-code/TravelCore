using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Services;

/// <summary>
/// Structural Review target validation only (P16-R3). Supported type + non-empty id.
/// No peer-module queries or cross-schema FK.
/// </summary>
internal sealed class StructuralReviewTargetValidator : IReviewTargetValidator
{
    public void ValidateLogicalReference(string targetType, Guid targetId) =>
        _ = ReviewTarget.Create(targetType, targetId);
}
