using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;

namespace TravelCore.Modules.Ugc.Infrastructure.Services;

/// <summary>
/// Structural Comment target validation only (P16-R6). Review/Travelogue + non-empty id.
/// No peer-module queries or polymorphic FK.
/// </summary>
internal sealed class StructuralCommentTargetValidator : ICommentTargetValidator
{
    public void ValidateLogicalReference(string targetType, Guid targetId) =>
        _ = CommentTarget.Create(targetType, targetId);
}
