namespace TravelCore.Modules.Tour.Contracts;

/// <summary>
/// Experience guide assignments (TC-P10-T006 / P10-R7).
/// GuidePartyId is a logical Party person Guid — validated via Party.Contracts; no cross-schema FK.
/// </summary>
public sealed record ExperienceGuideAssignmentDto(
    Guid Id,
    Guid GuidePartyId,
    string Role,
    string? Note);

public sealed record ExperienceGuideAssignmentsResponse(
    Guid TourProductId,
    IReadOnlyList<ExperienceGuideAssignmentDto> Assignments);

public sealed record AddExperienceGuideAssignmentRequest(
    Guid GuidePartyId,
    string Role,
    string? Note = null);

public interface IExperienceGuideAssignmentService
{
    Task<ExperienceGuideAssignmentsResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default);

    Task<ExperienceGuideAssignmentsResponse> AddAsync(
        Guid tourProductId,
        AddExperienceGuideAssignmentRequest request,
        CancellationToken cancellationToken = default);

    Task<ExperienceGuideAssignmentsResponse> RemoveAsync(
        Guid tourProductId,
        Guid assignmentId,
        CancellationToken cancellationToken = default);
}
