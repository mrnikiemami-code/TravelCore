import Link from "next/link";
import { Container, LtrValue, Stack, Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

type DiscoveryLink = {
  href: string;
  label: string;
  hint: string;
};

export type HomeDiscoveryViewProps = {
  locale: AppLocale;
  /** UIVAL dev route may include `/dev/*` links; production home must not. */
  includeDevLinks?: boolean;
};

function productionLinks(locale: AppLocale): DiscoveryLink[] {
  if (locale === "fa") {
    return [
      { href: `/${locale}/tours`, label: "فهرست تورها", hint: "کشف تور" },
      { href: `/${locale}/travelogues`, label: "سفرنامه‌ها", hint: "UGC travelogues" },
      { href: `/${locale}/plan`, label: "برنامه‌ریز سفر", hint: "Trip Planner" },
      { href: `/${locale}/flights`, label: "جستجوی پرواز", hint: "Flight search" },
      { href: `/${locale}/visas/TR`, label: "ویزا", hint: "Visa information" },
    ];
  }
  if (locale === "ar") {
    return [
      { href: `/${locale}/tours`, label: "قائمة الجولات", hint: "Tour discovery" },
      { href: `/${locale}/travelogues`, label: "Travelogues", hint: "UGC travelogues" },
      { href: `/${locale}/plan`, label: "مخطط الرحلة", hint: "Trip planner" },
      { href: `/${locale}/flights`, label: "البحث عن رحلات", hint: "Flight search" },
      { href: `/${locale}/visas/TR`, label: "التأشيرة", hint: "Visa information" },
    ];
  }
  return [
    { href: `/${locale}/tours`, label: "Tour listing", hint: "Tour discovery" },
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

/**
 * Home / Discovery entry surface (Server Component).
 * Workflow entry points — not a personalized feed or search engine.
 */
export function HomeDiscoveryView({
  locale,
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
      ? "ورودی‌های عمومی محصول — نه فید شخصی‌سازی‌شده"
      : locale === "ar"
        ? "نقاط دخول عامة للمنتج — وليس خلاصة مخصصة"
        : "Public product entry points — not a personalized feed";

  const links = includeDevLinks
    ? [...productionLinks(locale), ...devValidationLinks(locale)]
    : productionLinks(locale);

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
              {locale === "fa"
                ? "مسیرهای کشف"
                : locale === "ar"
                  ? "مسارات الاكتشاف"
                  : "Discovery paths"}
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
