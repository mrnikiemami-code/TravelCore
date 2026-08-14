using Microsoft.Extensions.Primitives;

namespace TravelCore.Observability;

/// <summary>
/// Validates optional caller-supplied application correlation identifiers.
/// </summary>
public static class CorrelationIdValidator
{
    /// <summary>
    /// Returns true when <paramref name="values"/> contains exactly one safe correlation ID.
    /// </summary>
    public static bool TryGetValid(StringValues values, out string correlationId)
    {
        correlationId = string.Empty;

        if (values.Count != 1)
        {
            return false;
        }

        var raw = values[0];
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var trimmed = raw.Trim();
        if (trimmed.Length is 0 or > TravelCoreCorrelationHeaders.MaxLength)
        {
            return false;
        }

        foreach (var ch in trimmed)
        {
            if (char.IsControl(ch))
            {
                return false;
            }
        }

        correlationId = trimmed;
        return true;
    }
}
