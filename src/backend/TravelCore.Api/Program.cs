using TravelCore.ApiFoundation;
using TravelCore.Health;
using TravelCore.Modularity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTravelCoreApiFoundation();
builder.Services.AddTravelCoreHealth();

// Explicit module composition list (compile-time / host-owned).
IReadOnlyList<ITravelCoreModule> modules = [];
builder.Services.AddTravelCoreModules(builder.Configuration, modules);

var app = builder.Build();

app.UseTravelCoreApiFoundation();
app.UseHttpsRedirection();
app.MapTravelCoreHealth();
app.MapTravelCoreModules(modules);

app.Run();
