namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// Transaction-time traveler snapshot owned by one Booking (P19-R4).
/// Not Party master, not CapacityHold, not a travel document.
/// </summary>
public sealed class BookingPassenger
{
    public const int NameMaxLength = 100;

    private BookingPassenger()
    {
    }

    private BookingPassenger(
        BookingPassengerId id,
        string givenName,
        string familyName,
        TravelerCategory category,
        int sequence)
    {
        Id = id;
        GivenName = givenName;
        FamilyName = familyName;
        Category = category;
        Sequence = sequence;
    }

    public BookingPassengerId Id { get; private set; }

    public string GivenName { get; private set; } = string.Empty;

    public string FamilyName { get; private set; } = string.Empty;

    public TravelerCategory Category { get; private set; }

    public int Sequence { get; private set; }

    public static BookingPassenger Create(
        string givenName,
        string familyName,
        TravelerCategory category,
        int sequence)
    {
        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "TravelerCategory is not controlled.");
        }

        if (sequence < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "Sequence must be >= 0.");
        }

        return new BookingPassenger(
            BookingPassengerId.New(),
            RequireName(givenName, nameof(givenName)),
            RequireName(familyName, nameof(familyName)),
            category,
            sequence);
    }

    public void Rename(string givenName, string familyName)
    {
        GivenName = RequireName(givenName, nameof(givenName));
        FamilyName = RequireName(familyName, nameof(familyName));
    }

    public void Recategorize(TravelerCategory category)
    {
        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(category), category, "TravelerCategory is not controlled.");
        }

        Category = category;
    }

    private static string RequireName(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Passenger name is required.", paramName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Name max length is {NameMaxLength}.", paramName);
        }

        return trimmed;
    }
}
