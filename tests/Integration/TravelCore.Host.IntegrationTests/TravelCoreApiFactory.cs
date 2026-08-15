using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TravelCore.Host.IntegrationTests;

/// <summary>
/// Host factory for security-hygiene proofs. Enables the gated fault endpoint only.
/// </summary>
public sealed class TravelCoreApiFactory : WebApplicationFactory<Program>
{
    private readonly string _environmentName;

    public TravelCoreApiFactory(string environmentName)
    {
        _environmentName = environmentName;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment(_environmentName);
        builder.UseSetting("TravelCore:SecurityTests:MapFaultEndpoint", "true");
    }
}
