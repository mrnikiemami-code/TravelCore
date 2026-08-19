import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicShell } from "@/components/shell";
import { Text } from "@/components/ui";
import { ContentDetailView } from "@/features/content-detail/content-detail-view";
import { loadContentArticleFixture } from "@/lib/fixtures/content-article";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";

export const metadata: Metadata = {
  title: "Content article validation",
  robots: { index: false, follow: false },
};

/** UIVAL-T008 Content Article validation. */
export default async function ContentArticleValidationPage({
  params,
}: {
  params: Promise<{ locale: string }>;
}) {
  const { locale: localeParam } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }

  const locale: AppLocale = localeParam;
  const loaded = loadContentArticleFixture(locale);
  if (!isApiOk(loaded) || loaded.data.kind !== "Article") {
    notFound();
  }

  return (
    <PublicShell
      header={
        <Text as="p" role="label">
          UIVAL-T008 · Content Article
        </Text>
      }
      footer={
        <Text role="caption">
          Dev validation · <span dir="ltr">{loaded.data.publicPath}</span>
        </Text>
      }
    >
      <ContentDetailView vm={loaded.data} />
    </PublicShell>
  );
}
