namespace TravelCore.Modules.B2B.Domain;

/// <summary>
/// Logical reference to a declared sales/distribution channel posture. Not a persisted sales channel entity.
/// </summary>
public sealed class SalesChannelReference
{
    private SalesChannelReference()
    {
        ChannelCode = null!;
    }

    private SalesChannelReference(string channelCode)
    {
        ChannelCode = NormalizeRequired(channelCode, nameof(channelCode), 64);
    }

    public string ChannelCode { get; private set; }

    public static SalesChannelReference FromCode(string channelCode) =>
        new(channelCode);

    private static string NormalizeRequired(string value, string paramName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Length must be <= {maxLength}.");
        }

        return trimmed;
    }
}
