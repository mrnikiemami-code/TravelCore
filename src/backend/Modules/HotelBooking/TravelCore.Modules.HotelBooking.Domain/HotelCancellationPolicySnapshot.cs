namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Immutable transaction-time cancellation terms for an accepted commercial offer.
/// Not cancellation execution and not Refund. PropertyTimeZoneId is IANA metadata only.
/// </summary>
public sealed class HotelCancellationPolicySnapshot
{
    public const int TimeZoneIdMaxLength = 64;
    public const int ExplanationMaxLength = 2000;

    private readonly List<HotelCancellationPenaltyRule> _rules = [];

    private HotelCancellationPolicySnapshot()
    {
    }

    internal HotelCancellationPolicySnapshot(
        HotelRateOfferSnapshotId hotelRateOfferSnapshotId,
        string? propertyTimeZoneId,
        string? publicExplanation)
    {
        HotelRateOfferSnapshotId = hotelRateOfferSnapshotId;
        PropertyTimeZoneId = propertyTimeZoneId;
        PublicExplanation = publicExplanation;
    }

    public HotelRateOfferSnapshotId HotelRateOfferSnapshotId { get; private set; }

    public string? PropertyTimeZoneId { get; private set; }

    public string? PublicExplanation { get; private set; }

    public IReadOnlyList<HotelCancellationPenaltyRule> Rules => _rules;

    internal void AddRule(HotelCancellationPenaltyRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
    }
}
