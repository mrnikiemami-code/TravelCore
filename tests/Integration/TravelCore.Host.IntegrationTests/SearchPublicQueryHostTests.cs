using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using TravelCore.Modules.Search.Contracts;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

public sealed class SearchPublicQueryHostTests
{
    [Fact]
    public async Task Public_Search_Requires_Locale_And_Returns_Empty_Stub()
    {
        await using var factory = new TravelCoreApiFactory(Environments.Development);
        var client = factory.CreateClient();
        var ct = TestContext.Current.CancellationToken;

        var missingLocale = await client.GetAsync("/api/search/", ct);
        Assert.Equal(HttpStatusCode.BadRequest, missingLocale.StatusCode);

        var ok = await client.GetAsync(
            "/api/search/?localeCode=fa-IR&queryText=istanbul&entityTypes=TourProduct&requestedFacets=difficulty&pageSize=5",
            ct);
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);

        var body = await ok.Content.ReadFromJsonAsync<SearchPublicQueryResponse>(cancellationToken: ct);
        Assert.NotNull(body);
        Assert.Equal("fa-IR", body.LocaleCode);
        Assert.Empty(body.Hits);
        Assert.Equal(5, body.Continuation.PageSize);
        Assert.Equal(0, body.Continuation.ReturnedCount);
        var json = JsonSerializer.Serialize(body);
        Assert.DoesNotContain("lucene", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("shard", json, StringComparison.OrdinalIgnoreCase);
    }
}
