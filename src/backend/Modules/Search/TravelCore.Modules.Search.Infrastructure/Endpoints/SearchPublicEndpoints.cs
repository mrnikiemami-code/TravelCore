using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TravelCore.Modules.Search.Contracts;

namespace TravelCore.Modules.Search.Infrastructure.Endpoints;

/// <summary>
/// Public Search query HTTP surface (TC-P15-T007 / P15-R7). Engine-neutral; anonymous read.
/// </summary>
internal static class SearchPublicEndpoints
{
    public static IEndpointRouteBuilder MapSearchPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/search")
            .WithTags("SearchPublic")
            .AllowAnonymous();

        group.MapGet("/", async Task<IResult> (
            string? localeCode,
            string? queryText,
            string? entityTypes,
            string? sort,
            int? pageSize,
            string? continuationToken,
            string? requestedFacets,
            string? filterDifficulty,
            string? filterDestination,
            ISearchQueryService query,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(localeCode))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["localeCode"] = ["LocaleCode is required."]
                });
            }

            var structuredFilters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(filterDifficulty))
            {
                structuredFilters["Difficulty"] = filterDifficulty.Trim();
            }

            if (!string.IsNullOrWhiteSpace(filterDestination))
            {
                structuredFilters["Destination"] = filterDestination.Trim();
            }

            var request = new SearchPublicQueryRequest(
                LocaleCode: localeCode.Trim(),
                QueryText: string.IsNullOrWhiteSpace(queryText) ? null : queryText.Trim(),
                EntityTypes: SplitCsv(entityTypes),
                StructuredFilters: structuredFilters.Count == 0 ? null : structuredFilters,
                Sort: string.IsNullOrWhiteSpace(sort) ? null : sort.Trim(),
                PageSize: pageSize,
                ContinuationToken: string.IsNullOrWhiteSpace(continuationToken) ? null : continuationToken.Trim(),
                RequestedFacets: SplitCsv(requestedFacets));

            try
            {
                var response = await query.QueryAsync(request, cancellationToken);
                return Results.Ok(response);
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

    private static IReadOnlyList<string>? SplitCsv(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var parts = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(static x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return parts.Length == 0 ? null : parts;
    }
}
