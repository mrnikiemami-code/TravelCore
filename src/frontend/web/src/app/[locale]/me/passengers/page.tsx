import { notFound } from "next/navigation";
import { CustomerSectionPage } from "@/features/customer-experience/customer-section-page";
import { isAppLocale } from "@/lib/i18n";

export default async function Page({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;
  if (!isAppLocale(locale)) notFound();
  return <CustomerSectionPage locale={locale} section="passengers" />;
}
