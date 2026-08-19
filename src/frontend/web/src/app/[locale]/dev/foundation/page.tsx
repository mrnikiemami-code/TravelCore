import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Container, Text } from "@/components/ui";
import { FoundationPrimitivesShowcase } from "@/features/foundation-validation/foundation-primitives-showcase";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Foundation validation",
  robots: { index: false, follow: false },
};

/**
 * UIVAL-T001 dev-only foundation primitive validation route.
 * Not a product surface — noindex/nofollow.
 */
export default async function FoundationValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T001 · Foundation primitives
        </Text>
      }
      footer={
        <Text role="caption">
          Dev validation only — not indexed · locale{" "}
          <span dir="ltr">{locale}</span>
        </Text>
      }
    >
      <div className="py-8">
        <Container width="content">
          <FoundationPrimitivesShowcase locale={locale} />
        </Container>
      </div>
    </PublicShell>
  );
}
