namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Shared UGC moderation + publication state machine (TC-P16-T007 / P16-R7).
/// One lifecycle for Review, Travelogue, UserPhoto, and Comment.
/// Public eligibility baseline: Approved + Published. Rejected is never publicly eligible.
/// </summary>
public readonly record struct UgcContentLifecycle
{
    public UgcContentLifecycle(ModerationStatus moderationStatus, PublicationStatus publicationStatus)
    {
        ArgumentNullException.ThrowIfNull(moderationStatus);
        ArgumentNullException.ThrowIfNull(publicationStatus);
        ModerationStatus = moderationStatus;
        PublicationStatus = publicationStatus;
    }

    public ModerationStatus ModerationStatus { get; }

    public PublicationStatus PublicationStatus { get; }

    /// <summary>Review, UserPhoto, and Comment enter Pending directly (not Draft).</summary>
    public static UgcContentLifecycle DirectPending() =>
        new(ModerationStatus.Pending, PublicationStatus.Hidden);

    /// <summary>Travelogue may start Draft; no separate Submission aggregate.</summary>
    public static UgcContentLifecycle DraftPending() =>
        new(ModerationStatus.Pending, PublicationStatus.Draft);

    public bool IsPubliclyEligible =>
        ModerationStatus == ModerationStatus.Approved
        && PublicationStatus == PublicationStatus.Published;

    public UgcContentLifecycle Submit()
    {
        EnsureNotArchived();
        return new UgcContentLifecycle(ModerationStatus.Pending, PublicationStatus);
    }

    public UgcContentLifecycle Approve()
    {
        EnsureNotArchived();
        return new UgcContentLifecycle(ModerationStatus.Approved, PublicationStatus);
    }

    public UgcContentLifecycle Reject()
    {
        EnsureNotArchived();
        return new UgcContentLifecycle(ModerationStatus.Rejected, PublicationStatus);
    }

    public UgcContentLifecycle Publish()
    {
        EnsureNotArchived();
        if (ModerationStatus == ModerationStatus.Rejected)
        {
            throw new InvalidOperationException("Rejected content must never be publicly eligible.");
        }

        if (ModerationStatus != ModerationStatus.Approved)
        {
            throw new InvalidOperationException("Only Approved content can be Published. Approved != Published.");
        }

        return new UgcContentLifecycle(ModerationStatus, PublicationStatus.Published);
    }

    public UgcContentLifecycle Hide()
    {
        EnsureNotArchived();
        return new UgcContentLifecycle(ModerationStatus, PublicationStatus.Hidden);
    }

    public UgcContentLifecycle Archive() =>
        new(ModerationStatus, PublicationStatus.Archived);

    private void EnsureNotArchived()
    {
        if (PublicationStatus == PublicationStatus.Archived)
        {
            throw new InvalidOperationException("Archived content cannot change moderation or publication state.");
        }
    }
}
