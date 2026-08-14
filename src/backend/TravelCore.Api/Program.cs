using TravelCore.Modularity;

var builder = WebApplication.CreateBuilder(args);

// Explicit module composition list (compile-time / host-owned).
// Add future modules here deliberately — no assembly scanning.
IReadOnlyList<ITravelCoreModule> modules = [];

builder.Services.AddTravelCoreModules(builder.Configuration, modules);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapTravelCoreModules(modules);

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
