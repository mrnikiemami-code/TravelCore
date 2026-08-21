import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { AdminSectionPage } from "@/features/admin-experience/admin-section-page";
import { isAppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Content & media",
  robots: { index: false, follow: false },
};

export default async function AdminContentPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) notFound();
  return <AdminSectionPage locale={localeParam} section="content" />;
}
