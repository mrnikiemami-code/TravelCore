import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { AdminShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { AdminSurfacesShowcase } from "@/features/admin-surfaces/admin-surfaces-showcase";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Admin surfaces validation",
  robots: { index: false, follow: false },
};

export default async function AdminSurfacesValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  return (
    <AdminShell header={<Text as="p" role="label">UIVAL-T014 · Admin surfaces</Text>}>
      <AdminSurfacesShowcase locale={localeParam as AppLocale} />
    </AdminShell>
  );
}
