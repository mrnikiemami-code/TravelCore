using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Factory input for a structured <see cref="PriceComponent"/> (kind + money + optional identity aids).
/// </summary>
public sealed record PriceComponentDefinition(
    PriceComponentKind Kind,
    MoneyValue Money,
    int SortOrder = 0,
    string? Code = null,
    string? Label = null);
