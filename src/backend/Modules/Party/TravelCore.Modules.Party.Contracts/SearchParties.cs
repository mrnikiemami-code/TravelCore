namespace TravelCore.Modules.Party.Contracts;

/// <summary>
/// Search/select read-model query for Party (no raw-ID UX requirement on consumers).
/// </summary>
public sealed class SearchPartiesRequest
{
    public string? Query { get; set; }

    /// <summary>
    /// Optional filter: Person | Organization | Agency.
    /// </summary>
    public string? Kind { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; } = 20;
}

public sealed class SearchPartiesResponse
{
    public required IReadOnlyList<PartySummaryResponse> Items { get; init; }

    public required int TotalCount { get; init; }
}

public sealed class PartySummaryResponse
{
    public required Guid Id { get; init; }

    public required string Kind { get; init; }

    public required string DisplayName { get; init; }

    public required string Status { get; init; }

    public string? PrimaryEmail { get; init; }
}
