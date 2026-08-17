import { LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { AgencyOfferView } from "./load-agency-offers";

/**
 * P14-R7: Inquiry-oriented Agency Information. Marketplace owns facts; PE composes only.
 * No commercial CTA, no agency-controlled prices, no ranking.
 */
export function AgencyOffersList({
  locale,
  items,
}: {
  locale: AppLocale;
  items: AgencyOfferView[];
}) {
  return (
    <Stack gap="sm">
      <Text as="h2" role="heading">
        {locale === "fa" ? "اطلاعات آژانس" : "Agency information"}
      </Text>
      <Text role="caption">
        {locale === "fa"
          ? "نمایش آگهی منتشرشده · بدون رزرو و پرداخت"
          : "Published offer display · inquiry only, not a sale"}
      </Text>
      {items.length === 0 ? (
        <Text role="muted">
          {locale === "fa"
            ? "آژانس منتشرشده‌ای برای این تور نیست."
            : "No published agency offers for this tour."}
        </Text>
      ) : (
        <ul className="flex flex-col gap-3">
          {items.map((item) => (
            <li
              key={item.agencyOfferId}
              className="rounded-md border border-border p-3 text-sm"
            >
              <Stack gap="sm">
                <Text>{item.agencyDisplayName}</Text>
                {item.titleOverride ? (
                  <Text role="caption">{item.titleOverride}</Text>
                ) : null}
                {item.highlight ? <Text>{item.highlight}</Text> : null}
                {item.agencyDescription ? (
                  <Text role="muted">{item.agencyDescription}</Text>
                ) : null}
                {(item.publicEmail || item.publicPhone || item.websiteUrl) && (
                  <ul className="list-inside list-disc">
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
                <a
                  className="min-h-touch inline-flex underline-offset-2 hover:underline"
                  href="#request-information"
                >
                  {locale === "fa" ? "درخواست اطلاعات" : "Request information"}
                </a>
              </Stack>
            </li>
          ))}
        </ul>
      )}
    </Stack>
  );
}
