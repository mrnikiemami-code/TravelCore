import type { IrrDisplayUnit, MoneyAmountString, MoneyView } from "@/types/money";

const AMOUNT_PATTERN = /^-?\d+(\.\d+)?$/;

export function assertMoneyAmount(amount: MoneyAmountString): void {
  if (!amount || !AMOUNT_PATTERN.test(amount.trim())) {
    throw new Error(`Invalid money amount string: ${amount}`);
  }
}

/**
 * Display-only IRR → Toman denomination (1 Toman = 10 IRR).
 * Not FX conversion. Caller must opt in via IrrDisplayUnit === "Toman".
 */
export function irrAmountToTomanDisplay(irrAmount: MoneyAmountString): MoneyAmountString {
  assertMoneyAmount(irrAmount);
  const raw = irrAmount.trim();
  const negative = raw.startsWith("-");
  const unsigned = negative ? raw.slice(1) : raw;
  const ten = BigInt(10);

  if (!unsigned.includes(".")) {
    const n = BigInt(unsigned);
    const whole = n / ten;
    const rem = n % ten;
    const body =
      rem === BigInt(0) ? whole.toString() : `${whole.toString()}.${rem.toString()}`;
    return negative ? `-${body}` : body;
  }

  const [w, f] = unsigned.split(".");
  const scale = ten ** BigInt(f.length);
  const asInt = BigInt(w) * scale + BigInt(f);
  const tomanInt = asInt / ten;
  const wholePart = tomanInt / scale;
  let fracPart = (tomanInt % scale).toString().padStart(f.length, "0");
  fracPart = fracPart.replace(/0+$/, "");
  const body = fracPart.length
    ? `${wholePart.toString()}.${fracPart}`
    : wholePart.toString();
  return negative ? `-${body}` : body;
}

export function formatAmountForLocale(
  amount: MoneyAmountString,
  locale: string,
): string {
  assertMoneyAmount(amount);
  const trimmed = amount.trim();
  const asNumber = Number(trimmed);
  if (Number.isFinite(asNumber) && Math.abs(asNumber) <= Number.MAX_SAFE_INTEGER) {
    const fractionDigits = trimmed.includes(".")
      ? (trimmed.split(".")[1] ?? "").replace(/0+$/, "").length
      : 0;
    return new Intl.NumberFormat(locale, {
      maximumFractionDigits: Math.max(fractionDigits, 0),
      minimumFractionDigits: 0,
    }).format(asNumber);
  }

  const neg = trimmed.startsWith("-");
  const [whole, frac] = (neg ? trimmed.slice(1) : trimmed).split(".");
  const grouped = whole.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
  const body = frac ? `${grouped}.${frac}` : grouped;
  return neg ? `-${body}` : body;
}

export type ResolvedMoneyDisplay = {
  amountText: string;
  unitLabel: string;
  currencyCode: string | null;
  accessibleText: string;
};

function unitLabels(
  locale: string,
  currencyCode: string,
  irrDisplayUnit: IrrDisplayUnit,
): { unitLabel: string; currencyCode: string | null } {
  if (currencyCode === "IRR") {
    if (irrDisplayUnit === "Toman") {
      const unitLabel = locale.startsWith("fa") ? "تومان" : "Toman";
      return { unitLabel, currencyCode: null };
    }
    const unitLabel = locale.startsWith("fa") ? "ریال" : "IRR";
    return { unitLabel, currencyCode: "IRR" };
  }

  return { unitLabel: currencyCode, currencyCode };
}

/**
 * Resolve presentation fields for a single MoneyView.
 * Locale formats digits only — it does not choose currency.
 */
export function resolveMoneyDisplay(
  money: MoneyView,
  locale: string,
  irrDisplayUnit: IrrDisplayUnit = "IRR",
): ResolvedMoneyDisplay {
  assertMoneyAmount(money.amount);

  let displayAmount = money.amount;
  if (money.currencyCode === "IRR" && irrDisplayUnit === "Toman") {
    displayAmount = irrAmountToTomanDisplay(money.amount);
  }

  const amountText = formatAmountForLocale(displayAmount, locale);
  const { unitLabel, currencyCode } = unitLabels(
    locale,
    money.currencyCode,
    money.currencyCode === "IRR" ? irrDisplayUnit : "IRR",
  );

  const accessibleText = `${amountText} ${unitLabel}`;

  return { amountText, unitLabel, currencyCode, accessibleText };
}
