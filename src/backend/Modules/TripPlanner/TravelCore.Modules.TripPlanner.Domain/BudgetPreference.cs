using TravelCore.Money;

namespace TravelCore.Modules.TripPlanner.Domain;

/// <summary>
/// Total-trip budget intent (P18-R4). Not Price/Quote/PaymentAmount.
/// </summary>
public sealed class BudgetPreference
{
    public const int AmountScale = 2;

    private BudgetPreference()
    {
        CurrencyCode = null!;
    }

    private BudgetPreference(decimal? minimumAmount, decimal? maximumAmount, CurrencyCode currencyCode)
    {
        MinimumAmount = minimumAmount;
        MaximumAmount = maximumAmount;
        CurrencyCode = currencyCode;
    }

    public decimal? MinimumAmount { get; private set; }

    public decimal? MaximumAmount { get; private set; }

    public CurrencyCode CurrencyCode { get; private set; }

    public static BudgetPreference Create(
        CurrencyCode currencyCode,
        decimal? minimumAmount = null,
        decimal? maximumAmount = null)
    {
        ArgumentNullException.ThrowIfNull(currencyCode);
        if (minimumAmount is < 0 or > 999_999_999_999.99m)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumAmount));
        }

        if (maximumAmount is < 0 or > 999_999_999_999.99m)
        {
            throw new ArgumentOutOfRangeException(nameof(maximumAmount));
        }

        if (minimumAmount is null && maximumAmount is null)
        {
            throw new ArgumentException("Budget preference requires minimum and/or maximum amount.");
        }

        if (minimumAmount is not null && maximumAmount is not null && maximumAmount < minimumAmount)
        {
            throw new ArgumentException("Maximum budget must be greater than or equal to minimum budget.");
        }

        return new BudgetPreference(minimumAmount, maximumAmount, currencyCode);
    }

    internal BudgetPreference CaptureCopy() => new(MinimumAmount, MaximumAmount, CurrencyCode);
}
