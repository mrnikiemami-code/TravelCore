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
    title: "پنل آژانس (پایه)",
    body: "سطح قابلیت دسترسی‌محور؛ بدون منطق تور/قیمت/رزرو/پرداخت. تصمیم مجوز سمت سرور است.",
    note: "این صفحه فقط Presentation stub است — مالک دامنه تجارت نیست.",
    back: "بازگشت",
  },
  en: {
    title: "Agency panel (baseline)",
    body: "Access-gated capability surface; no Tour/Pricing/Booking/Payment logic. Authorization stays server-side.",
    note: "Presentation stub only — does not own commerce domains.",
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
          </Stack>
        </Surface>
      </div>
    </AdminShell>
  );
}
