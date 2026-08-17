using NodaTime;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Submission-time consent/privacy evidence copied onto a Lead (P18-R7).
/// Historical submission context — not Party, Identity, marketing platform, or Notification delivery.
/// </summary>
public sealed class LeadConsentSnapshot
{
    public const int PrivacyNoticeVersionMaxLength = 64;

    private LeadConsentSnapshot()
    {
    }

    private LeadConsentSnapshot(
        bool followUpContactAllowed,
        bool marketingAllowed,
        string? privacyNoticeVersion,
        LeadContactChannelPreference? preferredContactChannel,
        Instant capturedAt)
    {
        FollowUpContactAllowed = followUpContactAllowed;
        MarketingAllowed = marketingAllowed;
        PrivacyNoticeVersion = privacyNoticeVersion;
        PreferredContactChannel = preferredContactChannel;
        CapturedAt = capturedAt;
    }

    public bool FollowUpContactAllowed { get; private set; }

    public bool MarketingAllowed { get; private set; }

    public string? PrivacyNoticeVersion { get; private set; }

    public LeadContactChannelPreference? PreferredContactChannel { get; private set; }

    public Instant CapturedAt { get; private set; }

    public static LeadConsentSnapshot Create(
        bool followUpContactAllowed,
        bool marketingAllowed,
        string? privacyNoticeVersion,
        LeadContactChannelPreference? preferredContactChannel,
        Instant capturedAt)
    {
        if (capturedAt == default)
        {
            throw new ArgumentException("CapturedAt cannot be default.", nameof(capturedAt));
        }

        var normalizedVersion = NormalizePrivacyNoticeVersion(privacyNoticeVersion);
        return new LeadConsentSnapshot(
            followUpContactAllowed,
            marketingAllowed,
            normalizedVersion,
            preferredContactChannel,
            capturedAt);
    }

    public static LeadConsentSnapshot EmptyFor(Instant capturedAt)
    {
        if (capturedAt == default)
        {
            throw new ArgumentException("CapturedAt cannot be default.", nameof(capturedAt));
        }

        return new LeadConsentSnapshot(false, false, null, null, capturedAt);
    }

    internal static LeadConsentSnapshot InferForSubmission(LeadContactSnapshot contact, Instant capturedAt)
    {
        ArgumentNullException.ThrowIfNull(contact);
        if (contact == LeadContactSnapshot.Empty)
        {
            return EmptyFor(capturedAt);
        }

        return Create(
            followUpContactAllowed: true,
            marketingAllowed: false,
            privacyNoticeVersion: "P18-PRIVACY-V1",
            preferredContactChannel: InferPreferredChannel(contact),
            capturedAt: capturedAt);
    }

    internal void ValidateForLeadSubmission(LeadContactSnapshot contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        if (CapturedAt == default)
        {
            throw new InvalidOperationException("Lead consent snapshot requires CapturedAt.");
        }

        if (contact != LeadContactSnapshot.Empty && !FollowUpContactAllowed)
        {
            throw new InvalidOperationException(
                "Follow-up contact permission is required when a Lead includes contact details.");
        }

        if (FollowUpContactAllowed && string.IsNullOrWhiteSpace(PrivacyNoticeVersion))
        {
            throw new InvalidOperationException(
                "Privacy notice version is required when follow-up contact is allowed.");
        }
    }

    internal LeadConsentSnapshot CaptureCopy() =>
        new(
            FollowUpContactAllowed,
            MarketingAllowed,
            PrivacyNoticeVersion,
            PreferredContactChannel,
            CapturedAt);

    private static LeadContactChannelPreference? InferPreferredChannel(LeadContactSnapshot contact)
    {
        if (!string.IsNullOrWhiteSpace(contact.Email))
        {
            return LeadContactChannelPreference.Email;
        }

        if (!string.IsNullOrWhiteSpace(contact.Phone))
        {
            return LeadContactChannelPreference.Phone;
        }

        return null;
    }

    private static string? NormalizePrivacyNoticeVersion(string? privacyNoticeVersion)
    {
        if (string.IsNullOrWhiteSpace(privacyNoticeVersion))
        {
            return null;
        }

        var trimmed = privacyNoticeVersion.Trim();
        if (trimmed.Length > PrivacyNoticeVersionMaxLength)
        {
            throw new ArgumentException(
                $"Privacy notice version max length is {PrivacyNoticeVersionMaxLength}.",
                nameof(privacyNoticeVersion));
        }

        return trimmed;
    }
}
