namespace TravelCore.Identifiers;

/// <summary>
/// TravelCore UUID version 7 identity generation (ADR 0002).
/// Uses the framework-native <see cref="Guid.CreateVersion7()"/> path — no custom algorithm, no third-party package.
/// </summary>
public static class Uuid7
{
    /// <summary>
    /// Creates a new UUID v7 value.
    /// </summary>
    public static Guid New() => Guid.CreateVersion7();
}
