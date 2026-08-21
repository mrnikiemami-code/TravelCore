import { notFound } from "next/navigation";
import { AgencySectionPage } from "@/features/agency-experience/agency-section-page";
import { isAppLocale } from "@/lib/i18n";

export default async function Page({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;
  if (!isAppLocale(locale)) notFound();
  return <AgencySectionPage locale={locale} section="profile" />;
}
