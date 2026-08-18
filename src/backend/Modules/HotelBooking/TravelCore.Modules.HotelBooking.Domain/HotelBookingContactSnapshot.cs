namespace TravelCore.Modules.HotelBooking.Domain;

/// <summary>
/// Transaction-time contact for this HotelBooking. Not Party master data.
/// Independent of LeadGuest.
/// </summary>
public sealed class HotelBookingContactSnapshot
{
    public const int EmailMaxLength = 256;
    public const int PhoneMaxLength = 256;

    private HotelBookingContactSnapshot(
        string? email,
        string? normalizedEmail,
        string? phone)
    {
        Email = email;
        NormalizedEmail = normalizedEmail;
        Phone = phone;
    }

    public string? Email { get; private set; }

    public string? NormalizedEmail { get; private set; }

    public string? Phone { get; private set; }

    public static HotelBookingContactSnapshot Create(string? email = null, string? phone = null)
    {
        var (rawEmail, normalizedEmail) = NormalizeEmail(email);
        var normalizedPhone = NormalizeOptional(phone, PhoneMaxLength, nameof(phone));

        if (rawEmail is null && normalizedPhone is null)
        {
            throw new ArgumentException("HotelBookingContactSnapshot requires an email or phone.");
        }

        return new HotelBookingContactSnapshot(rawEmail, normalizedEmail, normalizedPhone);
    }

    private static (string? Email, string? NormalizedEmail) NormalizeEmail(string? email)
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
