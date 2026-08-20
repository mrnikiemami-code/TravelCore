import Link from "next/link";
import { notFound } from "next/navigation";
import { AgencyShell } from "@/components/shell";
import { Stack, Surface, Text } from "@/components/ui";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata = {
  robots: { index: false, follow: false },
};

const COPY = {
  fa: {
    title: "داشبورد فروش",
    body: "سطح عملیاتی Agency Marketplace: مدیریت پروفایل، Offer و چرخهٔ انتشار. بدون Booking/Payment/Commission و بدون مالکیت SEO.",
    note: "مالکیت با ماژول Agency Marketplace است — نه Tour Admin و نه Identity. Published Offer ≠ SEO Indexed.",
    profile: "پروفایل تجاری",
    offers: "آگهی‌های فروش",
    publish: "انتشار و بازبینی",
  },
  en: {
    title: "Sales dashboard",
    body: "Agency Marketplace operational surface: profile, offer management, and publication workflow. No Booking/Payment/Commission and no SEO ownership.",
    note: "Owned by the Agency Marketplace module — not Tour Admin and not Identity. Published Offer ≠ SEO Indexed.",
    profile: "Commercial profile",
    offers: "Sales offers",
    publish: "Publish and moderate",
  },
} as const;

export default async function AgencyPanelPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;
  const copy = locale === "fa" ? COPY.fa : COPY.en;

  return (
    <AgencyShell locale={locale} title={copy.title}>
      <div className="p-4">
        <Stack gap="md">
          <Surface className="p-4">
            <Text as="p" role="body">
              {copy.body}
            </Text>
            <Text as="p" role="caption" className="mt-2 text-muted-foreground">
              {copy.note}
            </Text>
          </Surface>
          <ul className="grid gap-3 sm:grid-cols-3">
            <li>
              <Surface className="p-4">
                <Text as="h2" role="title">
                  {copy.profile}
                </Text>
              </Surface>
            </li>
            <li>
              <Surface className="p-4">
                <Text as="h2" role="title">
                  {copy.offers}
                </Text>
              </Surface>
            </li>
            <li>
              <Surface className="p-4">
                <Text as="h2" role="title">
                  {copy.publish}
                </Text>
              </Surface>
            </li>
          </ul>
          <Link
            className="min-h-touch inline-flex items-center text-sm text-primary underline-offset-2 hover:underline"
            href={`/${locale}`}
          >
            TravelCore
          </Link>
        </Stack>
      </div>
    </AgencyShell>
  );
}
