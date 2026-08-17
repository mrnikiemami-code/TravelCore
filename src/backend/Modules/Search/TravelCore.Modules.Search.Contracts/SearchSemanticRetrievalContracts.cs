namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Provenance for projected facts (TC-P15-T006 / P15-R6). Search does not own business truth.
/// </summary>
public sealed record SearchFactProvenance(
    string FactOwnerModule,
    string? FactKind,
    string? SourceVersion);

/// <summary>
/// Consumer-neutral semantic retrieval snapshot. Not chatbot-specific; not AI-generated facts.
/// </summary>
public sealed record SemanticRetrievalSnapshot(
    Guid DocumentId,
    string EntityType,
    Guid SourceId,
    string SourceModule,
    string LocaleCode,
    string Title,
    IReadOnlyDictionary<string, string>? StructuredAttributes,
    IReadOnlyList<string>? SemanticReferences,
    bool? IsPubliclyEligible,
    SearchFactProvenance? Provenance);
