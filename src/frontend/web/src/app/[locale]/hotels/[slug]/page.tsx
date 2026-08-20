import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicFooter, PublicHeader, PublicShell } from "@/components/shell";
import { HotelDetailView } from "@/features/hotel-detail/hotel-detail-view";
import { loadHotelDiscoveryList } from "@/features/hotel-discovery/load-hotel-discovery-list";
import { loadPlaceDetailPage } from "@/features/place-detail/load-place-detail";
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
 * Public Hotel catalog detail (TC-PRODSURF-T004 / TC-P30-T006).
 * Place catalog SoR — not HotelBooking availability engine.
 */
export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const loaded = await loadPlaceDetailPage(localeParam, slug);
  if (!isApiOk(loaded) || loaded.data.kind !== "Hotel") {
    return { title: "TravelCore", robots: { index: false, follow: false } };
  }

  const vm = loaded.data;
  const path = `hotels/${slug}`;
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

export default async function HotelDetailPage({ params }: PageProps) {
  const { locale: localeParam, slug } = await params;
  if (!isAppLocale(localeParam)) {
    notFound();
  }
  const locale: AppLocale = localeParam;

  const loaded = await loadPlaceDetailPage(locale, slug);
  if (!isApiOk(loaded) || loaded.data.kind !== "Hotel") {
    const missing =
      locale === "fa"
        ? {
            title: "هتل پیدا نشد",
            body: "این هتل در فهرست منتشرشده نیست یا موقتاً در دسترس نیست. قیمت یا موجودی ساختگی نشان داده نمی‌شود.",
            back: "بازگشت به فهرست هتل‌ها",
          }
        : locale === "ar"
          ? {
              title: "الفندق غير موجود",
              body: "هذا الفندق غير منشور أو غير متاح مؤقتاً. لا نعرض أسعاراً أو توفراً وهمياً.",
              back: "العودة إلى قائمة الفنادق",
            }
          : {
              title: "Hotel not found",
              body: "This hotel is not in the published list or is temporarily unavailable. We do not invent prices or availability.",
              back: "Back to hotels",
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
              href={`/${locale}/hotels`}
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
  const discovery = await loadHotelDiscoveryList(locale);
  const similarHotels = discovery.hotels
    .filter((h) => h.placeId !== vm.placeId && h.slug !== vm.slug)
    .slice(0, 3);

  const crumbs = [
    ...(vm.destination
      ? [
          {
            name: vm.destination.name,
            publicPath: vm.destination.slug
              ? `destinations/${vm.destination.slug}`
              : null,
          },
        ]
      : []),
    {
      name: vm.name,
      publicPath: `hotels/${vm.slug}`,
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
      <HotelDetailView vm={vm} similarHotels={similarHotels} />
    </PublicShell>
  );
}
