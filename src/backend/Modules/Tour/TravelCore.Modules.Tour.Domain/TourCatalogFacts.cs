namespace TravelCore.Modules.Tour.Domain;

/// <summary>
/// Shared opaque catalog-fact code rules for Tour services / policies / requirements (TC-P09-T006).
/// Descriptive TourProduct facts only — not Booking/Payment/Pricing engines.
/// </summary>
public static class TourCatalogFactCode
{
    public const int CodeMaxLength = 64;
    public const int DetailMaxLength = 2000;
    public const int MaxEntriesPerKind = 32;

    public static string NormalizeCode(string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        var trimmed = code.Trim().ToLowerInvariant();
        if (trimmed.Length > CodeMaxLength)
        {
            throw new ArgumentException($"Catalog fact code max length is {CodeMaxLength}.", nameof(code));
        }

        if (trimmed.Any(static c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
        {
            throw new ArgumentException(
                "Catalog fact code may contain only a-z, 0-9, hyphen, and underscore.",
                nameof(code));
        }

        return trimmed;
    }

    public static string? NormalizeDetail(string? detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
        {
            return null;
        }

        var trimmed = detail.Trim();
        if (trimmed.Length > DetailMaxLength)
        {
            throw new ArgumentException(
                $"Catalog fact detail max length is {DetailMaxLength}.",
                nameof(detail));
        }

        return trimmed;
    }
}

/// <summary>Tour-owned included/service fact (e.g. insurance, transfer, meals).</summary>
public sealed class TourProductService
{
    private TourProductService()
    {
        Code = null!;
    }

    private TourProductService(TourProductId tourProductId, string code, string? detail)
    {
        TourProductId = tourProductId;
        Code = code;
        Detail = detail;
    }

    public TourProductId TourProductId { get; private set; }
    public string Code { get; private set; }
    public string? Detail { get; private set; }

    internal static TourProductService Create(TourProductId tourProductId, string code, string? detail)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        return new TourProductService(
            tourProductId,
            TourCatalogFactCode.NormalizeCode(code),
            TourCatalogFactCode.NormalizeDetail(detail));
    }
}

/// <summary>Tour-owned policy fact (e.g. cancellation terms) — not Booking cancellation workflow.</summary>
public sealed class TourProductPolicy
{
    private TourProductPolicy()
    {
        Code = null!;
    }

    private TourProductPolicy(TourProductId tourProductId, string code, string? detail)
    {
        TourProductId = tourProductId;
        Code = code;
        Detail = detail;
    }

    public TourProductId TourProductId { get; private set; }
    public string Code { get; private set; }
    public string? Detail { get; private set; }

    internal static TourProductPolicy Create(TourProductId tourProductId, string code, string? detail)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        return new TourProductPolicy(
            tourProductId,
            TourCatalogFactCode.NormalizeCode(code),
            TourCatalogFactCode.NormalizeDetail(detail));
    }
}

/// <summary>Tour-owned requirement / eligibility fact (e.g. visa, passport) — not Payment rules.</summary>
public sealed class TourProductRequirement
{
    private TourProductRequirement()
    {
        Code = null!;
    }

    private TourProductRequirement(TourProductId tourProductId, string code, string? detail)
    {
        TourProductId = tourProductId;
        Code = code;
        Detail = detail;
    }

    public TourProductId TourProductId { get; private set; }
    public string Code { get; private set; }
    public string? Detail { get; private set; }

    internal static TourProductRequirement Create(TourProductId tourProductId, string code, string? detail)
    {
        if (tourProductId.Value == Guid.Empty)
        {
            throw new ArgumentException("TourProductId cannot be empty.", nameof(tourProductId));
        }

        return new TourProductRequirement(
            tourProductId,
            TourCatalogFactCode.NormalizeCode(code),
            TourCatalogFactCode.NormalizeDetail(detail));
    }
}

/// <summary>Input row for Replace* catalog-fact APIs.</summary>
public readonly record struct TourCatalogFactInput(string Code, string? Detail = null);
