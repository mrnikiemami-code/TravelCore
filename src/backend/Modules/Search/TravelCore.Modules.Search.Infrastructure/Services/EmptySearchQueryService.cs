using TravelCore.Modules.Search.Contracts;

namespace TravelCore.Modules.Search.Infrastructure.Services;

/// <summary>
/// Engine-neutral empty query stub (TC-P15-T007). No FTS/ES/OpenSearch.
/// </summary>
public sealed class EmptySearchQueryService : ISearchQueryService
{
    public Task<SearchPublicQueryResponse> QueryAsync(
        SearchPublicQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.LocaleCode))
        {
            throw new ArgumentException("LocaleCode is required.", nameof(request));
        }

        var pageSize = request.PageSize is > 0 and <= 100 ? request.PageSize.Value : 20;
        var response = new SearchPublicQueryResponse(
            LocaleCode: request.LocaleCode.Trim(),
            Hits: [],
            Facets: request.RequestedFacets is { Count: > 0 }
                ? request.RequestedFacets.Select(key => new FacetResult(key, [])).ToArray()
                : null,
            Continuation: new SearchContinuation(
                NextContinuationToken: null,
                PageSize: pageSize,
                ReturnedCount: 0),
            ResultMetadata: new Dictionary<string, string>
            {
                ["execution"] = "EmptyStub",
                ["engine"] = "None"
            });

        return Task.FromResult(response);
    }
}
