namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Controlled interest code for planning intent (P18-R4). Not Search facet authority.
/// </summary>
public sealed class InterestPreference
{
    public const int CodeMaxLength = 32;

    private InterestPreference()
    {
        Code = null!;
    }

    private InterestPreference(string code)
    {
        Code = code;
    }

    public string Code { get; private set; }

    public static InterestPreference Create(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Interest code max length is {CodeMaxLength}.", nameof(code));
        }

        if (normalized.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-')))
        {
            throw new ArgumentException("Interest code may contain A-Z, 0-9, hyphen, underscore.", nameof(code));
        }

        return new InterestPreference(normalized);
    }

    internal InterestPreference CaptureCopy() => new(Code);
}
