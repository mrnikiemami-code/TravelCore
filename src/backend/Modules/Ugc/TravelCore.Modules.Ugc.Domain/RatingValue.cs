namespace TravelCore.Modules.Ugc.Domain;

/// <summary>
/// Integer rating 1..5. Not an independent aggregate (P16-R2).
/// </summary>
public readonly record struct RatingValue
{
    public const int Min = 1;
    public const int Max = 5;

    public int Value { get; }

    private RatingValue(int value) => Value = value;

    public static RatingValue From(int value)
    {
        if (value is < Min or > Max)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Rating must be between {Min} and {Max}.");
        }

        return new RatingValue(value);
    }

    public override string ToString() => Value.ToString();
}
