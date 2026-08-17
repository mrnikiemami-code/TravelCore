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
    body: "سطح عملیاتی Agency Marketplace: مدیریت پروفایل و Offer. بدون Booking/Payment/Commission.",
    note: "مالکیت با ماژول Agency Marketplace است — نه Tour Admin و نه Identity.",
    profile: "پروفایل تجاری",
    offers: "آگهی‌های فروش",
    back: "بازگشت",
  },
  en: {
    title: "Agency panel",
    body: "Agency Marketplace operational surface: profile and offer management. No Booking/Payment/Commission.",
    note: "Owned by the Agency Marketplace module — not Tour Admin and not Identity.",
    profile: "Commercial profile",
    offers: "Sales offers",
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
            <Text>{copy.profile} · {copy.offers}</Text>
            <Text role="caption">/api/agency-marketplace/profiles · /api/agency-marketplace/offers</Text>
          </Stack>
        </Surface>
      </div>
    </AdminShell>
  );
}
