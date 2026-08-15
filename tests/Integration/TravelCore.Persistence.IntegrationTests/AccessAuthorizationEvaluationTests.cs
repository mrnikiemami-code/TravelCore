using Microsoft.EntityFrameworkCore;
using NodaTime;
using Testcontainers.PostgreSql;
using TravelCore.Modules.Access.Contracts;
using TravelCore.Modules.Access.Domain;
using TravelCore.Modules.Access.Infrastructure;
using TravelCore.Modules.Access.Infrastructure.Seeding;
using TravelCore.Modules.Access.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

public sealed class AccessEvaluationContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_access_evaluation")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public AccessDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AccessDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: AccessDbContext.SchemaName)
            .Options;
        return new AccessDbContext(options);
    }
}

[CollectionDefinition(nameof(AccessEvaluationCollection), DisableParallelization = true)]
public sealed class AccessEvaluationCollection : ICollectionFixture<AccessEvaluationContainerFixture>;

[Collection(nameof(AccessEvaluationCollection))]
public sealed class AccessAuthorizationEvaluationTests
{
    private readonly AccessEvaluationContainerFixture _postgres;

    public AccessAuthorizationEvaluationTests(AccessEvaluationContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task Evaluate_DenyByDefault_And_AllowViaRoleTaxonomy()
    {
        var ct = TestContext.Current.CancellationToken;

        await using (var db = _postgres.CreateDbContext())
        {
            await AccessMigrator.MigrateAsync(db, ct);
            await AccessTaxonomySeeder.SeedAdminBaselineAsync(db, SystemClock.Instance, ct);
        }

        await using (var db = _postgres.CreateDbContext())
        {
            var evaluator = new AccessAuthorizationEvaluator(db);
            var admin = await db.Roles.SingleAsync(x => x.Code == AccessPermissionCatalog.AdminRoleCode, ct);

            var deniedNoRoles = await evaluator.EvaluateAsync(new EvaluateAccessRequest
            {
                SubjectType = "Identity",
                SubjectId = Guid.CreateVersion7(),
                PermissionCode = "access.roles.read"
            }, ct);
            Assert.False(deniedNoRoles.Allowed);
            Assert.Equal("Deny", deniedNoRoles.Decision);

            var allowed = await evaluator.EvaluateAsync(new EvaluateAccessRequest
            {
                PermissionCode = "access.roles.read",
                RoleIds = [admin.Id.Value]
            }, ct);
            Assert.True(allowed.Allowed);
            Assert.Equal("Allow", allowed.Decision);

            var deniedUnknown = await evaluator.EvaluateAsync(new EvaluateAccessRequest
            {
                PermissionCode = "tour.inventory.nuke",
                RoleIds = [admin.Id.Value]
            }, ct);
            Assert.False(deniedUnknown.Allowed);
        }
    }
}
