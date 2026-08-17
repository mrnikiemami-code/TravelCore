namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Projection-ready source pointer. Domain modules remain fact owners (P15-R2).
/// Sync/pipeline (P15-R3) is not implemented here.
/// </summary>
public sealed record SearchProjectionSource(
    string SourceModule,
    string EntityType,
    Guid SourceId,
    string LocaleCode);

/// <summary>
/// Inbound projection envelope Search may later map to <see cref="SearchDocument"/>.
/// Not a catalog clone and not a price SoR.
/// </summary>
public sealed record SearchProjectionEnvelope(
    SearchProjectionSource Source,
    string Title,
    string? SearchableText,
    IReadOnlyDictionary<string, string>? StructuredAttributes);
