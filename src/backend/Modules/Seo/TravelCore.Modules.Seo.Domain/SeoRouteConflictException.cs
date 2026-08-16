namespace TravelCore.Modules.Seo.Domain;

/// <summary>
/// Raised when a locale path or resource+locale binding would collide in the public namespace.
/// </summary>
public sealed class SeoRouteConflictException : Exception
{
    public SeoRouteConflictException(string message)
        : base(message)
    {
    }

    public SeoRouteConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
