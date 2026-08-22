using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.CommercialFinance.Contracts;

namespace TravelCore.Modules.CommercialFinance.Infrastructure.Endpoints;

/// <summary>
/// Admin Commercial Finance HTTP surface (TC-P39-T006). Read-only skeleton endpoints.
/// </summary>
internal static class CommercialFinanceAdminEndpoints
{
    private const string AgreementsReadPolicy = "Access.CommercialFinance.Agreements.Read";
    private const string ObligationsReadPolicy = "Access.CommercialFinance.Obligations.Read";
    private const string SettlementsReadPolicy = "Access.CommercialFinance.Settlements.Read";
    private const string PayoutsReadPolicy = "Access.CommercialFinance.Payouts.Read";
    public static IEndpointRouteBuilder MapCommercialFinanceAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var agreements = endpoints.MapGroup("/api/commercial-finance/agreements")
            .WithTags("CommercialFinance");

        agreements.MapGet("/", async Task<IResult> (
            Guid? agencyProfileId,
            int? take,
            ICommercialFinanceAgreementQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await query.ListAgreementsAsync(agencyProfileId, take ?? 50, cancellationToken);
                return Results.Ok(items);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
        }).RequireAuthorization(AgreementsReadPolicy);

        var obligations = endpoints.MapGroup("/api/commercial-finance/obligations")
            .WithTags("CommercialFinance");

        obligations.MapGet("/", async Task<IResult> (
            Guid? agencyProfileId,
            string? lifecycleState,
            int? take,
            ICommercialFinanceObligationQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await query.ListObligationsAsync(
                    agencyProfileId,
                    lifecycleState,
                    take ?? 50,
                    cancellationToken);
                return Results.Ok(items);
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
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "lifecycleState"] = [ex.Message]
                });
            }
        }).RequireAuthorization(ObligationsReadPolicy);

        var settlements = endpoints.MapGroup("/api/commercial-finance/settlements")
            .WithTags("CommercialFinance");

        settlements.MapGet("/periods", async Task<IResult> (
            Guid? agencyProfileId,
            int? take,
            ICommercialFinanceSettlementQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await query.ListSettlementPeriodsAsync(agencyProfileId, take ?? 50, cancellationToken);
                return Results.Ok(items);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
        }).RequireAuthorization(SettlementsReadPolicy);

        var payouts = endpoints.MapGroup("/api/commercial-finance/payouts")
            .WithTags("CommercialFinance");

        payouts.MapGet("/instructions", async Task<IResult> (
            Guid? settlementRecordId,
            int? take,
            ICommercialFinancePayoutQuery query,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await query.ListPayoutInstructionsAsync(
                    settlementRecordId,
                    take ?? 50,
                    cancellationToken);
                return Results.Ok(items);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "take"] = [ex.Message]
                });
            }
        }).RequireAuthorization(PayoutsReadPolicy);

        return endpoints;
    }
}
