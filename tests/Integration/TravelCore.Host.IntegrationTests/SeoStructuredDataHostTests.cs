using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace TravelCore.Host.IntegrationTests;

[Collection(nameof(SeoRedirectHostCollection))]
public sealed class SeoStructuredDataHostTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SeoRedirectHostFixture _fixture;

    public SeoStructuredDataHostTests(SeoRedirectHostFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Breadcrumb_Compose_ReturnsSchemaOrgList()
    {
        await using var factory = _fixture.CreateFactory();
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });
        var ct = TestContext.Current.CancellationToken;

        var response = await client.PostAsJsonAsync(
            new Uri("/api/seo/structured-data/breadcrumb", UriKind.Relative),
            new
            {
                locale = "en",
                nodes =
                    new[]
                    {
                        new { name = "Turkey", publicPath = "destinations/turkey" },
                        new { name = "Istanbul", publicPath = "destinations/istanbul" },
                    }
            },
            ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = doc.RootElement;
        Assert.Equal("https://schema.org", root.GetProperty("@context").GetString());
        Assert.Equal("BreadcrumbList", root.GetProperty("@type").GetString());
        Assert.Equal(2, root.GetProperty("itemListElement").GetArrayLength());
        Assert.Equal(
            "/en/destinations/istanbul",
            root.GetProperty("itemListElement")[1].GetProperty("item").GetString());
    }
}
