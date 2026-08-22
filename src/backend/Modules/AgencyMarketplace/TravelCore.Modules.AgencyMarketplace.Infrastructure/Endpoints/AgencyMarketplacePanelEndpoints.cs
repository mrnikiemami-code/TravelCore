using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.Identity.Contracts;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Endpoints;

/// <summary>
/// Agency Marketplace panel HTTP surface (TC-P13-T006 / P13-R6; P38-T007 ownership).
/// Publication/moderation is Marketplace-owned — not SEO.
/// Agency write/read offer ops bind to the acting account's associated Party → AgencyProfile.
/// </summary>
internal static class AgencyMarketplacePanelEndpoints
{
    private const string ProfileReadPolicy = "Access.AgencyMarketplace.Profile.Read";
    private const string ProfileWritePolicy = "Access.AgencyMarketplace.Profile.Write";
    private const string OffersReadPolicy = "Access.AgencyMarketplace.Offers.Read";
    private const string OffersWritePolicy = "Access.AgencyMarketplace.Offers.Write";
    private const string OffersModeratePolicy = "Access.AgencyMarketplace.Offers.Moderate";

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

        profiles.MapGet("/me", async Task<IResult> (
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            var acting = await ResolveActingProfileAsync(httpContext, associations, service, cancellationToken);
            return acting.Result ?? Results.Ok(acting.Profile);
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
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            var acting = await ResolveActingProfileAsync(httpContext, associations, service, cancellationToken);
            if (acting.Result is not null)
            {
                return acting.Result;
            }

            if (request.AgencyProfileId != Guid.Empty && request.AgencyProfileId != acting.Profile!.Id)
            {
                return Results.Forbid();
            }

            try
            {
                var ownedRequest = request with { AgencyProfileId = acting.Profile!.Id };
                var created = await service.CreateOfferAsync(ownedRequest, cancellationToken);
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
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            var acting = await ResolveActingProfileAsync(httpContext, associations, service, cancellationToken);
            if (acting.Result is not null)
            {
                return acting.Result;
            }

            if (agencyProfileId is Guid requested && requested != acting.Profile!.Id)
            {
                return Results.Forbid();
            }

            try
            {
                var list = await service.ListOffersAsync(acting.Profile!.Id, cancellationToken);
                return Results.Ok(list);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(OffersReadPolicy);

        offers.MapGet("/{id:guid}", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
        {
            var acting = await ResolveActingProfileAsync(httpContext, associations, service, cancellationToken);
            if (acting.Result is not null)
            {
                return acting.Result;
            }

            try
            {
                await service.EnsureOfferOwnedByAgencyAsync(id, acting.Profile!.Id, cancellationToken);
                var item = await service.GetOfferAsync(id, cancellationToken);
                return item is null ? Results.NotFound() : Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        }).RequireAuthorization(OffersReadPolicy);

        offers.MapPost("/{id:guid}/activate", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.ActivateOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/list", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.ListOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/open-sales", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.OpenOfferSalesAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/close-sales", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.CloseOfferSalesAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/submit", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.SubmitOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/publish", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.PublishOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/unpublish", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.UnpublishOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/suspend", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.SuspendOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        offers.MapPost("/{id:guid}/retire", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService service,
            CancellationToken cancellationToken) =>
            await MutateOwnedOffer(id, httpContext, associations, service, service.RetireOfferAsync, cancellationToken))
            .RequireAuthorization(OffersWritePolicy);

        // Platform moderation — Moderate policy + self-moderation denial (P38-T010).
        // Prefer /api/agency-marketplace/moderation/offers for Admin review queue + suspend.
        offers.MapPost("/{id:guid}/approve", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService panel,
            IAgencyOfferGovernanceService governance,
            CancellationToken cancellationToken) =>
            await MutateModeration(
                id,
                httpContext,
                associations,
                panel,
                (offerId, acting, actor, ct) => governance.ApproveOfferAsync(offerId, acting, actor, ct),
                cancellationToken))
            .RequireAuthorization(OffersModeratePolicy);

        offers.MapPost("/{id:guid}/reject", async Task<IResult> (
            Guid id,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService panel,
            IAgencyOfferGovernanceService governance,
            CancellationToken cancellationToken) =>
            await MutateModeration(
                id,
                httpContext,
                associations,
                panel,
                (offerId, acting, actor, ct) => governance.RejectOfferAsync(offerId, acting, actor, ct),
                cancellationToken))
            .RequireAuthorization(OffersModeratePolicy);

        return endpoints;
    }

    private static async Task<IResult> MutateModeration(
        Guid id,
        HttpContext httpContext,
        IAccountAssociationQuery associations,
        IAgencyMarketplacePanelService panel,
        Func<Guid, Guid?, Guid?, CancellationToken, Task<AgencyOfferModerationQueueItem>> action,
        CancellationToken cancellationToken)
    {
        try
        {
            var actingProfileId = await AgencyMarketplaceAdminEndpoints.TryResolveActingAgencyProfileIdAsync(
                httpContext,
                associations,
                panel,
                cancellationToken);
            var actorAccountId = AgencyMarketplaceAdminEndpoints.TryResolveAccountId(httpContext);
            await action(id, actingProfileId, actorAccountId, cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
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

    private static async Task<IResult> MutateOwnedOffer(
        Guid id,
        HttpContext httpContext,
        IAccountAssociationQuery associations,
        IAgencyMarketplacePanelService service,
        Func<Guid, CancellationToken, Task> action,
        CancellationToken cancellationToken)
    {
        var acting = await ResolveActingProfileAsync(httpContext, associations, service, cancellationToken);
        if (acting.Result is not null)
        {
            return acting.Result;
        }

        try
        {
            await service.EnsureOfferOwnedByAgencyAsync(id, acting.Profile!.Id, cancellationToken);
            await action(id, cancellationToken);
            return Results.NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Forbid();
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

    private static async Task<(AgencyProfilePanelResponse? Profile, IResult? Result)> ResolveActingProfileAsync(
        HttpContext httpContext,
        IAccountAssociationQuery associations,
        IAgencyMarketplacePanelService service,
        CancellationToken cancellationToken)
    {
        var idValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var accountId) || accountId == Guid.Empty)
        {
            return (null, Results.Unauthorized());
        }

        var partyId = await associations.GetAssociatedPartyIdAsync(accountId, cancellationToken);
        if (partyId is null)
        {
            return (null, Results.Conflict(new
            {
                title = "Conflict",
                detail = "Authenticated account has no associated Agency Party."
            }));
        }

        var profile = await service.GetProfileByPartyAsync(partyId.Value, cancellationToken);
        if (profile is null)
        {
            return (null, Results.Conflict(new
            {
                title = "Conflict",
                detail = "Acting Agency Party has no AgencyProfile yet."
            }));
        }

        return (profile, null);
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "request"] = [ex.Message]
        });
}
