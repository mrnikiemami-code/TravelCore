using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Party.Contracts;

namespace TravelCore.Modules.Access.Infrastructure.Services;

public sealed class AccessSubjectAssignmentService
{
    private readonly AccessDbContext _db;
    private readonly IAccountExistenceQuery _accounts;
    private readonly IPartyExistenceQuery _parties;
    private readonly IClock _clock;

    public AccessSubjectAssignmentService(
        AccessDbContext db,
        IAccountExistenceQuery accounts,
        IPartyExistenceQuery parties,
        IClock clock)
    {
        _db = db;
        _accounts = accounts;
        _parties = parties;
        _clock = clock;
    }

    public async Task<SubjectRoleAssignmentResponse> AssignAsync(
        AssignSubjectRoleRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var kind = ParseKind(request.SubjectType);
        await EnsureSubjectExistsAsync(kind, request.SubjectId, cancellationToken);

        var roleId = RoleId.From(request.RoleId);
        if (!await _db.Roles.AnyAsync(x => x.Id == roleId, cancellationToken))
        {
            throw new InvalidOperationException("Role does not exist.");
        }

        var existing = await _db.SubjectRoleAssignments.FirstOrDefaultAsync(
            x => x.SubjectKind == kind && x.SubjectId == request.SubjectId && x.RoleId == roleId,
            cancellationToken);
        if (existing is not null)
        {
            return Map(existing);
        }

        var assignment = new SubjectRoleAssignment(kind, request.SubjectId, roleId, _clock.GetCurrentInstant());
        _db.SubjectRoleAssignments.Add(assignment);
        await _db.SaveChangesAsync(cancellationToken);
        return Map(assignment);
    }

    public async Task RevokeAsync(
        string subjectType,
        Guid subjectId,
        Guid roleIdValue,
        CancellationToken cancellationToken)
    {
        var kind = ParseKind(subjectType);
        var roleId = RoleId.From(roleIdValue);
        var existing = await _db.SubjectRoleAssignments.FirstOrDefaultAsync(
            x => x.SubjectKind == kind && x.SubjectId == subjectId && x.RoleId == roleId,
            cancellationToken);
        if (existing is null)
        {
            throw new KeyNotFoundException("Assignment was not found.");
        }

        _db.SubjectRoleAssignments.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SubjectRoleAssignmentResponse>> ListAsync(
        string subjectType,
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        var kind = ParseKind(subjectType);
        var items = await _db.SubjectRoleAssignments.AsNoTracking()
            .Where(x => x.SubjectKind == kind && x.SubjectId == subjectId)
            .OrderBy(x => x.AssignedAt)
            .ToListAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    private async Task EnsureSubjectExistsAsync(
        AccessSubjectKind kind,
        Guid subjectId,
        CancellationToken cancellationToken)
    {
        var exists = kind switch
        {
            AccessSubjectKind.Identity => await _accounts.ExistsAsync(subjectId, cancellationToken),
            AccessSubjectKind.Party => await _parties.ExistsAsync(subjectId, cancellationToken),
            _ => false
        };

        if (!exists)
        {
            throw new InvalidOperationException($"{kind} subject does not exist.");
        }
    }

    private static AccessSubjectKind ParseKind(string subjectType)
    {
        if (Enum.TryParse<AccessSubjectKind>(subjectType, ignoreCase: true, out var kind)
            && Enum.IsDefined(kind))
        {
            return kind;
        }

        throw new ArgumentException("SubjectType must be Identity or Party.", nameof(subjectType));
    }

    private static SubjectRoleAssignmentResponse Map(SubjectRoleAssignment a) => new()
    {
        SubjectType = a.SubjectKind.ToString(),
        SubjectId = a.SubjectId,
        RoleId = a.RoleId.Value,
        AssignedAt = a.AssignedAt
    };
}
