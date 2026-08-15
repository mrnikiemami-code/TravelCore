using Microsoft.EntityFrameworkCore;
using NodaTime;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Access.Infrastructure.Services;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Party.Contracts;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

[Collection(nameof(AccessEvaluationCollection))]
public sealed class AccessSubjectAssignmentTests
{
    private readonly AccessEvaluationContainerFixture _postgres;

    public AccessSubjectAssignmentTests(AccessEvaluationContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Assign_IdentityAndParty_Then_EvaluateAllow_And_MissingSubjectConflicts()
    {
        var ct = TestContext.Current.CancellationToken;
        var identityId = Guid.CreateVersion7();
        var partyId = Guid.CreateVersion7();
        var accounts = new StubAccountExistence(identityId);
        var parties = new StubPartyExistence(partyId);

        await using (var db = _postgres.CreateDbContext())
        {
            await AccessMigrator.MigrateAsync(db, ct);
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(db, SystemClock.Instance, ct);
        }

        Guid adminRoleId;
        await using (var db = _postgres.CreateDbContext())
        {
            var admin = await db.Roles.SingleAsync(x => x.Code == AccessPermissionCatalog.AdminRoleCode, ct);
            adminRoleId = admin.Id.Value;

            var svc = new AccessSubjectAssignmentService(db, accounts, parties, SystemClock.Instance);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                svc.AssignAsync(new AssignSubjectRoleRequest
                {
                    SubjectType = "Identity",
                    SubjectId = Guid.CreateVersion7(),
                    RoleId = adminRoleId
                }, ct));

            var identityAssignment = await svc.AssignAsync(new AssignSubjectRoleRequest
            {
                SubjectType = "Identity",
                SubjectId = identityId,
                RoleId = adminRoleId
            }, ct);
            Assert.Equal("Identity", identityAssignment.SubjectType);
            Assert.Equal(identityId, identityAssignment.SubjectId);
            Assert.Equal(adminRoleId, identityAssignment.RoleId);

            var partyAssignment = await svc.AssignAsync(new AssignSubjectRoleRequest
            {
                SubjectType = "Party",
                SubjectId = partyId,
                RoleId = adminRoleId
            }, ct);
            Assert.Equal("Party", partyAssignment.SubjectType);

            var listed = await svc.ListAsync("Identity", identityId, ct);
            Assert.Single(listed);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var evaluator = new AccessAuthorizationEvaluator(db);

            var deniedUnassigned = await evaluator.EvaluateAsync(new EvaluateAccessRequest
            {
                SubjectType = "Identity",
                SubjectId = Guid.CreateVersion7(),
                PermissionCode = "access.roles.read"
            }, ct);
            Assert.False(deniedUnassigned.Allowed);

            var allowedIdentity = await evaluator.EvaluateAsync(new EvaluateAccessRequest
            {
                SubjectType = "Identity",
                SubjectId = identityId,
                PermissionCode = "access.roles.read"
            }, ct);
            Assert.True(allowedIdentity.Allowed);
            Assert.Equal("Allow", allowedIdentity.Decision);

            var allowedParty = await evaluator.EvaluateAsync(new EvaluateAccessRequest
            {
                SubjectType = "Party",
                SubjectId = partyId,
                PermissionCode = "access.roles.read"
            }, ct);
            Assert.True(allowedParty.Allowed);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var svc = new AccessSubjectAssignmentService(db, accounts, parties, SystemClock.Instance);
            await svc.RevokeAsync("Identity", identityId, adminRoleId, ct);
            var afterRevoke = await svc.ListAsync("Identity", identityId, ct);
            Assert.Empty(afterRevoke);
        }
    }

    private sealed class StubAccountExistence(Guid knownId) : IAccountExistenceQuery
    {
        public Task<bool> ExistsAsync(Guid accountId, CancellationToken cancellationToken = default)
            => Task.FromResult(accountId == knownId);
    }

    private sealed class StubPartyExistence(Guid knownId) : IPartyExistenceQuery
    {
        public Task<bool> ExistsAsync(Guid partyId, CancellationToken cancellationToken = default)
            => Task.FromResult(partyId == knownId);
    }
}
