import { Container, Stack, Text } from "@/components/ui";
import { HotelCard } from "@/features/hotel-discovery/hotel-card";
import {
  applyHotelListingCriteria,
  type HotelListingCriteria,
} from "@/features/hotel-discovery/hotel-listing-criteria";
import { HotelListingToolbar } from "@/features/hotel-discovery/hotel-listing-toolbar";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";
import type { AppLocale } from "@/lib/i18n";

/**
 * Hotel commerce listing experience (TC-P30-T006).
 * Place catalog discovery · not Search · not HotelBooking availability.
 */
export function HotelDiscoveryView({
  locale,
  hotels,
  criteria,
  loadError,
}: {
  locale: AppLocale;
  hotels: HotelBrowseItemView[];
  criteria: HotelListingCriteria;
  loadError: boolean;
}) {
  const title = locale === "fa" ? "هتل‌ها" : locale === "ar" ? "الفنادق" : "Hotels";
  const filtered = applyHotelListingCriteria(hotels, criteria);

  const copy =
    locale === "fa"
      ? {
          blurb:
            "کشف کاتالوگ Place · تجربه تجاری هتل · نه موتور availability و نه قیمت جعلی",
          emptyTitle: "هتلی برای نمایش نیست",
          emptyBody:
            "برای این locale هتل فعالی با slug نیست، یا فیلتر نتیجه‌ای ندارد. قیمت/موجودی جعلی نشان نمی‌دهیم.",
          errorTitle: "بارگذاری کاتالوگ ناموفق بود",
          errorBody: "لطفاً بعداً دوباره تلاش کنید. رزرو و availability در این لایه اجرا نمی‌شود.",
          count: (n: number) => `${n} هتل`,
        }
      : locale === "ar"
        ? {
            blurb:
              "اكتشاف كتالوج Place · تجربة فنادق تجارية · دون محرك توفر أو أسعار وهمية",
            emptyTitle: "لا فنادق للعرض",
            emptyBody:
              "لا فنادق نشطة لهذا الإعداد المحلي، أو لا نتائج للتصفية. لا نعرض أسعاراً/توفراً وهمياً.",
            errorTitle: "تعذر تحميل الكتالوج",
            errorBody: "حاول لاحقاً. الحجز والتوفر ليسا في هذه الطبقة.",
            count: (n: number) => `${n} فندق`,
          }
        : {
            blurb:
              "Place catalog discovery · hotel commerce experience · not availability search · no invented prices",
            emptyTitle: "No hotels to show",
            emptyBody:
              "No active hotels with slug for this locale, or the filter matched nothing. We do not invent prices or availability.",
            errorTitle: "Catalog failed to load",
            errorBody:
              "Please try again later. Booking and availability are not handled on this layer.",
            count: (n: number) => `${n} hotels`,
          };

  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {title}
            </Text>
            <Text role="caption">{copy.blurb}</Text>
          </Stack>

          <HotelListingToolbar locale={locale} criteria={criteria} />

          {loadError ? (
            <div
              role="alert"
              className="rounded-xl border border-border bg-surface p-6 shadow-sm"
            >
              <Text as="h2" role="label">
                {copy.errorTitle}
              </Text>
              <Text role="muted" className="mt-2">
                {copy.errorBody}
              </Text>
            </div>
          ) : filtered.length === 0 ? (
            <div className="rounded-xl border border-dashed border-border bg-surface/60 p-8 text-center">
              <Text as="h2" role="label">
                {copy.emptyTitle}
              </Text>
              <Text role="muted" className="mt-2">
                {copy.emptyBody}
              </Text>
              <div className="mt-6 grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {[0, 1, 2].map((i) => (
                  <div
                    key={i}
                    className="aspect-[4/3] rounded-xl border border-border bg-muted/40"
                    aria-hidden
                  />
                ))}
              </div>
            </div>
          ) : (
            <Stack gap="md">
              <Text role="caption">{copy.count(filtered.length)}</Text>
              <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {filtered.map((item) => (
                  <li key={item.placeId}>
                    <HotelCard locale={locale} hotel={item} />
                  </li>
                ))}
              </ul>
            </Stack>
          )}
        </Stack>
      </Container>
    </div>
  );
}
