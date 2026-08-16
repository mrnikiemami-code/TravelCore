using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Seo.Contracts;

namespace TravelCore.Modules.Seo.Infrastructure.Endpoints;

internal static class SeoEndpoints
{
    public static IEndpointRouteBuilder MapSeoEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/seo")
            .WithTags("SEO");

        // Public redirect/canonical resolution — no Admin authorization.
        group.MapGet("/resolve/{locale}/{*path}", async Task<IResult> (
            string locale,
            string path,
            ISeoRedirectService redirects,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var resolution = await redirects.ResolvePathAsync(locale, path, cancellationToken);
                return resolution.Kind switch
                {
                    "CurrentRoute" => Results.Ok(resolution),
                    "PermanentRedirect" => Results.Redirect(
                        BuildPublicLocation(resolution.Locale, resolution.TargetPath!),
                        permanent: true),
                    "Gone" => Results.StatusCode(StatusCodes.Status410Gone),
                    _ => Results.NotFound()
                };
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "path"] = [ex.Message]
                });
            }
        });

        group.MapGet("/canonical/{locale}/{*path}", async Task<IResult> (
            string locale,
            string path,
            ISeoRedirectService redirects,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var canonical = await redirects.GetCanonicalAsync(locale, path, cancellationToken);
                return canonical is null ? Results.NotFound() : Results.Ok(canonical);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "path"] = [ex.Message]
                });
            }
        });

        // Public IndexPolicy evaluation for SSR/robots consumers (T005 metadata integration contract).
        group.MapGet("/indexability/{locale}/{*path}", async Task<IResult> (
            string locale,
            string path,
            ISeoIndexPolicyService policies,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var evaluation = await policies.EvaluatePathAsync(locale, path, cancellationToken);
                return Results.Ok(evaluation);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "path"] = [ex.Message]
                });
            }
        });

        return endpoints;
    }

    private static string BuildPublicLocation(string locale, string path) =>
        $"/{locale.Trim('/')}/{path.Trim('/')}";
}
