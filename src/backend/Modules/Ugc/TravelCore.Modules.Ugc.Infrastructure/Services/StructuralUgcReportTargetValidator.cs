using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Services;

/// <summary>
/// Structural UgcReport target validation only (P16-R7). Controlled types + non-empty id.
/// Does not query peers, hide content, or create polymorphic FK.
/// </summary>
internal sealed class StructuralUgcReportTargetValidator : IUgcReportTargetValidator
{
    public void ValidateLogicalReference(string targetType, Guid targetId) =>
        _ = UgcReportTarget.Create(targetType, targetId);
}
