using System.Text.Json.Serialization;

namespace TravelCore.Modules.Seo.Contracts;

public sealed record SeoBreadcrumbNodeRequest(
    string Name,
    string? PublicPath);

public sealed record ComposeSeoBreadcrumbRequest(
    string Locale,
    IReadOnlyList<SeoBreadcrumbNodeRequest> Nodes);

public sealed record SeoBreadcrumbListItemResponse(
    [property: JsonPropertyName("@type")] string Type,
    int Position,
    string Name,
    string? Item);

public sealed record SeoBreadcrumbListResponse(
    [property: JsonPropertyName("@context")] string Context,
    [property: JsonPropertyName("@type")] string Type,
    IReadOnlyList<SeoBreadcrumbListItemResponse> ItemListElement);

/// <summary>SEO/presentation structured-data projections (TC-P05-T008) — not domain entities.</summary>
public interface ISeoStructuredDataService
{
    Task<SeoBreadcrumbListResponse?> ComposeBreadcrumbAsync(
        ComposeSeoBreadcrumbRequest request,
        CancellationToken cancellationToken = default);
}
