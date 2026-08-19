using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.Analytics.Infrastructure;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class AnalyticsFoundationHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public AnalyticsFoundationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Host_Registers_AnalyticsDbContext_Without_Analytics_Endpoints()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
            Assert.Equal("analytics", AnalyticsDbContext.SchemaName);
            Assert.Equal("analytics", db.Model.GetDefaultSchema());
            Assert.False(db.Database.HasPendingModelChanges());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/analytics", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var publicList = await client.GetAsync("/api/analytics/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, publicList.StatusCode);
        using var post = await client.PostAsync("/api/analytics", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
    }
}
