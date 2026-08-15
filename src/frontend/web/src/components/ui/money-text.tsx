import type { ReactNode } from "react";
import { LtrValue } from "@/components/ui/bidi-text";
import { cn } from "@/lib/ui/cn";
import { resolveMoneyDisplay } from "@/lib/formatting/money";
import type { IrrDisplayUnit, MoneyView } from "@/types/money";

export type MoneyTextProps = {
  money: MoneyView;
  /** Locale for digit/number formatting only — never selects currency. */
  locale: string;
  /** Explicit IRR denomination when currency is IRR. Default: canonical IRR. */
  irrDisplayUnit?: IrrDisplayUnit;
  className?: string;
};

/**
 * Single-money presentation. Display-only — no FX, no pricing authority.
 */
export function MoneyText({
  money,
  locale,
  irrDisplayUnit = "IRR",
  className,
}: MoneyTextProps): ReactNode {
  const resolved = resolveMoneyDisplay(money, locale, irrDisplayUnit);

  return (
    <span
      className={cn(
        "inline-flex max-w-full flex-wrap items-baseline gap-x-1 text-body text-foreground",
        className,
      )}
    >
      <span className="sr-only">{resolved.accessibleText}</span>
      <span
        aria-hidden="true"
        className="inline-flex max-w-full flex-wrap items-baseline gap-x-1"
      >
        <span className="tabular-nums">{resolved.amountText}</span>
        {resolved.currencyCode ? (
          <LtrValue>{resolved.currencyCode}</LtrValue>
        ) : (
          <span>{resolved.unitLabel}</span>
        )}
      </span>
    </span>
  );
}
