import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicFooter, PublicHeader, PublicShell } from "@/components/shell";
import { loadTourDetailPage } from "@/features/tour-detail/load-tour-detail";
import { TourDetailView } from "@/features/tour-detail/tour-detail-view";
import { isApiOk } from "@/lib/api/result";
import { isAppLocale, type AppLocale } from "@/lib/i18n";
import { loadSeoBreadcrumbJsonLd } from "@/lib/seo/load-breadcrumb-jsonld";
import { loadComposedSeoMetadata } from "@/lib/seo/load-composed-metadata";
import {
  languagesFromComposed,
  robotsFromComposed,
} from "@/lib/seo/metadata-contract";
import { serializeBreadcrumbJsonLd } from "@/lib/seo/structured-data-contract";

type PageProps = {
  params: Promise<{ locale: string; slug: string }>;
};

/**
 * Public Tour commerce detail (TC-P30-T007 · TC-P31-T005 polish).
 * Catalog SoR · Pricing display-only · booking CTA → existing prepare entry.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const loaded = await loadTourDetailPage(localeParam, slug);
  if (!isApiOk(loaded)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const vm = loaded.data;
  const path = `tours/${slug}`;
  const composed = await loadComposedSeoMetadata({
    locale: localeParam,
    path,
    localizedTitle: vm.name,
    localizedDescription: vm.description,
  });

  if (!composed) {
    return {
      title: vm.name,
      description: vm.description ?? undefined,
      robots: { index: false, follow: true },
    };
  }

  const languages = languagesFromComposed(composed);
  const robots = robotsFromComposed(composed);

  return {
    title: composed.title,
    description: composed.description ?? undefined,
    ...(composed.canonicalHref || Object.keys(languages).length > 0
      ? {
          alternates: {
            ...(composed.canonicalHref
              ? { canonical: composed.canonicalHref }
              : {}),
            ...(Object.keys(languages).length > 0 ? { languages } : {}),
          },
        }
      : {}),
    robots,
  };
}

export default async function TourDetailPage({ params }: PageProps) {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const loaded = await loadTourDetailPage(locale, slug);
  if (!isApiOk(loaded)) {
    const missing =
      locale === "fa"
        ? {
            title: "تور پیدا نشد",
            body: "این تور در فهرست منتشرشده نیست یا موقتاً در دسترس نیست. قیمت یا موجودی ساختگی نشان داده نمی‌شود.",
            back: "بازگشت به فهرست تورها",
          }
        : locale === "ar"
          ? {
              title: "الجولة غير موجودة",
              body: "هذه الجولة غير منشورة أو غير متاحة مؤقتاً. لا نعرض أسعاراً أو توفراً وهمياً.",
              back: "العودة إلى قائمة الجولات",
            }
          : {
              title: "Tour not found",
              body: "This tour is not in the published list or is temporarily unavailable. We do not invent prices or availability.",
              back: "Back to tours",
            };

    return (
      <PublicShell
        header={<PublicHeader locale={locale} />}
        footer={<PublicFooter locale={locale} />}
      >
        <div className="py-10 sm:py-14">
          <div className="mx-auto max-w-xl rounded-xl border border-border bg-surface p-6 shadow-sm sm:p-8">
            <h1 className="text-2xl font-semibold tracking-tight text-foreground">
              {missing.title}
            </h1>
            <p className="mt-2 text-sm text-muted-foreground">{missing.body}</p>
            <a
              href={`/${locale}/tours`}
              className="mt-6 inline-flex min-h-touch items-center justify-center rounded-md bg-primary px-4 text-sm font-medium text-primary-foreground hover:opacity-95"
            >
              {missing.back}
            </a>
          </div>
        </div>
      </PublicShell>
    );
  }

  const vm = loaded.data;
  const crumbs = [
    {
      name: vm.name,
      publicPath: `tours/${vm.slug}`,
    },
  ];
  const breadcrumbJsonLd = await loadSeoBreadcrumbJsonLd(locale, crumbs);
  const breadcrumbScript = serializeBreadcrumbJsonLd(breadcrumbJsonLd);

  return (
    <PublicShell
      header={<PublicHeader locale={locale} />}
      footer={<PublicFooter locale={locale} />}
    >
      {breadcrumbScript ? (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{ __html: breadcrumbScript }}
        />
      ) : null}
      <TourDetailView vm={vm} />
    </PublicShell>
  );
}
