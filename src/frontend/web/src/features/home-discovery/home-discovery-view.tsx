import Link from "next/link";
import { Container, LtrValue, Stack, Surface, Text } from "@/components/ui";
import type { HomeDiscoveryComposition } from "@/features/home-discovery/types";
import type { AppLocale } from "@/lib/i18n";

type DiscoveryLink = {
  href: string;
  label: string;
  hint: string;
};

export type HomeDiscoveryViewProps = {
  locale: AppLocale;
  composition?: HomeDiscoveryComposition;
  /** UIVAL dev route may include `/dev/*` links; production home must not. */
  includeDevLinks?: boolean;
};

function productionLinks(locale: AppLocale): DiscoveryLink[] {
  if (locale === "fa") {
    return [
      { href: `/${locale}/tours`, label: "فهرست تورها", hint: "کشف تور" },
      { href: `/${locale}/hotels`, label: "هتل‌ها", hint: "Hotel catalog browse" },
      { href: `/${locale}/travelogues`, label: "سفرنامه‌ها", hint: "UGC travelogues" },
      { href: `/${locale}/plan`, label: "برنامه‌ریز سفر", hint: "Trip Planner" },
      { href: `/${locale}/flights`, label: "جستجوی پرواز", hint: "Flight search" },
      { href: `/${locale}/visas/TR`, label: "ویزا", hint: "Visa information" },
    ];
  }
  if (locale === "ar") {
    return [
      { href: `/${locale}/tours`, label: "قائمة الجولات", hint: "Tour discovery" },
      { href: `/${locale}/hotels`, label: "الفنادق", hint: "Hotel catalog browse" },
      { href: `/${locale}/travelogues`, label: "Travelogues", hint: "UGC travelogues" },
      { href: `/${locale}/plan`, label: "مخطط الرحلة", hint: "Trip planner" },
      { href: `/${locale}/flights`, label: "البحث عن رحلات", hint: "Flight search" },
      { href: `/${locale}/visas/TR`, label: "التأشيرة", hint: "Visa information" },
    ];
  }
  return [
    { href: `/${locale}/tours`, label: "Tour listing", hint: "Tour discovery" },
    { href: `/${locale}/hotels`, label: "Hotels", hint: "Hotel catalog browse" },
    { href: `/${locale}/travelogues`, label: "Travelogues", hint: "UGC travelogues" },
    { href: `/${locale}/plan`, label: "Trip planner", hint: "Lead experience" },
    { href: `/${locale}/flights`, label: "Flight search", hint: "Flight booking search" },
    { href: `/${locale}/visas/TR`, label: "Visa", hint: "Visa information" },
  ];
}

function devValidationLinks(locale: AppLocale): DiscoveryLink[] {
  return [
    {
      href: `/${locale}/destinations/fixture-istanbul`,
      label: locale === "fa" ? "مقصد نمونه (fixture)" : "Sample destination (fixture)",
      hint: "Destination landing fixture",
    },
    {
      href: `/${locale}/dev/foundation`,
      label: locale === "fa" ? "اعتبارسنجی primitives" : "Foundation validation",
      hint: "dev-only",
    },
  ];
}

function sectionCopy(locale: AppLocale) {
  if (locale === "fa") {
    return {
      paths: "مسیرهای کشف",
      travelogues: "سفرنامه‌های اخیر",
      hotels: "هتل‌های فعال",
      seeAllTravelogues: "همه سفرنامه‌ها",
      seeAllHotels: "همه هتل‌ها",
      noTravelogues: "سفرنامه‌ای برای پیش‌نمایش نیست.",
      noHotels: "هتلی برای پیش‌نمایش نیست.",
    };
  }
  if (locale === "ar") {
    return {
      paths: "مسارات الاكتشاف",
      travelogues: "Travelogues حديثة",
      hotels: "فنادق نشطة",
      seeAllTravelogues: "كل Travelogues",
      seeAllHotels: "كل الفنادق",
      noTravelogues: "لا travelogues للمعاينة.",
      noHotels: "لا فنادق للمعاينة.",
    };
  }
  return {
    paths: "Discovery paths",
    travelogues: "Recent travelogues",
    hotels: "Active hotels",
    seeAllTravelogues: "All travelogues",
    seeAllHotels: "All hotels",
    noTravelogues: "No travelogues to preview.",
    noHotels: "No hotels to preview.",
  };
}

