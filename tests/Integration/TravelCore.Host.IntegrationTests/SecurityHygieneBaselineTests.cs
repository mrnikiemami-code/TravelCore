using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

public sealed class SecurityHygieneBaselineTests
{
    [Fact]
    public async Task Development_host_starts_and_health_live_succeeds()
    {
        await using var factory = new TravelCoreApiFactory(Environments.Development);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var ct = TestContext.Current.CancellationToken;
        var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Production_host_starts_and_health_live_succeeds()
    {
        await using var factory = new TravelCoreApiFactory(Environments.Production);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var ct = TestContext.Current.CancellationToken;
        var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Host_configures_kestrel_without_server_header_and_http_response_has_no_server_header()
    {
        await using var factory = new TravelCoreApiFactory(Environments.Production);

        var kestrel = factory.Services.GetRequiredService<IOptions<KestrelServerOptions>>().Value;
        Assert.False(kestrel.AddServerHeader);

        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var ct = TestContext.Current.CancellationToken;
        var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Server"));
        Assert.DoesNotContain(
            "Kestrel",
            response.Headers.ToString(),
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Production_unhandled_exception_does_not_expose_stack_or_source_path()
    {
        await using var factory = new TravelCoreApiFactory(Environments.Production);
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        var ct = TestContext.Current.CancellationToken;
        var response = await client.GetAsync(new Uri("/__security_test/fault", UriKind.Relative), ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType ?? "",
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain("intentional-security-test-fault", body, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidOperationException", body, StringComparison.Ordinal);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Program.cs", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TravelCore.Api", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("at TravelCore", body, StringComparison.Ordinal);

        using var doc = JsonDocument.Parse(body);
        Assert.True(doc.RootElement.TryGetProperty("status", out var status));
        Assert.Equal(500, status.GetInt32());
    }
}
