using Microsoft.EntityFrameworkCore;
using NodaTime;
using Testcontainers.PostgreSql;
using TravelCore.Modules.Identity.Infrastructure;
using TravelCore.Modules.Identity.Infrastructure.Security;
using TravelCore.Modules.Identity.Infrastructure.Services;
using TravelCore.Modules.Party.Infrastructure;
using TravelCore.Modules.Party.Infrastructure.Services;
using TravelCore.Persistence.PostgreSql;
using AccountAggregate = TravelCore.Modules.Identity.Domain.Account;
using PartyAggregate = TravelCore.Modules.Party.Domain.Party;
using Xunit;

namespace TravelCore.Persistence.IntegrationTests;

public sealed class IdentityPartyAssociationContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_identity_party_association")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync() => await _container.StartAsync();

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public IdentityDbContext CreateIdentityDb()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: IdentityDbContext.SchemaName)
            .Options;
        return new IdentityDbContext(options);
    }

    public PartyDbContext CreatePartyDb()
    {
        var options = new DbContextOptionsBuilder<PartyDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: PartyDbContext.SchemaName)
            .Options;
        return new PartyDbContext(options);
    }
}

[CollectionDefinition(nameof(IdentityPartyAssociationCollection), DisableParallelization = true)]
public sealed class IdentityPartyAssociationCollection : ICollectionFixture<IdentityPartyAssociationContainerFixture>;

[Collection(nameof(IdentityPartyAssociationCollection))]
public sealed class IdentityPartyAssociationTests
{
    private readonly IdentityPartyAssociationContainerFixture _postgres;

    public IdentityPartyAssociationTests(IdentityPartyAssociationContainerFixture postgres)
    {
        _postgres = postgres;
    }

    [Fact]
    public async Task LinkReplaceUnlink_ViaContracts_WithoutCrossSchemaFk()
    {
        var ct = TestContext.Current.CancellationToken;
        var now = Instant.FromUtc(2026, 8, 15, 15, 0);

        await using (var partyDb = _postgres.CreatePartyDb())
        {
            await PartyMigrator.MigrateAsync(partyDb, ct);
        }

        await using (var identityDb = _postgres.CreateIdentityDb())
        {
            await IdentityMigrator.MigrateAsync(identityDb, ct);
        }

        Guid partyA;
        Guid partyB;
        await using (var partyDb = _postgres.CreatePartyDb())
        {
            var a = PartyAggregate.CreatePerson("Person A", "A", "One", now);
            var b = PartyAggregate.CreateOrganization("Org B", "Org B Legal", now);
            partyDb.Parties.AddRange(a, b);
            await partyDb.SaveChangesAsync(ct);
            partyA = a.Id.Value;
            partyB = b.Id.Value;
        }

        Guid accountId;
        await using (var identityDb = _postgres.CreateIdentityDb())
        {
            var hasher = new AspNetCoreIdentityPasswordHasher();
            var account = AccountAggregate.Create(
                "assoc@travelcore.test",
                hasher.HashPassword("Association-Test-Password-1"),
                now);
            identityDb.Accounts.Add(account);
            await identityDb.SaveChangesAsync(ct);
            accountId = account.Id.Value;
        }

        await using (var identityDb = _postgres.CreateIdentityDb())
        await using (var partyDb = _postgres.CreatePartyDb())
        {
            var service = new IdentityApplicationService(
                identityDb,
                new AspNetCoreIdentityPasswordHasher(),
                new PartyExistenceQuery(partyDb),
                SystemClock.Instance);

            var linked = await service.LinkPartyAsync(accountId, partyA, ct);
            Assert.Equal(partyA, linked.AssociatedPartyId);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.LinkPartyAsync(accountId, partyB, ct));

            var replaced = await service.ReplacePartyAsync(accountId, partyB, ct);
            Assert.Equal(partyB, replaced.AssociatedPartyId);

            var missing = Guid.CreateVersion7();
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.ReplacePartyAsync(accountId, missing, ct));

            var unlinked = await service.UnlinkPartyAsync(accountId, ct);
            Assert.Null(unlinked.AssociatedPartyId);
        }

        await using (var identityDb = _postgres.CreateIdentityDb())
        {
            var conn = identityDb.Database.GetDbConnection();
            await identityDb.Database.OpenConnectionAsync(ct);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                SELECT COUNT(*)::int
                FROM information_schema.table_constraints
                WHERE constraint_type = 'FOREIGN KEY'
                  AND table_schema = 'identity'
                  AND table_name = 'accounts';
                """;
            Assert.Equal(0, (int)(await cmd.ExecuteScalarAsync(ct) ?? -1));
        }
    }
}
