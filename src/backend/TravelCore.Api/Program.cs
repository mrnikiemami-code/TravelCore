using TravelCore.ApiFoundation;
using TravelCore.Health;
using TravelCore.Modularity;
using TravelCore.Observability;
using TravelCore.Time;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTravelCoreApiFoundation();
builder.Services.AddTravelCoreTime();
builder.Services.AddTravelCoreObservability();
builder.Services.AddTravelCoreHealth();

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
