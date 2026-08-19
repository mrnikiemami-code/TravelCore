using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.DynamicPackage.Infrastructure;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class DynamicPackageFoundationHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public DynamicPackageFoundationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Host_Registers_DynamicPackageDbContext_Without_DynamicPackage_Endpoints()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DynamicPackageDbContext>();
            Assert.Equal("dynamic_package", DynamicPackageDbContext.SchemaName);
            Assert.Equal("dynamic_package", db.Model.GetDefaultSchema());
            Assert.False(db.Database.HasPendingModelChanges());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/dynamic-package", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var publicList = await client.GetAsync("/api/dynamic-package/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, publicList.StatusCode);
        using var post = await client.PostAsync("/api/dynamic-package", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
        using var search = await client.GetAsync("/api/dynamic-package/search", ct);
        Assert.Equal(HttpStatusCode.NotFound, search.StatusCode);
        using var book = await client.PostAsync("/api/dynamic-package/bookings", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, book.StatusCode);
    }
}
