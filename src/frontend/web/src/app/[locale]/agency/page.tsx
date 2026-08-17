import Link from "next/link";
import { notFound } from "next/navigation";
import { AdminShell } from "@/components/shell";
import { Stack, Surface, Text } from "@/components/ui";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata = {
  robots: { index: false, follow: false },
};

const COPY = {
  fa: {
    title: "پنل آژانس",
    body: "سطح عملیاتی Agency Marketplace: مدیریت پروفایل، Offer و چرخهٔ انتشار. بدون Booking/Payment/Commission و بدون مالکیت SEO.",
    note: "مالکیت با ماژول Agency Marketplace است — نه Tour Admin و نه Identity. Published Offer ≠ SEO Indexed.",
    profile: "پروفایل تجاری",
    offers: "آگهی‌های فروش",
    publish: "انتشار و بازبینی",
    back: "بازگشت",
  },
  en: {
    title: "Agency panel",
    body: "Agency Marketplace operational surface: profile, offer management, and publication workflow. No Booking/Payment/Commission and no SEO ownership.",
    note: "Owned by the Agency Marketplace module — not Tour Admin and not Identity. Published Offer ≠ SEO Indexed.",
    profile: "Commercial profile",
    offers: "Sales offers",
    publish: "Publish and moderate",
    back: "Back",
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
    <AdminShell
      header={
        <Text as="h1" role="heading">
          {copy.title}
        </Text>
      }
      navigation={
        <Link
          className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
          href={`/${locale}/admin/accounts`}
        >
          {copy.back}
        </Link>
      }
    >
      <div className="p-4">
        <Surface>
          <Stack gap="sm">
            <Text role="muted">{copy.body}</Text>
            <Text role="caption">{copy.note}</Text>
            <Text>{copy.profile} · {copy.offers} · {copy.publish}</Text>
            <Text role="caption">/api/agency-marketplace/profiles · /api/agency-marketplace/offers · submit/approve/publish</Text>
          </Stack>
        </Surface>
      </div>
    </AdminShell>
  );
}
