using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.AgencyMarketplace.Contracts;
using TravelCore.Modules.AgencyMarketplace.Infrastructure.Policies;
using TravelCore.Modules.Identity.Contracts;

namespace TravelCore.Modules.AgencyMarketplace.Infrastructure.Endpoints;

/// <summary>
/// Admin AgencyOffer governance HTTP surface (TC-P38-T010).
/// Review queue + approve/reject/suspend. Not Agency Portal. No financial engines.
/// </summary>
internal static class AgencyMarketplaceAdminEndpoints
{
    private const string OffersReadPolicy = "Access.AgencyMarketplace.Offers.Read";
    private const string OffersModeratePolicy = "Access.AgencyMarketplace.Offers.Moderate";

    public static IEndpointRouteBuilder MapAgencyMarketplaceAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/agency-marketplace/moderation/offers")
            .WithTags("AgencyMarketplace");

        group.MapGet("/pending", async Task<IResult> (
            int? take,
            IAgencyOfferGovernanceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await service.ListPendingOffersAsync(take ?? 50, cancellationToken);
                return Results.Ok(items);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
        }).RequireAuthorization(OffersReadPolicy);

        group.MapGet("/{offerId:guid}/policy-evaluation", async Task<IResult> (
            Guid offerId,
            IAgencyOfferGovernanceService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var report = await service.EvaluateOfferPoliciesAsync(offerId, cancellationToken);
                return Results.Ok(report);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "value"] = [ex.Message]
                });
            }
        }).RequireAuthorization(OffersReadPolicy);

        group.MapGet("/{offerId:guid}/governance-history", async Task<IResult> (
            Guid offerId,
            int? take,
            IAgencyOfferGovernanceAuditQuery audit,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await audit.ListByOfferAsync(offerId, take ?? 50, cancellationToken);
                return Results.Ok(items);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "value"] = [ex.Message]
                });
            }
        }).RequireAuthorization(OffersModeratePolicy);

        group.MapPost("/{offerId:guid}/approve", async Task<IResult> (
            Guid offerId,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService panel,
            IAgencyOfferGovernanceService service,
            CancellationToken cancellationToken) =>
            await Mutate(
                offerId,
                httpContext,
                associations,
                panel,
                (id, acting, actor, ct) => service.ApproveOfferAsync(id, acting, actor, ct),
                cancellationToken))
            .RequireAuthorization(OffersModeratePolicy);

        group.MapPost("/{offerId:guid}/reject", async Task<IResult> (
            Guid offerId,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService panel,
            IAgencyOfferGovernanceService service,
            CancellationToken cancellationToken) =>
            await Mutate(
                offerId,
                httpContext,
                associations,
                panel,
                (id, acting, actor, ct) => service.RejectOfferAsync(id, acting, actor, ct),
                cancellationToken))
            .RequireAuthorization(OffersModeratePolicy);

        group.MapPost("/{offerId:guid}/suspend", async Task<IResult> (
            Guid offerId,
            HttpContext httpContext,
            IAccountAssociationQuery associations,
            IAgencyMarketplacePanelService panel,
            IAgencyOfferGovernanceService service,
            CancellationToken cancellationToken) =>
            await Mutate(
                offerId,
                httpContext,
                associations,
                panel,
                (id, acting, actor, ct) => service.SuspendOfferAsync(id, acting, actor, ct),
                cancellationToken))
            .RequireAuthorization(OffersModeratePolicy);

        return endpoints;
    }

    private static async Task<IResult> Mutate(
        Guid offerId,
        HttpContext httpContext,
        IAccountAssociationQuery associations,
        IAgencyMarketplacePanelService panel,
        Func<Guid, Guid?, Guid?, CancellationToken, Task<AgencyOfferModerationQueueItem>> action,
        CancellationToken cancellationToken)
    {
        try
        {
            var actingProfileId = await TryResolveActingAgencyProfileIdAsync(
                httpContext,
                associations,
                panel,
                cancellationToken);
            var actorAccountId = TryResolveAccountId(httpContext);
            var item = await action(offerId, actingProfileId, actorAccountId, cancellationToken);
            return Results.Ok(item);
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
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [ex.ParamName ?? "value"] = [ex.Message]
            });
        }
        catch (AgencyOfferPolicyDeniedException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "AgencyOffer policy denial",
                extensions: new Dictionary<string, object?>
                {
                    ["policyCode"] = ex.Decision.Code,
                    ["policyName"] = ex.Decision.PolicyName,
                    ["policyReason"] = ex.Decision.Reason,
                    ["policyKind"] = ex.Decision.Kind.ToString()
                });
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict,
                title: "AgencyOffer governance lifecycle conflict");
        }
    }

    internal static Guid? TryResolveAccountId(HttpContext httpContext)
    {
        var idValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idValue, out var accountId) && accountId != Guid.Empty
            ? accountId
            : null;
    }

    /// <summary>
    /// Optional acting AgencyProfile for self-moderation denial.
    /// Missing Party/Profile is fine for pure Admin operators.
    /// </summary>
    internal static async Task<Guid?> TryResolveActingAgencyProfileIdAsync(
        HttpContext httpContext,
        IAccountAssociationQuery associations,
        IAgencyMarketplacePanelService panel,
        CancellationToken cancellationToken)
    {
        var idValue = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var accountId) || accountId == Guid.Empty)
        {
            return null;
        }

        var partyId = await associations.GetAssociatedPartyIdAsync(accountId, cancellationToken);
        if (partyId is null)
        {
            return null;
        }

        var profile = await panel.GetProfileByPartyAsync(partyId.Value, cancellationToken);
        return profile?.Id;
    }
}
