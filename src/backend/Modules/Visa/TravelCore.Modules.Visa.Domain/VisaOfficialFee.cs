using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;
using NodaTime;

namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Official/regulatory visa fee fact for one VisaRequirementSet (TC-P17-T006 / P17-R6).
/// Not a commercial Price, Quote, markup, discount, payment amount, or FX conversion.
/// </summary>
public sealed class VisaOfficialFee
{
    public const int SourceMaxLength = 200;

    private VisaOfficialFee()
    {
        Kind = null!;
        Money = null!;
    }

    private VisaOfficialFee(
        VisaOfficialFeeId id,
        VisaRequirementSetId visaRequirementSetId,
        VisaOfficialFeeKind kind,
        MoneyValue money,
        int sortOrder,
        string? source,
        Instant createdAt)
    {
        Id = id;
        VisaRequirementSetId = visaRequirementSetId;
        Kind = kind;
        Money = money;
        SortOrder = sortOrder;
        Source = source;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public VisaOfficialFeeId Id { get; private set; }

    public VisaRequirementSetId VisaRequirementSetId { get; private set; }

    public VisaOfficialFeeKind Kind { get; private set; }

    /// <summary>Platform money in the official source currency. Not a display/FX amount.</summary>
    public MoneyValue Money { get; private set; }

    public int SortOrder { get; private set; }

    /// <summary>Optional provenance label. Not a government integration or scrape.</summary>
    public string? Source { get; private set; }

    public Instant CreatedAt { get; private set; }

    public Instant UpdatedAt { get; private set; }

    internal static VisaOfficialFee Create(
        VisaOfficialFeeId id,
        VisaRequirementSetId visaRequirementSetId,
        string kind,
        decimal amount,
        string currencyCode,
        Instant now,
        string? source = null,
        int sortOrder = 0)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaOfficialFeeId cannot be empty.", nameof(id));
        }

        if (visaRequirementSetId.Value == Guid.Empty)
        {
            throw new ArgumentException("VisaRequirementSetId cannot be empty.", nameof(visaRequirementSetId));
        }

        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Official fee amount cannot be negative.");
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sortOrder), "SortOrder cannot be negative.");
        }

        if (now == default)
        {
            throw new ArgumentException("CreatedAt cannot be default.", nameof(now));
        }

        var money = new MoneyValue(amount, VisaCurrency.ParseRequired(currencyCode));
        return new VisaOfficialFee(
            id,
            visaRequirementSetId,
            VisaOfficialFeeKind.Parse(kind),
            money,
            sortOrder,
            NormalizeSource(source),
            now);
    }

    private static string? NormalizeSource(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return null;
        }

        var trimmed = source.Trim();
        if (trimmed.Length > SourceMaxLength)
        {
            throw new ArgumentException($"Source max length is {SourceMaxLength}.", nameof(source));
        }

        return trimmed;
    }
}
