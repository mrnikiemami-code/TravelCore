namespace TravelCore.Modules.Visa.Domain;

/// <summary>
/// Stable snake_case codes for visa requirement facts (TC-P17-T004). Data-extensible, not schema flags.
/// </summary>
public static class VisaRequirementCode
{
    public const int MaxLength = 64;

    public static string Normalize(string code, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code, paramName);
        var trimmed = code.Trim().ToLowerInvariant();
        if (trimmed.Length > MaxLength)
        {
            throw new ArgumentException($"{paramName} max length is {MaxLength}.", paramName);
        }

        if (trimmed[0] is < 'a' or > 'z'
            || trimmed.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c == '_')))
        {
            throw new ArgumentException(
                $"{paramName} must start with a letter and contain only a-z, 0-9, and underscore.",
                paramName);
        }

        return trimmed;
    }

    public static string? NormalizeOptional(string? value, string paramName) =>
        string.IsNullOrWhiteSpace(value) ? null : Normalize(value, paramName);
}
