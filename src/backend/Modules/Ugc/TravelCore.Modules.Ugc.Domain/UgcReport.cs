using NodaTime;

namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// UGC-owned report of user content (TC-P16-T007 / P16-R7). Moderation input only.
/// Does not own the target, hide content, reject, ban, rank, or change SEO policy.
/// </summary>
public sealed class UgcReport
{
    public const int OptionalDetailMaxLength = 2000;

    private UgcReport()
    {
        TargetType = null!;
        ReasonCode = null!;
        Status = null!;
    }

    private UgcReport(
        UgcReportId id,
        Guid reporterActorId,
        UgcReportTarget target,
        UgcReportReasonCode reasonCode,
        string? optionalDetail,
        Instant createdAt)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("UgcReportId cannot be empty.", nameof(id));
        }

        if (reporterActorId == Guid.Empty)
        {
            throw new ArgumentException("ReporterActorId cannot be empty.", nameof(reporterActorId));
        }

        Id = id;
        ReporterActorId = reporterActorId;
        TargetType = target.TargetType;
        TargetId = target.TargetId;
        ReasonCode = reasonCode;
        OptionalDetail = NormalizeOptionalDetail(optionalDetail);
        Status = UgcReportStatus.Open;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public UgcReportId Id { get; private set; }

    /// <summary>Opaque logical reporter actor id. Not Identity/Party ownership.</summary>
    public Guid ReporterActorId { get; private set; }

    public UgcReportTargetType TargetType { get; private set; }

    public Guid TargetId { get; private set; }

    public UgcReportTarget Target => new(TargetType, TargetId);

    public UgcReportReasonCode ReasonCode { get; private set; }

    public string? OptionalDetail { get; private set; }

    public UgcReportStatus Status { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    public static UgcReport Create(
        Guid reporterActorId,
        string targetType,
        Guid targetId,
        string reasonCode,
        Instant now,
        string? optionalDetail = null) =>
        new(
            UgcReportId.New(),
            reporterActorId,
            UgcReportTarget.Create(targetType, targetId),
            UgcReportReasonCode.Parse(reasonCode),
            optionalDetail,
            now);

    public void Resolve(Instant now)
    {
        EnsureOpen();
        Status = UgcReportStatus.Resolved;
        UpdatedAt = now;
    }

    public void Dismiss(Instant now)
    {
        EnsureOpen();
        Status = UgcReportStatus.Dismissed;
        UpdatedAt = now;
    }

    private void EnsureOpen()
    {
        if (Status != UgcReportStatus.Open)
        {
            throw new InvalidOperationException("Only an Open report can be Resolved or Dismissed.");
        }
    }

    private static string? NormalizeOptionalDetail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > OptionalDetailMaxLength)
        {
            throw new ArgumentException(
                $"optionalDetail cannot exceed {OptionalDetailMaxLength} characters.",
                nameof(value));
        }

        return trimmed;
    }
}
