"use client";

import { useCallback, useEffect, useMemo } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { AgencyOfferView } from "./load-agency-offers";

export const AGENCY_OFFER_QUERY_KEY = "agencyOfferId";

/**
 * P38-T004: Customer AgencyOffer selection foundation.
 * URL is source of truth for selected offer id (booking boundary prep).
 */
export function AgencyOffersList({
  locale,
  items,
}: {
  locale: AppLocale;
  items: AgencyOfferView[];
}) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const fromUrl = searchParams.get(AGENCY_OFFER_QUERY_KEY);

  const ids = useMemo(() => new Set(items.map((i) => i.agencyOfferId)), [items]);

  const selectedId = useMemo(() => {
    if (items.length === 0) return null;
    if (fromUrl && ids.has(fromUrl)) return fromUrl;
    if (items.length === 1) return items[0]!.agencyOfferId;
    return null;
  }, [fromUrl, ids, items]);

  const writeUrl = useCallback(
    (offerId: string | null) => {
      const params = new URLSearchParams(searchParams.toString());
      if (offerId) {
        params.set(AGENCY_OFFER_QUERY_KEY, offerId);
      } else {
        params.delete(AGENCY_OFFER_QUERY_KEY);
      }
      const qs = params.toString();
      router.replace(qs ? `${pathname}?${qs}` : pathname, { scroll: false });
    },
    [pathname, router, searchParams],
  );

  useEffect(() => {
    if (items.length === 0) {
      if (fromUrl) writeUrl(null);
      return;
    }
    if (items.length === 1) {
      const only = items[0]!.agencyOfferId;
      if (fromUrl !== only) writeUrl(only);
      return;
    }
    if (fromUrl && !ids.has(fromUrl)) {
      writeUrl(null);
    }
  }, [fromUrl, ids, items, writeUrl]);

  const title =
    locale === "fa"
      ? "پیشنهادهای آژانس"
      : locale === "ar"
        ? "عروض الوكالة"
        : "Agency offers";
  const caption =
    locale === "fa"
      ? "یک پیشنهاد منتشرشده را انتخاب کنید · هویت پیشنهاد برای مرز رزرو حفظ می‌شود · نه قیمت جعلی و نه کمیسیون"
      : "Select a published offer · offer identity is kept for the booking boundary · no invented prices or commissions";
  const selectLabel =
    locale === "fa" ? "انتخاب این پیشنهاد" : "Select this offer";
  const selectedLabel = locale === "fa" ? "انتخاب‌شده" : "Selected";

  if (items.length === 0) {
    return null;
  }

  return (
    <Stack gap="sm">
      <Text as="h2" role="heading">
        {title}
      </Text>
      <Text role="caption">{caption}</Text>
      <fieldset className="m-0 border-0 p-0">
        <legend className="sr-only">{title}</legend>
        <ul className="flex flex-col gap-3">
          {items.map((item) => {
            const checked = item.agencyOfferId === selectedId;
            return (
              <li key={item.agencyOfferId}>
                <label
                  className={`block cursor-pointer rounded-2xl border p-4 transition-colors ${
                    checked
                      ? "border-[#1D4ED8] bg-[#1D4ED8]/5 shadow-sm"
                      : "border-border bg-surface hover:border-[#1D4ED8]/40"
                  }`}
                >
                  <div className="flex items-start gap-3">
                    <input
                      type="radio"
                      className="mt-1"
                      name="agency-offer"
                      value={item.agencyOfferId}
                      checked={checked}
                      onChange={() => writeUrl(item.agencyOfferId)}
                    />
                    <Stack gap="sm" className="min-w-0 flex-1">
                      <Text>{item.agencyDisplayName}</Text>
                      {item.titleOverride ? (
                        <Text role="caption">{item.titleOverride}</Text>
                      ) : null}
                      {item.highlight ? <Text>{item.highlight}</Text> : null}
                      {item.agencyDescription ? (
                        <Text role="muted">{item.agencyDescription}</Text>
                      ) : null}
                      {(item.publicEmail || item.publicPhone || item.websiteUrl) && (
                        <ul className="list-inside list-disc text-sm">
                          {item.publicEmail ? (
                            <li>
                              <LtrValue>{item.publicEmail}</LtrValue>
                            </li>
                          ) : null}
                          {item.publicPhone ? (
                            <li>
                              <LtrValue>{item.publicPhone}</LtrValue>
                            </li>
                          ) : null}
                          {item.websiteUrl ? (
                            <li>
                              <LtrValue>{item.websiteUrl}</LtrValue>
                            </li>
                          ) : null}
                        </ul>
                      )}
                      {item.requiresManualConfirmation ? (
                        <Text role="caption">
                          {locale === "fa"
                            ? "نیاز به تأیید دستی آژانس · نه رزرو خودکار"
                            : "Manual agency confirmation · not an automatic reservation"}
                        </Text>
                      ) : null}
                      <Text role="caption">
                        {checked ? selectedLabel : selectLabel}
                      </Text>
                    </Stack>
                  </div>
                </label>
              </li>
            );
          })}
        </ul>
      </fieldset>
    </Stack>
  );
}
