import Link from "next/link";
import { Container, LtrValue, Stack, Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

type DiscoveryLink = {
  href: string;
  label: string;
  hint: string;
};

/**
 * UIVAL-T007 Home / Discovery entry surface (Server Component).
 * Workflow entry points — not a personalized feed or search engine.
 */
export function HomeDiscoveryView({ locale }: { locale: AppLocale }) {
  const title =
    locale === "fa" ? "کشف TravelCore" : "Discover TravelCore";
  const subtitle =
    locale === "fa"
      ? "ورودی‌های عمومی محصول — نه فید شخصی‌سازی‌شده"
      : "Public product entry points — not a personalized feed";

  const links: DiscoveryLink[] =
    locale === "fa"
      ? [
          { href: `/${locale}/tours`, label: "فهرست تورها", hint: "کشف تور" },
          { href: `/${locale}/plan`, label: "برنامه‌ریز سفر", hint: "Trip Planner" },
          {
            href: `/${locale}/destinations/fixture-istanbul`,
            label: "مقصد نمونه",
            hint: "Destination landing",
          },
          { href: `/${locale}/flights`, label: "جستجوی پرواز", hint: "Flight search" },
          { href: `/${locale}/dev/foundation`, label: "اعتبارسنجی primitives", hint: "dev-only" },
        ]
      : [
          { href: `/${locale}/tours`, label: "Tour listing", hint: "Tour discovery" },
          { href: `/${locale}/plan`, label: "Trip planner", hint: "Lead experience" },
          {
            href: `/${locale}/destinations/fixture-istanbul`,
            label: "Sample destination",
            hint: "Destination landing",
          },
          { href: `/${locale}/flights`, label: "Flight search", hint: "Flight booking search" },
          { href: `/${locale}/dev/foundation`, label: "Foundation validation", hint: "dev-only" },
        ];

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
              {locale === "fa" ? "مسیرهای کشف" : "Discovery paths"}
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
