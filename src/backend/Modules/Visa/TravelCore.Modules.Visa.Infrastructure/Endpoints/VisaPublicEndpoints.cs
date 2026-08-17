using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Visa.Contracts;

namespace TravelCore.Modules.Visa.Infrastructure.Endpoints;

/// <summary>
/// Anonymous public Visa composition reads (TC-P17-T007 / P17-R7).
/// Structured facts only. Not SEO indexing authority and not a Search engine.
/// </summary>
internal static class VisaPublicEndpoints
{
    public static IEndpointRouteBuilder MapVisaPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var publicVisa = endpoints.MapGroup("/api/visa/public")
            .WithTags("Visa");

        publicVisa.MapGet("/definitions/{code}", async Task<IResult> (
            string code,
            string? localeCode,
            IVisaPublicQuery query,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["localeCode"] = ["Locale code is required."]
                });
            }

            try
            {
                var page = await query.GetByCodeAsync(code, localeCode, cancellationToken);
                return page is null ? Results.NotFound() : Results.Ok(page);
            }
            catch (ArgumentException ex)
            {
                return Validation(ex);
            }
        });

        return endpoints;
    }

    private static IResult Validation(ArgumentException ex) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            [ex.ParamName ?? "value"] = [ex.Message]
        });
}