/**
 * Home / Discovery entry surface (Server Component).
 * Curated composition from public loaders — not a personalized feed or search engine.
 */
export function HomeDiscoveryView({
  locale,
  composition,
  includeDevLinks = false,
}: HomeDiscoveryViewProps) {
  const title =
    locale === "fa"
      ? "کشف TravelCore"
      : locale === "ar"
        ? "اكتشف TravelCore"
        : "Discover TravelCore";
  const subtitle =
    locale === "fa"
      ? "ترکیب کشف عمومی — نه فید شخصی‌سازی‌شده · نه موتور جستجو"
      : locale === "ar"
        ? "تكوين اكتشاف عام — وليس خلاصة مخصصة · ليس محرك بحث"
        : "Public discovery composition — not a personalized feed · not a search engine";
  const copy = sectionCopy(locale);

  const links = includeDevLinks
    ? [...productionLinks(locale), ...devValidationLinks(locale)]
    : productionLinks(locale);

  const travelogues = composition?.travelogues ?? [];
  const hotels = composition?.hotels ?? [];

  return (
    <div className="py-8">
      <Container width="content">
        <Stack gap="lg">
          <Surface>
            <Stack gap="sm">
              <Text as="h1" role="heading">
                {title}
              </Text>
              <Text role="muted">{subtitle}</Text>
              <Text role="caption">
                locale=<LtrValue>{locale}</LtrValue>
              </Text>
            </Stack>
          </Surface>

          <Stack gap="md">
            <Text as="h2" role="title">
              {copy.travelogues}
            </Text>
            {travelogues.length === 0 ? (
              <Text role="muted">{copy.noTravelogues}</Text>
            ) : (
              <ul className="flex flex-col gap-2">
                {travelogues.map((item) => (
                  <li key={item.travelogueId}>
                    <Link
                      href={`/${locale}/travelogues/${encodeURIComponent(item.travelogueId)}`}
                      className="min-h-touch block rounded-md border border-border px-4 py-3 underline-offset-2 hover:underline"
                    >
                      <Text role="label">{item.title}</Text>
                      <Text role="caption">{item.body.slice(0, 120)}</Text>
                    </Link>
                  </li>
                ))}
              </ul>
            )}
            <Link
              href={`/${locale}/travelogues`}
              className="min-h-touch inline-flex w-fit underline-offset-2 hover:underline"
            >
              {copy.seeAllTravelogues}
            </Link>
          </Stack>

          <Stack gap="md">
            <Text as="h2" role="title">
              {copy.hotels}
            </Text>
            {hotels.length === 0 ? (
              <Text role="muted">{copy.noHotels}</Text>
            ) : (
              <ul className="flex flex-col gap-2">
                {hotels.map((item) => (
                  <li key={item.placeId}>
                    <Link
                      href={`/${locale}/hotels/${encodeURIComponent(item.slug)}`}
                      className="min-h-touch block rounded-md border border-border px-4 py-3 underline-offset-2 hover:underline"
                    >
                      <Text role="label">{item.name}</Text>
                      <Text role="caption">
                        <LtrValue>{item.slug}</LtrValue>
                      </Text>
                    </Link>
                  </li>
                ))}
              </ul>
            )}
            <Link
              href={`/${locale}/hotels`}
              className="min-h-touch inline-flex w-fit underline-offset-2 hover:underline"
            >
              {copy.seeAllHotels}
            </Link>
          </Stack>

          <Stack gap="md">
            <Text as="h2" role="title">
              {copy.paths}
            </Text>
            <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              {links.map((item) => (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    className="min-h-touch flex flex-col rounded-md border border-border px-4 py-3 underline-offset-2 hover:underline"
                  >
                    <Text role="label">{item.label}</Text>
                    <Text role="caption">
                      <LtrValue>{item.hint}</LtrValue>
                    </Text>
                  </Link>
                </li>
              ))}
            </ul>
          </Stack>
        </Stack>
      </Container>
    </div>
  );
}
