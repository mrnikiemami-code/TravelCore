import Link from "next/link";
import { Container, LtrValue, Stack, Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";

/**
 * Discovery index for Active hotels — P07 catalog browse · not Search · not availability.
 */
export function HotelDiscoveryView({
  locale,
  hotels,
}: {
  locale: AppLocale;
  hotels: HotelBrowseItemView[];
}) {
  const title = locale === "fa" ? "هتل‌ها" : locale === "ar" ? "الفنادق" : "Hotels";

  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {title}
            </Text>
            <Text role="caption">
              {locale === "fa"
                ? "کشف کاتالوگ Place · نه موتور جستجو · نه availability رزرو"
                : locale === "ar"
                  ? "اكتشاف كatalog Place · ليس محرك بحث · ليس توفر الحجز"
                  : "Place catalog discovery · not a search engine · not booking availability"}
            </Text>
          </Stack>

          {hotels.length === 0 ? (
            <Text role="muted">
              {locale === "fa"
                ? "هتل فعالی با slug برای این locale یافت نشد."
                : "No active hotels with slug for this locale."}
            </Text>
          ) : (
            <ul className="flex flex-col gap-3">
              {hotels.map((item) => (
                <li key={item.placeId}>
                  <Surface>
                    <Stack gap="sm">
                      <Link
                        href={`/${locale}/hotels/${encodeURIComponent(item.slug)}`}
                        className="min-h-touch inline-flex underline-offset-2 hover:underline"
                      >
                        <Text role="label">{item.name}</Text>
                      </Link>
                      {item.description ? (
                        <Text role="muted">{item.description.slice(0, 200)}</Text>
                      ) : null}
                      <Text role="caption">
                        {item.starRating != null ? (
                          <>
                            {locale === "fa" ? "ستاره" : "Stars"}: {item.starRating}
                            {" · "}
                          </>
                        ) : null}
                        <LtrValue>{item.slug}</LtrValue>
                      </Text>
                    </Stack>
                  </Surface>
                </li>
              ))}
            </ul>
          )}
        </Stack>
      </Container>
    </div>
  );
}
