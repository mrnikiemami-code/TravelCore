namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Submission-time contact facts copied onto a Lead (P18-R3).
/// Historical follow-up context — not Party, Identity, or Customer master data.
/// </summary>
public sealed class LeadContactSnapshot
{
    public const int DisplayNameMaxLength = 200;
    public const int EmailMaxLength = 256;
    public const int PhoneMaxLength = 256;

    private static readonly LeadContactSnapshot EmptyInstance = new(null, null, null, null);

    private LeadContactSnapshot(
        string? displayName,
        string? email,
        string? normalizedEmail,
        string? phone)
    {
        DisplayName = displayName;
        Email = email;
        NormalizedEmail = normalizedEmail;
        Phone = phone;
    }

    public string? DisplayName { get; private set; }

    public string? Email { get; private set; }

    public string? NormalizedEmail { get; private set; }

    public string? Phone { get; private set; }

    public static LeadContactSnapshot Empty => EmptyInstance;

    public static LeadContactSnapshot Create(
        string? displayName = null,
        string? email = null,
        string? phone = null)
    {
        var normalizedDisplayName = NormalizeOptional(displayName, DisplayNameMaxLength, nameof(displayName));
        var (rawEmail, normalizedEmail) = NormalizeEmail(email);
        var normalizedPhone = NormalizeOptional(phone, PhoneMaxLength, nameof(phone));

        if (normalizedDisplayName is null && rawEmail is null && normalizedPhone is null)
        {
            return Empty;
        }

        return new LeadContactSnapshot(normalizedDisplayName, rawEmail, normalizedEmail, normalizedPhone);
    }

    internal static (string? Email, string? NormalizedEmail) NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return (null, null);
        }

        var trimmed = email.Trim();
        if (trimmed.Length > EmailMaxLength)
        {
            throw new ArgumentException($"Email max length is {EmailMaxLength}.", nameof(email));
        }

        if (!trimmed.Contains('@', StringComparison.Ordinal))
        {
            throw new ArgumentException("Email must contain '@'.", nameof(email));
        }

        return (trimmed, trimmed.ToUpperInvariant());
    }

    private static string? NormalizeOptional(string? value, int maxLength, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Max length is {maxLength}.", paramName);
        }

        return trimmed;
    }
}
