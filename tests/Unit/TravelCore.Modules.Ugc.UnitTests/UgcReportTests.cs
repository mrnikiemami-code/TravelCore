using NodaTime;
using TravelCore.Modules.Ugc.Contracts;
using TravelCore.Modules.Ugc.Domain;
using Xunit;

namespace TravelCore.Modules.Ugc.UnitTests;

/// <summary>
/// UGC-owned report (TC-P16-T007 / P16-R7). Moderation input only — no automatic enforcement.
/// </summary>
public sealed class UgcReportTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 8, 17, 23, 0);
    private static readonly Instant Later = Instant.FromUtc(2026, 8, 17, 23, 45);
    private static readonly Guid Reporter = Guid.Parse("0198b3e0-0000-7000-8000-0000000000a1");
    private static readonly Guid Target = Guid.Parse("0198b3e0-0000-7000-8000-0000000000a2");

    [Fact]
    public void Create_Opens_Report_Without_Changing_Target()
    {
        var report = UgcReport.Create(Reporter, "Review", Target, "Spam", Now, "  copy  ");

        Assert.NotEqual(Guid.Empty, report.Id.Value);
        Assert.Equal(Reporter, report.ReporterActorId);
        Assert.Equal(UgcReportTargetType.Review, report.TargetType);
        Assert.Equal(Target, report.TargetId);
        Assert.Equal(UgcReportReasonCode.Spam, report.ReasonCode);
        Assert.Equal("copy", report.OptionalDetail);
        Assert.Equal(UgcReportStatus.Open, report.Status);
        Assert.True(UgcOwnershipBoundary.ReportImplemented);
        Assert.False(UgcOwnershipBoundary.ReportTriggersAutomaticEnforcement);
        Assert.Null(typeof(UgcReport).GetProperty("HideAfterCount"));
        Assert.Null(typeof(UgcReport).GetProperty("RankingDelta"));
    }

    [Fact]
    public void Create_Rejects_Unknown_Target_Reason_Or_Empty_Actor()
    {
        Assert.Throws<ArgumentException>(() => UgcReport.Create(Guid.Empty, "Review", Target, "spam", Now));
        Assert.Throws<ArgumentException>(() => UgcReport.Create(Reporter, "Place", Target, "spam", Now));
        Assert.Throws<ArgumentException>(() => UgcReport.Create(Reporter, "Review", Guid.Empty, "spam", Now));
        Assert.Throws<ArgumentException>(() => UgcReport.Create(Reporter, "Review", Target, "bot_net", Now));
    }

    [Fact]
    public void Resolve_And_Dismiss_Are_Terminal_For_Open_Reports()
    {
        var resolved = UgcReport.Create(Reporter, "Travelogue", Target, "abuse", Now);
        resolved.Resolve(Later);
        Assert.Equal(UgcReportStatus.Resolved, resolved.Status);
        Assert.Equal(Later, resolved.UpdatedAt);
        Assert.Throws<InvalidOperationException>(() => resolved.Dismiss(Later));

        var dismissed = UgcReport.Create(Reporter, "UserPhoto", Target, "other", Now);
        dismissed.Dismiss(Later);
        Assert.Equal(UgcReportStatus.Dismissed, dismissed.Status);
        Assert.Throws<InvalidOperationException>(() => dismissed.Resolve(Later));
    }

    [Fact]
    public void Report_Does_Not_Hide_Or_Reject_Target()
    {
        var review = Review.Create(Reporter, 3, Now, "Agency", Target);
        review.Approve(Now);
        review.Publish(Now);
        _ = UgcReport.Create(Reporter, "Comment", Target, "off_topic", Now);
        Assert.True(review.IsPubliclyEligible);
        Assert.Equal(ModerationStatus.Approved, review.ModerationStatus);
        Assert.Equal(PublicationStatus.Published, review.PublicationStatus);
    }
}
