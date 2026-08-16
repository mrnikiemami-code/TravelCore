namespace TravelCore.Modules.Seo.Domain;

/// <summary>One truthful breadcrumb node from Destination hierarchy (not SEO-owned content).</summary>
public sealed record SeoBreadcrumbNodeInput(
    string Name,
    string? PublicPath);

/// <summary>JSON-LD BreadcrumbList projection (TC-P05-T008) — truthful fields only.</summary>
public sealed record SeoBreadcrumbListDocument(
    string Context,
    string Type,
    IReadOnlyList<SeoBreadcrumbListItem> ItemListElement);

public sealed record SeoBreadcrumbListItem(
    string Type,
    int Position,
    string Name,
    string? Item);

/// <summary>
/// Pure structured-data projection for breadcrumbs (TC-P05-T008).
/// Never fabricates ratings/prices/Tour/Hotel types.
/// </summary>
public static class SeoStructuredDataEngine
{
    public const string SchemaContext = "https://schema.org";

    public static SeoBreadcrumbListDocument? BuildBreadcrumbList(
        string locale,
        IEnumerable<SeoBreadcrumbNodeInput> nodes)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        var localeNorm = SeoRoute.NormalizeLocale(locale);

        var items = new List<SeoBreadcrumbListItem>();
        var position = 1;
        foreach (var node in nodes)
        {
            ArgumentNullException.ThrowIfNull(node);
            var name = node.Name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            string? itemHref = null;
            if (!string.IsNullOrWhiteSpace(node.PublicPath))
            {
                var path = SeoRoute.NormalizePath(node.PublicPath);
                itemHref = $"/{localeNorm}/{path}";
            }

            items.Add(new SeoBreadcrumbListItem("ListItem", position, name, itemHref));
            position++;
        }

        if (items.Count == 0)
        {
            return null;
        }

        return new SeoBreadcrumbListDocument(SchemaContext, "BreadcrumbList", items);
    }
}
