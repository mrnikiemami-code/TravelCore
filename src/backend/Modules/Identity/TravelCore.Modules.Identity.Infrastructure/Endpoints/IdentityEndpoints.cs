using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Identity.Contracts;
using TravelCore.Modules.Identity.Infrastructure.Services;

namespace TravelCore.Modules.Identity.Infrastructure.Endpoints;

internal static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/identity/accounts")
            .WithTags("Identity");

        group.MapPost("/", async Task<IResult> (
            CreateAccountRequest request,
            IdentityApplicationService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/identity/accounts/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = "Conflict", detail = ex.Message });
            }
        });

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            IdentityApplicationService service,
            CancellationToken cancellationToken) =>
        {
            var account = await service.GetStatusByIdAsync(id, cancellationToken);
            return account is null ? Results.NotFound() : Results.Ok(account);
        });

        return endpoints;
    }
}
