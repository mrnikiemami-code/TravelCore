namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Structured PriceComponent kind (TC-P12-T003) — not an opaque blob.
/// </summary>
public enum PriceComponentKind : short
{
    Base = 0,
    Fee = 1,
    Tax = 2
}
