namespace TravelCore.Modules.Search.Contracts;

/// <summary>
/// Replaceable Search index port (TC-P15-T002 / P15-R2). No concrete engine in T002.
/// Query execution remains P15-R7; ranking/faceting remain unlocked.
/// </summary>
public interface ISearchIndex
{
    Task UpsertAsync(
        SearchDocument document,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);
}
