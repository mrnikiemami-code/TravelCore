using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using TravelCore.Modules.Access.Infrastructure;
using TravelCore.Modules.Identity.Infrastructure;
using TravelCore.Modules.Party.Infrastructure;
using TravelCore.Modules.ReferenceData.Infrastructure;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Modules.Media.Infrastructure;
using TravelCore.Modules.Place.Infrastructure;
using TravelCore.Modules.Content.Infrastructure;
using TravelCore.Modules.Seo.Infrastructure;
using TravelCore.Modules.Tour.Infrastructure;
using TravelCore.Modules.Pricing.Infrastructure;
using TravelCore.Modules.AgencyMarketplace.Infrastructure;
using TravelCore.Modules.Search.Infrastructure;
using TravelCore.Modules.Ugc.Infrastructure;
using TravelCore.Persistence.PostgreSql;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

public sealed class IdentityAuthHostFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:18.6")
        .WithDatabase("travelcore_identity_auth_host")
        .WithUsername("travelcore_it")
        .WithPassword("not-a-real-secret")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var identity = CreateIdentityDb();
        await IdentityMigrator.MigrateAsync(identity);

        await using var party = CreatePartyDb();
        await PartyMigrator.MigrateAsync(party);

        await using var access = CreateAccessDb();
        await AccessMigrator.MigrateAsync(access);

        await using var referenceData = CreateReferenceDataDb();
        await ReferenceDataMigrator.MigrateAsync(referenceData);

        await using var destination = CreateDestinationDb();
        await DestinationMigrator.MigrateAsync(destination);

        await using var seo = CreateSeoDb();
        await SeoMigrator.MigrateAsync(seo);

        await using var media = CreateMediaDb();
        await MediaMigrator.MigrateAsync(media);

        await using var place = CreatePlaceDb();
        await PlaceMigrator.MigrateAsync(place);

        await using var content = CreateContentDb();
        await ContentMigrator.MigrateAsync(content);

        await using var tour = CreateTourDb();
        await TourMigrator.MigrateAsync(tour);

        await using var pricing = CreatePricingDb();
        await PricingMigrator.MigrateAsync(pricing);

        await using var agencyMarketplace = CreateAgencyMarketplaceDb();
        await AgencyMarketplaceMigrator.MigrateAsync(agencyMarketplace);

        await using var search = CreateSearchDb();
        await SearchMigrator.MigrateAsync(search);

        await using var ugc = CreateUgcDb();
        await UgcMigrator.MigrateAsync(ugc);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();

    public IdentityDbContext CreateIdentityDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<IdentityDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: IdentityDbContext.SchemaName)
            .Options;
        return new IdentityDbContext(options);
    }

    public PartyDbContext CreatePartyDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<PartyDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: PartyDbContext.SchemaName)
            .Options;
        return new PartyDbContext(options);
    }

    public AccessDbContext CreateAccessDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AccessDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: AccessDbContext.SchemaName)
            .Options;
        return new AccessDbContext(options);
    }

    public ReferenceDataDbContext CreateReferenceDataDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ReferenceDataDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: ReferenceDataDbContext.SchemaName)
            .Options;
        return new ReferenceDataDbContext(options);
    }

    public DestinationDbContext CreateDestinationDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<DestinationDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: DestinationDbContext.SchemaName)
            .Options;
        return new DestinationDbContext(options);
    }

    public SeoDbContext CreateSeoDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<SeoDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: SeoDbContext.SchemaName)
            .Options;
        return new SeoDbContext(options);
    }

    public MediaDbContext CreateMediaDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<MediaDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: MediaDbContext.SchemaName)
            .Options;
        return new MediaDbContext(options);
    }

    public PlaceDbContext CreatePlaceDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<PlaceDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: PlaceDbContext.SchemaName)
            .Options;
        return new PlaceDbContext(options);
    }

    public ContentDbContext CreateContentDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ContentDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: ContentDbContext.SchemaName)
            .Options;
        return new ContentDbContext(options);
    }

    public TourDbContext CreateTourDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<TourDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: TourDbContext.SchemaName)
            .Options;
        return new TourDbContext(options);
    }

    public PricingDbContext CreatePricingDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<PricingDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: PricingDbContext.SchemaName)
            .Options;
        return new PricingDbContext(options);
    }

    public AgencyMarketplaceDbContext CreateAgencyMarketplaceDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AgencyMarketplaceDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: AgencyMarketplaceDbContext.SchemaName)
            .Options;
        return new AgencyMarketplaceDbContext(options);
    }

    public SearchDbContext CreateSearchDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<SearchDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: SearchDbContext.SchemaName)
            .Options;
        return new SearchDbContext(options);
    }

    public UgcDbContext CreateUgcDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<UgcDbContext>()
            .UseTravelCorePostgreSql(ConnectionString, migrationsHistorySchema: UgcDbContext.SchemaName)
            .Options;
        return new UgcDbContext(options);
    }

    public TravelCoreApiFactory CreateFactory(string environmentName) =>
        new(environmentName, ConnectionString);
}

[CollectionDefinition(nameof(IdentityAuthHostCollection), DisableParallelization = true)]
public sealed class IdentityAuthHostCollection : ICollectionFixture<IdentityAuthHostFixture>;

public sealed class TravelCoreApiFactory : WebApplicationFactory<Program>
{
    private readonly string _environmentName;
    private readonly string? _connectionString;

    public TravelCoreApiFactory(string environmentName, string? connectionString = null)
    {
        _environmentName = environmentName;
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment(_environmentName);
        builder.UseSetting("TravelCore:SecurityTests:MapFaultEndpoint", "true");

        if (!string.IsNullOrWhiteSpace(_connectionString))
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [$"ConnectionStrings:{TravelCoreConnectionStrings.TravelCore}"] = _connectionString,
                    ["Media:ObjectStorage:UseInMemory"] = "true"
                });
            });
        }
    }
}
