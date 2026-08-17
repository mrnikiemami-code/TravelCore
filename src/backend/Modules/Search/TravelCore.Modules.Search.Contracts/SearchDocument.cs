namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R2 Search read-model document. Not a Tour/Content/Pricing/Agency entity and not an EF table in T002.
/// </summary>
public sealed record SearchDocument(
    Guid DocumentId,
    string EntityType,
    Guid SourceId,
    string SourceModule,
    string LocaleCode,
    string Title,
    string? SearchableText,
    IReadOnlyDictionary<string, string>? StructuredAttributes);
