import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { AdminSectionPage } from "@/features/admin-experience/admin-section-page";
import { isAppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Audit & workflow",
  robots: { index: false, follow: false },
};

export default async function AdminAuditPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) notFound();
  return <AdminSectionPage locale={localeParam} section="audit" />;
}
