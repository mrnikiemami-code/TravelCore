using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Seo.Contracts;
using TravelCore.Modules.Seo.Domain;

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

        // Public hreflang bindings — genuine SeoRoute locales only (T006 / ADR 0008).
        group.MapGet("/hreflang/{resourceType}/{resourceId:guid}", async Task<IResult> (
            string resourceType,
            Guid resourceId,
            ISeoHreflangService hreflang,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var bindings = await hreflang.GetByResourceAsync(resourceType, resourceId, cancellationToken);
                return bindings is null ? Results.NotFound() : Results.Ok(bindings);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "resourceType"] = [ex.Message]
                });
            }
        });

        group.MapGet("/hreflang/by-path/{locale}/{*path}", async Task<IResult> (
            string locale,
            string path,
            ISeoHreflangService hreflang,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var bindings = await hreflang.GetByPathAsync(locale, path, cancellationToken);
                return bindings is null ? Results.NotFound() : Results.Ok(bindings);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "path"] = [ex.Message]
                });
            }
        });

        // Server-side metadata composition (T007) — content inputs from domain contracts.
        group.MapPost("/metadata/compose", async Task<IResult> (
            ComposeSeoMetadataRequest request,
            ISeoMetadataService metadata,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var composed = await metadata.ComposeAsync(request, cancellationToken);
                return Results.Ok(composed);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        });

        group.MapGet("/metadata/overrides/{resourceType}/{resourceId:guid}/{locale}", async Task<IResult> (
            string resourceType,
            Guid resourceId,
            string locale,
            ISeoMetadataService metadata,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var row = await metadata.GetOverrideAsync(resourceType, resourceId, locale, cancellationToken);
                return row is null ? Results.NotFound() : Results.Ok(row);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "resourceType"] = [ex.Message]
                });
            }
        });

        group.MapPut("/metadata/overrides", async Task<IResult> (
            SetSeoMetadataOverrideRequest request,
            ISeoMetadataService metadata,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var row = await metadata.SetOverrideAsync(request, cancellationToken);
                return Results.Ok(row);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        });

        // Truthful breadcrumb JSON-LD projection (T008) — no ratings/prices fabrication.
        group.MapPost("/structured-data/breadcrumb", async Task<IResult> (
            ComposeSeoBreadcrumbRequest request,
            ISeoStructuredDataService structuredData,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var doc = await structuredData.ComposeBreadcrumbAsync(request, cancellationToken);
                return doc is null ? Results.NoContent() : Results.Ok(doc);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        });

        // Sitemap / robots framework (T009) — IndexPolicy-gated inclusion only.
        group.MapGet("/sitemap.xml", async Task<IResult> (
            ISeoSitemapService sitemap,
            CancellationToken cancellationToken) =>
        {
            var xml = await sitemap.RenderSitemapXmlAsync(cancellationToken);
            return Results.Content(xml, "application/xml; charset=utf-8");
        });

        group.MapGet("/sitemap", async Task<IResult> (
            ISeoSitemapService sitemap,
            CancellationToken cancellationToken) =>
        {
            var doc = await sitemap.BuildAsync(cancellationToken);
            return Results.Ok(doc);
        });

        group.MapGet("/robots.txt", (ISeoSitemapService sitemap) =>
            Results.Content(sitemap.RenderRobotsTxt(), "text/plain; charset=utf-8"));

        // Admin Destination SEO posture (T011) — job-based inspect; Access-backed.
        group.MapGet("/admin/destination-posture/{destinationId:guid}/{locale}", async Task<IResult> (
            Guid destinationId,
            string locale,
            ISeoAdminDestinationPostureService posture,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var snapshot = await posture.GetDestinationPostureAsync(destinationId, locale, cancellationToken);
                return Results.Ok(snapshot);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "destinationId"] = [ex.Message]
                });
            }
        }).RequireAuthorization("Access.Seo.DestinationPosture.Write");

        group.MapGet("/routes/by-resource/{resourceType}/{resourceId:guid}", async Task<IResult> (
            string resourceType,
            Guid resourceId,
            ISeoRouteService routes,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var list = await routes.ListByResourceAsync(resourceType, resourceId, cancellationToken);
                return Results.Ok(list);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "resourceType"] = [ex.Message]
                });
            }
        }).RequireAuthorization("Access.Seo.DestinationPosture.Write");

        group.MapGet("/index-policies/{resourceType}/{resourceId:guid}/{locale}", async Task<IResult> (
            string resourceType,
            Guid resourceId,
            string locale,
            ISeoIndexPolicyService policies,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var row = await policies.GetAsync(resourceType, resourceId, locale, cancellationToken);
                return row is null ? Results.NotFound() : Results.Ok(row);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "resourceType"] = [ex.Message]
                });
            }
        }).RequireAuthorization("Access.Seo.DestinationPosture.Write");

        group.MapPut("/index-policies", async Task<IResult> (
            SetSeoIndexPolicyRequest request,
            ISeoIndexPolicyService policies,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var row = await policies.SetAsync(request, cancellationToken);
                return Results.Ok(row);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
        }).RequireAuthorization("Access.Seo.DestinationPosture.Write");

        // Destination public-path publication (T010/T011) — SEO namespace; Access-backed write.
        group.MapPost("/publication/destination", async Task<IResult> (
            PublishDestinationSeoRouteRequest request,
            ISeoDestinationPublicationService publication,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await publication.PublishAsync(request, cancellationToken);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
            catch (SeoRouteConflictException ex)
            {
                return Results.Conflict(new { title = "SeoRoute conflict", detail = ex.Message });
            }
        }).RequireAuthorization("Access.Seo.DestinationPosture.Write");

        // Place public-path publication (TC-P07-T007) — SEO namespace; Access-backed write.
        // Does not set IndexPolicy (P07-R5: default missing policy remains noindex,follow).
        group.MapPost("/publication/place", async Task<IResult> (
            PublishPlaceSeoRouteRequest request,
            ISeoPlacePublicationService publication,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await publication.PublishAsync(request, cancellationToken);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
            catch (SeoRouteConflictException ex)
            {
                return Results.Conflict(new { title = "SeoRoute conflict", detail = ex.Message });
            }
        }).RequireAuthorization("Access.Seo.PlacePosture.Write");

        // Article public-path publication (TC-P08-T008) — SEO namespace; Access-backed write.
        // Does not set IndexPolicy (P08-R4: default missing policy remains noindex,follow).
        group.MapPost("/publication/article", async Task<IResult> (
            PublishArticleSeoRouteRequest request,
            ISeoArticlePublicationService publication,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await publication.PublishAsync(request, cancellationToken);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
            catch (SeoRouteConflictException ex)
            {
                return Results.Conflict(new { title = "SeoRoute conflict", detail = ex.Message });
            }
        }).RequireAuthorization("Access.Seo.ContentPosture.Write");

        // LandingPage public-path publication (TC-P08-T008) — SEO namespace; Access-backed write.
        // Does not set IndexPolicy (P08-R4: default missing policy remains noindex,follow).
        group.MapPost("/publication/landing-page", async Task<IResult> (
            PublishLandingPageSeoRouteRequest request,
            ISeoLandingPagePublicationService publication,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await publication.PublishAsync(request, cancellationToken);
                return Results.Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    [ex.ParamName ?? "request"] = [ex.Message]
                });
            }
            catch (SeoRouteConflictException ex)
            {
                return Results.Conflict(new { title = "SeoRoute conflict", detail = ex.Message });
            }
        }).RequireAuthorization("Access.Seo.ContentPosture.Write");

        return endpoints;
    }

    private static string BuildPublicLocation(string locale, string path) =>
        $"/{locale.Trim('/')}/{path.Trim('/')}";
}
