import type { ReactNode } from "react";
import { MoneyText } from "@/components/ui/money-text";
import { Stack } from "@/components/ui/stack";
import { cn } from "@/lib/ui/cn";
import type { IrrDisplayUnit, MixedCurrencyPriceView } from "@/types/money";

export type MixedCurrencyPriceProps = {
  price: MixedCurrencyPriceView;
  /** Locale for digit formatting only — does not select or convert currency. */
  locale: string;
  /** Explicit IRR denomination for any IRR components. */
  irrDisplayUnit?: IrrDisplayUnit;
  className?: string;
};

/**
 * Presents multiple authoritative money components as supplied.
 * Does NOT convert between currencies or invent a total.
 */
export function MixedCurrencyPrice({
  price,
  locale,
  irrDisplayUnit = "IRR",
  className,
}: MixedCurrencyPriceProps): ReactNode {
  const items = price.components;

  if (items.length === 0) {
    return null;
  }

  return (
    <Stack gap="sm" className={cn("w-full", className)}>
      {items.map((component, index) => (
        <div
          key={`${component.currencyCode}-${component.amount}-${component.purpose ?? index}`}
          className="flex max-w-full flex-wrap items-baseline gap-x-2 gap-y-1"
        >
          {index > 0 ? (
            <span className="text-muted-foreground" aria-hidden="true">
              +
            </span>
          ) : null}
          <MoneyText
            money={component}
            locale={locale}
            irrDisplayUnit={irrDisplayUnit}
          />
          {component.purpose ? (
            <span className="text-caption text-muted-foreground">
              ({component.purpose})
            </span>
          ) : null}
        </div>
      ))}
    </Stack>
  );
}
