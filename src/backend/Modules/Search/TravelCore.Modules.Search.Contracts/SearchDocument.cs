namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// P15-R2 Search read-model document. Not a Tour/Content/Pricing/Agency entity and not an EF table in T002.
/// P15-R6 extends optional structured/semantic fields for future AI consumers without embeddings.
/// </summary>
public sealed record SearchDocument(
    Guid DocumentId,
    string EntityType,
    Guid SourceId,
    string SourceModule,
    string LocaleCode,
    string Title,
    string? SearchableText,
    IReadOnlyDictionary<string, string>? StructuredAttributes,
    IReadOnlyList<string>? SemanticReferences = null,
    bool? IsPubliclyEligible = null,
    SearchFactProvenance? Provenance = null);
