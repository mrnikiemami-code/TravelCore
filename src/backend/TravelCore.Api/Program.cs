using TravelCore.ApiFoundation;
using TravelCore.Health;
using TravelCore.Modularity;
using TravelCore.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTravelCoreApiFoundation();
builder.Services.AddTravelCoreObservability();
builder.Services.AddTravelCoreHealth();
// Validation metadata is assembly-scoped; call AddValidation in the endpoint-owning host (Api).
builder.Services.AddValidation();

// Explicit module composition list (compile-time / host-owned).
IReadOnlyList<ITravelCoreModule> modules = [];
builder.Services.AddTravelCoreModules(builder.Configuration, modules);

var app = builder.Build();

app.UseTravelCoreApiFoundation();
// Correlation early so downstream handlers/logs see CorrelationId / TraceId scope.
app.UseTravelCoreObservability();
app.UseHttpsRedirection();
app.MapTravelCoreHealth();
app.MapTravelCoreModules(modules);

app.Run();
