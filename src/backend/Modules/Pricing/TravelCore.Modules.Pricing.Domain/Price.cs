using TravelCore.Money;
using MoneyValue = TravelCore.Money.Money;

namespace TravelCore.Modules.Pricing.Domain;

/// <summary>
/// Buyable/executable Price aggregate (TC-P12-T003 / P12-R3).
/// Attaches via polymorphic logical <see cref="TargetType"/> + <see cref="TargetId"/> only —
/// Pricing does not know TourDeparture CLR types and owns no Tour FK.
/// Product-level pricing and Booking are out of scope here; Quote is a separate aggregate (T004).
/// </summary>
public sealed class Price
{
    private readonly List<PriceComponent> _components = [];

    private Price()
    {
        TargetType = null!;
    }

    private Price(PriceId id, PriceTargetType targetType, Guid targetId)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("PriceId cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(targetType);

        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));
        }

        Id = id;
        TargetType = targetType;
        TargetId = targetId;
    }

    public PriceId Id { get; private set; }

    /// <summary>Logical target discriminator (e.g. TourDeparture). Not a Tour module type.</summary>
    public PriceTargetType TargetType { get; private set; }

    /// <summary>Logical target identity (Guid). No EF FK to tour schema.</summary>
    public Guid TargetId { get; private set; }

    public IReadOnlyCollection<PriceComponent> Components => _components;

    public IReadOnlyList<PriceComponent> ComponentsOrdered =>
        _components.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value).ToList();

    /// <summary>Authoritative currency shared by every component on this Price.</summary>
    public CurrencyCode Currency =>
        _components.Count == 0
            ? throw new InvalidOperationException("Price has no components.")
            : _components[0].Money.Currency;

    /// <summary>
    /// Creates a Price for a polymorphic logical target with structured components.
    /// Requires ≥1 Base component and a single shared currency across all components.
    /// </summary>
    public static Price Create(
        string targetType,
        Guid targetId,
        IReadOnlyList<PriceComponentDefinition> components)
    {
        ArgumentNullException.ThrowIfNull(components);

        var parsedType = PriceTargetType.Parse(targetType);
        if (targetId == Guid.Empty)
        {
            throw new ArgumentException("TargetId cannot be empty.", nameof(targetId));
        }

        if (components.Count == 0)
        {
            throw new ArgumentException("Price requires at least one component.", nameof(components));
        }

        if (!components.Any(c => c.Kind == PriceComponentKind.Base))
        {
            throw new ArgumentException(
                "Price requires at least one Base component.",
                nameof(components));
        }

        EnsureComponentDefinitionsValid(components);

        var price = new Price(PriceId.New(), parsedType, targetId);
        foreach (var definition in components)
        {
            price._components.Add(
                PriceComponent.Create(
                    price.Id,
                    definition.Kind,
                    definition.Money,
                    definition.SortOrder,
                    definition.Code,
                    definition.Label));
        }

        return price;
    }

    /// <summary>
    /// Adds a structured component. Must match the Price currency; SortOrder/Code uniqueness enforced.
    /// </summary>
    public PriceComponent AddComponent(
        PriceComponentKind kind,
        MoneyValue money,
        int sortOrder = 0,
        string? code = null,
        string? label = null)
    {
        ArgumentNullException.ThrowIfNull(money);

        if (_components.Count > 0 && !money.Currency.Equals(Currency))
        {
            throw new ArgumentException(
                $"Component currency {money.Currency.Value} does not match Price currency {Currency.Value}.",
                nameof(money));
        }

        EnsureSortOrderAvailable(sortOrder);
        EnsureCodeAvailable(code);

        var component = PriceComponent.Create(Id, kind, money, sortOrder, code, label);
        _components.Add(component);
        return component;
    }

    private static void EnsureComponentDefinitionsValid(IReadOnlyList<PriceComponentDefinition> components)
    {
        CurrencyCode? currency = null;
        var sortOrders = new HashSet<int>();
        var codes = new HashSet<string>(StringComparer.Ordinal);

        foreach (var definition in components)
        {
            ArgumentNullException.ThrowIfNull(definition.Money);

            if (currency is null)
            {
                currency = definition.Money.Currency;
            }
            else if (!definition.Money.Currency.Equals(currency))
            {
                // یک Price عمداً چندارزی نیست؛ تبدیل FX خارج از T003 است.
                throw new ArgumentException(
                    "All PriceComponents within one Price must share the same currency.",
                    nameof(components));
            }

            if (!sortOrders.Add(definition.SortOrder))
            {
                throw new ArgumentException(
                    $"Duplicate SortOrder {definition.SortOrder} within one Price.",
                    nameof(components));
            }

            if (!string.IsNullOrWhiteSpace(definition.Code))
            {
                var normalized = definition.Code.Trim();
                if (!codes.Add(normalized))
                {
                    throw new ArgumentException(
                        $"Duplicate Code '{normalized}' within one Price.",
                        nameof(components));
                }
            }
        }
    }

    private void EnsureSortOrderAvailable(int sortOrder)
    {
        if (_components.Any(x => x.SortOrder == sortOrder))
        {
            throw new InvalidOperationException(
                $"SortOrder {sortOrder} already exists on this Price.");
        }
    }

    private void EnsureCodeAvailable(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        var normalized = code.Trim();
        if (_components.Any(x =>
                x.Code is not null
                && x.Code.Equals(normalized, StringComparison.Ordinal)))
        {
            throw new InvalidOperationException(
                $"Code '{normalized}' already exists on this Price.");
        }
    }
}
