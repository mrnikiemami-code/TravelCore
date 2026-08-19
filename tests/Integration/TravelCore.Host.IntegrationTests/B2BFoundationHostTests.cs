using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.B2B.Infrastructure;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class B2BFoundationHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public B2BFoundationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Host_Registers_B2BDbContext_Without_B2B_Endpoints()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<B2BDbContext>();
            Assert.Equal("b2b", B2BDbContext.SchemaName);
            Assert.Equal("b2b", db.Model.GetDefaultSchema());
            Assert.False(db.Database.HasPendingModelChanges());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/b2b", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var publicList = await client.GetAsync("/api/b2b/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, publicList.StatusCode);
        using var post = await client.PostAsync("/api/b2b", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
    }
}
