using TravelCore.ApiFoundation;
using TravelCore.Health;
using TravelCore.Modularity;
using TravelCore.Modules.Access.Infrastructure;
using TravelCore.Modules.Destination.Infrastructure;
using TravelCore.Modules.Identity.Infrastructure;
using TravelCore.Modules.Party.Infrastructure;
using TravelCore.Modules.ReferenceData.Infrastructure;
using TravelCore.Observability;

var builder = WebApplication.CreateBuilder(args);

// Kestrel fingerprinting: do not emit the Server identification header.
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
});

builder.Services.AddTravelCoreApiFoundation();
builder.Services.AddTravelCoreObservability();
builder.Services.AddTravelCoreHealth();
// Validation metadata is assembly-scoped; call AddValidation in the endpoint-owning host (Api).
builder.Services.AddValidation();

// Explicit module composition list (compile-time / host-owned). Order is deterministic.
IReadOnlyList<ITravelCoreModule> modules =
[
    new IdentityModule(),
    new AccessModule(),
    new PartyModule(),
    new ReferenceDataModule(),
    new DestinationModule(),
];
builder.Services.AddTravelCoreModules(builder.Configuration, modules);

var app = builder.Build();

app.UseTravelCoreApiFoundation();
// Correlation early so downstream handlers/logs see CorrelationId / TraceId scope.
app.UseTravelCoreObservability();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapTravelCoreHealth();
app.MapTravelCoreModules(modules);

// Test-only fault endpoint — enabled solely by host test infrastructure (never default config).
if (app.Configuration.GetValue("TravelCore:SecurityTests:MapFaultEndpoint", defaultValue: false))
{
    app.MapGet("/__security_test/fault", () =>
    {
        throw new InvalidOperationException("intentional-security-test-fault");
    });
}

app.Run();

public partial class Program;
