using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Party.Contracts;
using TravelCore.Modules.Tour.Contracts;
using TravelCore.Modules.Tour.Domain;

namespace TravelCore.Modules.Tour.Infrastructure.Services;

/// <summary>
/// Experience guide assignment mutations with Party.Contracts Person validation (TC-P10-T006 / P10-R7).
/// </summary>
public sealed class ExperienceGuideAssignmentService : IExperienceGuideAssignmentService
{
    public const string PersonKind = "Person";

    private readonly TourDbContext _db;
    private readonly IPartyReadQuery _parties;
    private readonly IClock _clock;

    public ExperienceGuideAssignmentService(
        TourDbContext db,
        IPartyReadQuery parties,
        IClock clock)
    {
        _db = db;
        _parties = parties;
        _clock = clock;
    }

    public async Task<ExperienceGuideAssignmentsResponse?> GetAsync(
        Guid tourProductId,
        CancellationToken cancellationToken = default)
    {
        var specialization = await FindSpecializationAsync(tourProductId, cancellationToken);
        return specialization is null ? null : Map(specialization);
    }

    public async Task<ExperienceGuideAssignmentsResponse> AddAsync(
        Guid tourProductId,
        AddExperienceGuideAssignmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await EnsureGuidePersonExistsAsync(request.GuidePartyId, cancellationToken);
        var role = ParseRole(request.Role);

        var specialization = await LoadSpecializationAsync(tourProductId, cancellationToken);
        specialization.AddGuideAssignment(
            request.GuidePartyId,
            role,
            _clock.GetCurrentInstant(),
            request.Note);

        await _db.SaveChangesAsync(cancellationToken);
        return Map(specialization);
    }

    public async Task<ExperienceGuideAssignmentsResponse> RemoveAsync(
        Guid tourProductId,
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        var specialization = await LoadSpecializationAsync(tourProductId, cancellationToken);
        var removed = specialization.RemoveGuideAssignment(
            ExperienceGuideAssignmentId.From(assignmentId),
            _clock.GetCurrentInstant());

        if (!removed)
        {
            throw new KeyNotFoundException(
                $"Experience guide assignment '{assignmentId}' was not found on TourProduct '{tourProductId}'.");
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Map(specialization);
    }

    private async Task EnsureGuidePersonExistsAsync(
        Guid guidePartyId,
        CancellationToken cancellationToken)
    {
        if (guidePartyId == Guid.Empty)
        {
            throw new ArgumentException("GuidePartyId cannot be empty.", nameof(guidePartyId));
        }

        var party = await _parties.GetAsync(guidePartyId, cancellationToken);
        if (party is null)
        {
            throw new ArgumentException($"Guide party '{guidePartyId}' was not found.", nameof(guidePartyId));
        }

        if (!string.Equals(party.Kind, PersonKind, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Party '{guidePartyId}' kind '{party.Kind}' is not Person.",
                nameof(guidePartyId));
        }
    }

    private static ExperienceGuideRole ParseRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            throw new ArgumentException("Role is required.", nameof(role));
        }

        if (Enum.TryParse<ExperienceGuideRole>(role.Trim(), ignoreCase: true, out var parsed)
            && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Unsupported ExperienceGuideRole '{role}'. Allowed: Primary, Assistant.",
            nameof(role));
    }

    private async Task<TourExperienceSpecialization?> FindSpecializationAsync(
        Guid tourProductId,
        CancellationToken cancellationToken)
    {
        var id = TourProductId.From(tourProductId);
        return await _db.Set<TourExperienceSpecialization>()
            .FirstOrDefaultAsync(x => x.TourProductId == id, cancellationToken);
    }

    private async Task<TourExperienceSpecialization> LoadSpecializationAsync(
        Guid tourProductId,
        CancellationToken cancellationToken)
    {
        return await FindSpecializationAsync(tourProductId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"TourExperienceSpecialization '{tourProductId}' was not found.");
    }

    private static ExperienceGuideAssignmentsResponse Map(TourExperienceSpecialization specialization)
        => new(
            specialization.TourProductId.Value,
            specialization.GuideAssignments
                .OrderBy(x => x.Role)
                .ThenBy(x => x.GuidePartyId)
                .Select(x => new ExperienceGuideAssignmentDto(
                    x.Id.Value,
                    x.GuidePartyId,
                    x.Role.ToString(),
                    x.Note))
                .ToList());
}
