using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.Notification.Infrastructure;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(IdentityAuthHostCollection))]
public sealed class NotificationFoundationHostTests
{
    private readonly IdentityAuthHostFixture _fixture;

    public NotificationFoundationHostTests(IdentityAuthHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Host_Registers_NotificationDbContext_Without_Notification_Endpoints()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = _fixture.CreateFactory(Environments.Development);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            Assert.Equal("notification", NotificationDbContext.SchemaName);
            Assert.Equal("notification", db.Model.GetDefaultSchema());
            Assert.False(db.Database.HasPendingModelChanges());
        }

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        using var list = await client.GetAsync("/api/notification", ct);
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var publicList = await client.GetAsync("/api/notification/public", ct);
        Assert.Equal(HttpStatusCode.NotFound, publicList.StatusCode);
        using var post = await client.PostAsync("/api/notification", content: null, ct);
        Assert.Equal(HttpStatusCode.NotFound, post.StatusCode);
    }
}
