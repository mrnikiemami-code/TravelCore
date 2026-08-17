using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Pricing.Contracts;

namespace TravelCore.Modules.Pricing.Infrastructure.Endpoints;

/// <summary>
/// Admin Pricing HTTP surface (TC-P12-T006 / P12-R6). Pricing-owned; not Tour Admin.
/// Mutations require Access.Pricing.Prices.Write.
/// </summary>
internal static class PricingAdminEndpoints
{
    private const string PricesReadPolicy = "Access.Pricing.Prices.Read";
    private const string PricesWritePolicy = "Access.Pricing.Prices.Write";

    public static IEndpointRouteBuilder MapPricingAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/pricing/prices")
            .WithTags("Pricing");

        group.MapPost("/", async Task<IResult> (
            CreatePriceRequest request,
            IPriceAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateAsync(request, cancellationToken);
                return Results.Created($"/api/pricing/prices/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(PricesWritePolicy);

        group.MapGet("/", async Task<IResult> (
            string? targetType,
            Guid? targetId,
            int? take,
            IPriceAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var list = await service.ListAsync(targetType, targetId, take ?? 50, cancellationToken);
                return Results.Ok(list);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PricesReadPolicy);

        group.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            IPriceAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.GetAsync(id, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(PricesReadPolicy);

        group.MapPut("/{id:guid}", async Task<IResult> (
            Guid id,
            UpdatePriceRequest request,
            IPriceAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.UpdateAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(PricesWritePolicy);

        group.MapPost("/{id:guid}/components", async Task<IResult> (
            Guid id,
            PriceComponentInput request,
            IPriceAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.AddComponentAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(PricesWritePolicy);

        group.MapPut("/{id:guid}/components", async Task<IResult> (
            Guid id,
            ReplacePriceComponentsRequest request,
            IPriceAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.ReplaceComponentsAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(PricesWritePolicy);

        group.MapPost("/{id:guid}/occupancy-rules", async Task<IResult> (
            Guid id,
            PriceOccupancyRuleInput request,
            IPriceAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.AddOccupancyRuleAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(PricesWritePolicy);

        group.MapPut("/{id:guid}/occupancy-rules", async Task<IResult> (
            Guid id,
            ReplacePriceOccupancyRulesRequest request,
            IPriceAdminService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var updated = await service.ReplaceOccupancyRulesAsync(id, request, cancellationToken);
                return Results.Ok(updated);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
        }).RequireAuthorization(PricesWritePolicy);

        return endpoints;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}
