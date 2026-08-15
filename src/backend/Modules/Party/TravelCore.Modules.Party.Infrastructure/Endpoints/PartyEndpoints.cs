using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Party.Contracts;
using TravelCore.Modules.Party.Infrastructure.Services;

namespace TravelCore.Modules.Party.Infrastructure.Endpoints;

internal static class PartyEndpoints
{
    public static IEndpointRouteBuilder MapPartyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/party/parties")
            .WithTags("Party");

        group.MapPost("/", async Task<IResult> (
            CreatePartyRequest request,
            PartyApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/party/parties/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        });

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            PartyApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var party = await service.GetByIdAsync(id, cancellationToken);
            return party is null ? Results.NotFound() : Results.Ok(party);
        });

        group.MapGet("/", async Task<IResult> (
            string? q,
            string? kind,
            int? skip,
            int? take,
            PartyApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await service.SearchAsync(
                    new SearchPartiesRequest
                    {
                        Query = q,
                        Kind = kind,
                        Skip = skip ?? 0,
                        Take = take ?? 20
                    },
                    cancellationToken);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        });

        return endpoints;
    }
}
