namespace TravelCore.Modules.Booking.Domain;

/// <summary>
/// P19-R5 monetary snapshot boundary. Booking copies a valid Pricing Quote; it is not Pricing.
/// </summary>
public static class BookingMonetaryBoundary
{
    public const string PriceIsNotQuote = "Price != Quote";
    public const string QuoteIsNotBookingMonetarySnapshot = "Quote != BookingMonetarySnapshot";
    public const string BookingMonetarySnapshotIsNotPaymentAmount = "BookingMonetarySnapshot != PaymentAmount";
    public const string BookingIsNotPricingAuthority = "Booking != Pricing Authority";
    public const string QuoteExpiredIsNotBookingStatus = "QuoteExpired != BookingStatus";
    public const string QuoteExpiresAtIsNotCapacityHoldExpiresAt = "QuoteExpiresAt != CapacityHold.ExpiresAt";
    public const string BudgetPreferenceIsNotBookingMonetarySnapshot = "BudgetPreference != BookingMonetarySnapshot";
    public const string TomanIsNotCurrencyCode = "Toman != CurrencyCode";
    public const bool RecalculationImplemented = false;
    public const bool FxImplemented = false;
    public const bool RepricingImplemented = false;
    public const bool PaymentInferenceImplemented = false;
}
