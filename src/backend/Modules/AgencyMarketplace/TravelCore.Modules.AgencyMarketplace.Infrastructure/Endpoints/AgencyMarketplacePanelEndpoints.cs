using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.AgencyMarketplace.Contracts;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Endpoints;

/// <summary>
/// Agency Marketplace panel HTTP surface (TC-P13-T006 / P13-R6). Marketplace-owned; not Tour Admin.
/// </summary>
internal static class AgencyMarketplacePanelEndpoints
{
    private const string ProfileReadPolicy = "Access.AgencyMarketplace.Profile.Read";
    private const string ProfileWritePolicy = "Access.AgencyMarketplace.Profile.Write";
    private const string OffersReadPolicy = "Access.AgencyMarketplace.Offers.Read";
    private const string OffersWritePolicy = "Access.AgencyMarketplace.Offers.Write";

    public static IEndpointRouteBuilder MapAgencyMarketplacePanelEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var profiles = endpoints.MapGroup("/api/agency-marketplace/profiles")
            .WithTags("AgencyMarketplace");

        profiles.MapPost("/", async Task<IResult> (
            UpsertAgencyProfileRequest request,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.UpsertProfileAsync(request, cancellationToken);
                return Results.Created($"/api/agency-marketplace/profiles/{created.Id:D}", created);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ProfileWritePolicy);

        profiles.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var item = await service.GetProfileAsync(id, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ProfileReadPolicy);

        profiles.MapGet("/", async Task<IResult> (
            Guid? partyId,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            if (partyId is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["partyId"] = ["partyId query is required."]
                });
            }

            try
            {
                var item = await service.GetProfileByPartyAsync(partyId.Value, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ProfileReadPolicy);

        profiles.MapPost("/{id:guid}/activate", async Task<IResult> (
            Guid id,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                await service.ActivateProfileAsync(id, cancellationToken);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { title = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(ProfileWritePolicy);

        var offers = endpoints.MapGroup("/api/agency-marketplace/offers")
            .WithTags("AgencyMarketplace");

        offers.MapPost("/", async Task<IResult> (
            CreateAgencyOfferRequest request,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var created = await service.CreateOfferAsync(request, cancellationToken);
                return Results.Created($"/api/agency-marketplace/offers/{created.Id:D}", created);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(OffersWritePolicy);

        offers.MapGet("/", async Task<IResult> (
            Guid? agencyProfileId,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            if (agencyProfileId is null)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["agencyProfileId"] = ["agencyProfileId query is required."]
                });
            }

            try
            {
                var list = await service.ListOffersAsync(agencyProfileId.Value, cancellationToken);
                return Results.Ok(list);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(OffersReadPolicy);

        offers.MapPost("/{id:guid}/activate", async Task<IResult> (
            Guid id,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOffer(id, service.ActivateOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/list", async Task<IResult> (
            Guid id,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOffer(id, service.ListOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/open-sales", async Task<IResult> (
            Guid id,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOffer(id, service.OpenOfferSalesAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/close-sales", async Task<IResult> (
            Guid id,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOffer(id, service.CloseOfferSalesAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        return endpoints;
    }

    private static async Task<IResult> MutateOffer(
        Guid id,
        Func<Guid, CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        try
        {
            await action(id, cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { title = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Validation(ex);
        }
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}
