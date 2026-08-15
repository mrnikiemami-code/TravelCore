/**
 * Frontend presentation contracts for Money.
 * Authoritative Money rules live in ADR 0003 / docs — UI only displays supplied values.
 */

/** ISO-like currency code. TOMAN is NOT a currency code. */
export type CurrencyCode = string;

/**
 * Decimal amount as a string (never float/number in the contract).
 * Canonical units for the given currencyCode (IRR rials, USD major units as supplied).
 */
export type MoneyAmountString = string;

export type MoneyView = {
  amount: MoneyAmountString;
  currencyCode: CurrencyCode;
};

/** Explicit IRR display denomination — never implied silently by locale alone. */
export type IrrDisplayUnit = "IRR" | "Toman";

export type PriceComponentView = MoneyView & {
  /** Optional purpose label already decided upstream (display only). */
  purpose?: string;
};

export type MixedCurrencyPriceView = {
  /** Authoritative components supplied by read/view model — UI does not convert between them. */
  components: PriceComponentView[];
};
