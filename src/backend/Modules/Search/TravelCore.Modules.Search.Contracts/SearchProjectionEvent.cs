namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Projection event shape consumed by Search (TC-P15-T003 / P15-R3).
/// Domain modules remain publishers / SoT; Search does not take FKs to peer schemas.
/// </summary>
public sealed record SearchProjectionEvent(
    Guid EventId,
    string SourceType,
    Guid SourceId,
    long Version,
    string LocaleCode,
    string ChangeKind,
    DateTimeOffset OccurredAtUtc);
