using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.ReferenceData.Contracts;

namespace TravelCore.Modules.ReferenceData.Infrastructure.Endpoints;

internal static class ReferenceDataEndpoints
{
    public static IEndpointRouteBuilder MapReferenceDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/reference-data")
            .WithTags("ReferenceData");

        group.MapGet("/currencies", async Task<IResult> (IReferenceDataCatalogQuery query, CancellationToken cancellationToken) =>
            Results.Ok(await query.ListCurrenciesAsync(cancellationToken)));

        group.MapGet("/currencies/{code}", async Task<IResult> (string code, IReferenceDataCatalogQuery query, CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await query.GetCurrencyAsync(code, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "code"] = [ex.Message]
                });
            }
        });

        group.MapGet("/locales", async Task<IResult> (IReferenceDataCatalogQuery query, CancellationToken cancellationToken) =>
            Results.Ok(await query.ListLocalesAsync(cancellationToken)));

        group.MapGet("/locales/{code}", async Task<IResult> (string code, IReferenceDataCatalogQuery query, CancellationToken cancellationToken) =>
        {
            var item = await query.GetLocaleAsync(code, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        group.MapGet("/countries", async Task<IResult> (IReferenceDataCatalogQuery query, CancellationToken cancellationToken) =>
            Results.Ok(await query.ListCountriesAsync(cancellationToken)));

        group.MapGet("/countries/{alpha2}", async Task<IResult> (string alpha2, IReferenceDataCatalogQuery query, CancellationToken cancellationToken) =>
        {
            var item = await query.GetCountryAsync(alpha2, cancellationToken);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });

        group.MapGet("/time-zones", async Task<IResult> (IReferenceDataCatalogQuery query, CancellationToken cancellationToken) =>
            Results.Ok(await query.ListTimeZonesAsync(cancellationToken)));

        group.MapGet("/time-zones/{*id}", async Task<IResult> (string id, IReferenceDataCatalogQuery query, CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await query.GetTimeZoneAsync(id, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "id"] = [ex.Message]
                });
            }
        });

        return endpoints;
    }
}
