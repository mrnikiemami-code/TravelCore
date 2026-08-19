import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { AdminShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { AgencySurfacesShowcase } from "@/features/agency-surfaces/agency-surfaces-showcase";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Agency surfaces validation",
  robots: { index: false, follow: false },
};

export default async function AgencySurfacesValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  return (
    <AdminShell header={<Text as="p" role="label">UIVAL-T015 · Agency surfaces</Text>}>
      <AgencySurfacesShowcase locale={localeParam as AppLocale} />
    </AdminShell>
  );
}
