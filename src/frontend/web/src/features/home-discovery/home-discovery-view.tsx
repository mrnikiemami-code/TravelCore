import Link from "next/link";
import type { ReactNode } from "react";
import { Container, Stack, Surface, Text } from "@/components/ui";
import { HotelCard } from "@/features/hotel-discovery/hotel-card";
import type { HomeDiscoveryComposition } from "@/features/home-discovery/types";
import { TourCard } from "@/features/tour-discovery/tour-card";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";

export type HomeDiscoveryViewProps = {
  locale: AppLocale;
  composition?: HomeDiscoveryComposition;
  /** UIVAL dev route may include `/dev/*` links; production home must not. */
  includeDevLinks?: boolean;
};

function copyFor(locale: AppLocale) {
  if (locale === "fa") {
    return {
      brandLine: "کشف · اعتماد · رزرو",
      heroEyebrow: "بازار گردشگری حرفه‌ای",
      heroTitle: "سفر را حرفه‌ای شروع کنید",
      heroSubtitle:
        "مقصد، هتل و تور در یک تجربهٔ تجاری شفاف — کاتالوگ واقعی، بدون قیمت یا موجودی جعلی.",
      searchDest: "مقصد",
      searchDestPh: "مثلاً استانبول یا تهران",
      searchIntent: "نوع تجربه",
      searchHotels: "هتل",
      searchTours: "تور",
      searchCta: "کاوش سفر",
      heroHotels: "مشاهده هتل‌ها",
      heroTours: "مشاهده تورها",
      destinationsTitle: "مقاصد الهام‌بخش",
      destinationsBlurb: "مقاصد واقعی از کاتالوگ عمومی — تصویرمحور و آماده کشف.",
      toursTitle: "تورهای منتخب",
      toursBlurb: "پکیج‌های منتشرشده — قیمت فقط وقتی دادهٔ Pricing واقعی باشد.",
      toursCta: "همه تورها",
      hotelsTitle: "هتل‌های شاخص",
      hotelsBlurb: "کاتالوگ Place فعال؛ بدون امتیاز یا نرخ ساختگی.",
      seeAllHotels: "همه هتل‌ها",
      noHotelsTitle: "هتل‌ها به‌زودی در پیش‌نمایش عمومی",
      noHotelsBody:
        "ساختار کارت آماده است. تا دادهٔ عمومی واقعی، قیمت یا امتیاز جعلی نشان نمی‌دهیم.",
      noToursTitle: "تورهای منتشرشده به‌زودی",
      noToursBody:
        "وقتی تور Published با مقصد مشخص باشد، اینجا کارت محصول نمایش داده می‌شود.",
      trustTitle: "چرا TravelCore قابل اعتماد است",
      storiesTitle: "سفرنامه‌ها",
      seeAllStories: "همه سفرنامه‌ها",
      noStoriesTitle: "الهام سفر به‌زودی",
      noStoriesBody:
        "وقتی سفرنامهٔ منتشرشده باشد، اینجا کارت‌های الهام نمایش داده می‌شود.",
      ctaTitle: "می‌خواهید این پلتفرم را برای آژانس ببینید؟",
      ctaBody:
        "از کشف مقصد تا هتل و تور — مسیر تجربهٔ تجاری آماده دمو است.",
      ctaButton: "شروع برنامه‌ریزی",
      openDestination: "ورود به مقصد",
      openCatalog: "ورود به کاتالوگ",
      sampleNote:
        "جستجو به مسیرهای موجود هدایت می‌شود — موتور رزرو یا موجودی جعلی نیست.",
      demoHint: "نمونه کاتالوگ",
      heroPhotoAlt: "تصویر سفر",
    };
  }
  if (locale === "ar") {
    return {
      brandLine: "اكتشف · ثق · احجز",
      heroEyebrow: "سوق سفر احترافي",
      heroTitle: "ابدأ رحلتك باحتراف",
      heroSubtitle:
        "وجهات وفنادق وجولات في تجربة تجارية شفافة — كتالوج حقيقي دون أسعار أو توفر وهمي.",
      searchDest: "الوجهة",
      searchDestPh: "مثل إسطنبول أو طهران",
      searchIntent: "نوع التجربة",
      searchHotels: "فندق",
      searchTours: "جولة",
      searchCta: "استكشف السفر",
      heroHotels: "عرض الفنادق",
      heroTours: "عرض الجولات",
      destinationsTitle: "وجهات ملهمة",
      destinationsBlurb: "وجهات حقيقية من الكتالوج العام — بطاقات تعتمد على الصور.",
      toursTitle: "جولات مختارة",
      toursBlurb: "باقات منشورة — السعر فقط عند توفر بيانات التسعير الحقيقية.",
      toursCta: "كل الجولات",
      hotelsTitle: "فنادق مميزة",
      hotelsBlurb: "كتالوج Place نشط؛ دون تقييمات أو أسعار مصطنعة.",
      seeAllHotels: "كل الفنادق",
      noHotelsTitle: "الفنادق قريباً في المعاينة العامة",
      noHotelsBody: "هيكل البطاقة جاهز. لا أسعار أو تقييمات وهمية.",
      noToursTitle: "الجولات المنشورة قريباً",
      noToursBody: "عند توفر جولات منشورة مرتبطة بوجهة ستظهر هنا.",
      trustTitle: "لماذا يمكن الوثوق بـ TravelCore",
      storiesTitle: "قصص السفر",
      seeAllStories: "كل القصص",
      noStoriesTitle: "إلهام السفر قريباً",
      noStoriesBody: "عند توفر القصص المنشورة ستظهر هنا بطاقات الإلهام.",
      ctaTitle: "هل تريد عرض المنصة لوكالة سفر؟",
      ctaBody: "من الوجهة إلى الفندق والجولة — مسار تجربة تجارية جاهز للعرض.",
      ctaButton: "ابدأ التخطيط",
      openDestination: "افتح الوجهة",
      openCatalog: "افتح الكتالوج",
      sampleNote: "البحث يوجّه إلى مسارات موجودة — ليس محرك حجز وهمي.",
      demoHint: "عينة الكتالوج",
      heroPhotoAlt: "صورة سفر",
    };
  }
  return {
    brandLine: "Discover · Trust · Book",
    heroEyebrow: "Professional travel marketplace",
    heroTitle: "Start travel the professional way",
    heroSubtitle:
      "Destinations, hotels, and tours in one transparent commerce experience — real catalog, no fake prices or availability.",
    searchDest: "Destination",
    searchDestPh: "e.g. Istanbul or Tehran",
    searchIntent: "Experience",
    searchHotels: "Hotels",
    searchTours: "Tours",
    searchCta: "Explore trips",
    heroHotels: "Browse hotels",
    heroTours: "Browse tours",
    destinationsTitle: "Inspiring destinations",
    destinationsBlurb: "Live public destinations — image-led discovery cards.",
    toursTitle: "Featured tours",
    toursBlurb:
      "Published tour packages — price only when Pricing owns real data.",
    toursCta: "All tours",
    hotelsTitle: "Featured hotels",
    hotelsBlurb: "Active Place catalog — no invented rates or review scores.",
    seeAllHotels: "All hotels",
    noHotelsTitle: "Hotels coming to public preview",
    noHotelsBody:
      "Card structure is ready. No invented prices or ratings until public data exists.",
    noToursTitle: "Published tours coming soon",
    noToursBody:
      "When Published tours are linked to a destination, product cards appear here.",
    trustTitle: "Why travel businesses can trust TravelCore",
    storiesTitle: "Travel stories",
    seeAllStories: "All stories",
    noStoriesTitle: "Inspiration coming soon",
    noStoriesBody: "Published travelogues will appear here as inspiration cards.",
    ctaTitle: "Ready to show this platform to an agency?",
    ctaBody:
      "From destination discovery to hotels and tours — a commercial demo path.",
    ctaButton: "Start planning",
    openDestination: "Open destination",
    openCatalog: "Open catalog",
    sampleNote:
      "Search routes to existing surfaces — not a fake booking or inventory engine.",
    demoHint: "Sample catalog",
    heroPhotoAlt: "Travel photography",
  };
}

function trustItems(locale: AppLocale) {
  if (locale === "fa") {
    return [
      {
        title: "کاتالوگ واقعی",
        body: "مقصد، هتل و تور از مالک دامنه — نه موجودی ساختگی.",
      },
      {
        title: "مرز تجاری شفاف",
        body: "قیمت و موجودی فقط وقتی Pricing/Booking فعال باشد.",
      },
      {
        title: "موبایل و چندزبانه",
        body: "تجربهٔ عمومی برای FA / EN / AR با چیدمان RTL-native.",
      },
      {
        title: "تأیید پرداخت صادقانه",
        body: "تأیید رزرو فقط پس از شواهد پرداخت معتبر — بدون میان‌بر جعلی.",
      },
    ];
  }
  if (locale === "ar") {
    return [
      {
        title: "كتالوج حقيقي",
        body: "وجهات وفنادق وجولات من مالك المجال — دون مخزون وهمي.",
      },
      {
        title: "حدود تجارية واضحة",
        body: "السعر والتوفر فقط عند تفعيل التسعير/الحجز.",
      },
      {
        title: "متعدد اللغات والجوال",
        body: "تجربة عامة لـ FA / EN / AR مع تخطيط RTL أصلي.",
      },
      {
        title: "تأكيد دفع صادق",
        body: "تأكيد الحجز فقط بعد أدلة دفع صالحة — دون اختصارات وهمية.",
      },
    ];
  }
  return [
    {
      title: "Real catalog",
      body: "Destinations, hotels, and tours from domain owners — not invented stock.",
    },
    {
      title: "Honest commerce boundaries",
      body: "Price and availability only when Pricing/Booking are live.",
    },
    {
      title: "Mobile & multilingual",
      body: "Public experience for FA / EN / AR with RTL-native layout.",
    },
    {
      title: "Honest confirmation path",
      body: "Booking confirms only after valid payment evidence — no fake shortcuts.",
    },
  ];
}

function fallbackDestinationCards(locale: AppLocale) {
  return [
    {
      href: `/${locale}/tours`,
      title:
        locale === "fa"
          ? "تورهای مقصد"
          : locale === "ar"
            ? "جولات الوجهات"
            : "Destination tours",
      blurb:
        locale === "fa"
          ? "ورود به کاتالوگ تور"
          : locale === "ar"
            ? "ادخل كتالوج الجولات"
            : "Enter the tour catalog",
    },
    {
      href: `/${locale}/hotels`,
      title:
        locale === "fa" ? "اقامتگاه‌ها" : locale === "ar" ? "الإقامة" : "Stays",
      blurb:
        locale === "fa"
          ? "کشف هتل‌های عمومی"
          : locale === "ar"
            ? "اكتشف الفنادق العامة"
            : "Browse public hotels",
    },
    {
      href: `/${locale}/plan`,
      title:
        locale === "fa"
          ? "برنامه سفر"
          : locale === "ar"
            ? "خطة الرحلة"
            : "Trip plan",
      blurb:
        locale === "fa"
          ? "شروع طراحی برنامه"
          : locale === "ar"
            ? "ابدأ التخطيط"
            : "Start planning",
    },
    {
      href: `/${locale}/travelogues`,
      title:
        locale === "fa"
          ? "الهام سفر"
          : locale === "ar"
            ? "إلهام السفر"
            : "Travel inspiration",
      blurb:
        locale === "fa"
          ? "سفرنامه‌ها و داستان‌ها"
          : locale === "ar"
            ? "قصص السفر"
            : "Stories and travelogues",
    },
  ];
}

function SectionHeading({
  title,
  blurb,
  action,
  id,
}: {
  title: string;
  blurb?: string;
  action?: ReactNode;
  id: string;
}) {
  return (
    <div className="mb-6 flex flex-wrap items-end justify-between gap-3">
      <div className="space-y-1.5">
        <h2
          id={id}
          className="text-2xl font-semibold tracking-tight text-foreground sm:text-[1.65rem]"
        >
          {title}
        </h2>
        {blurb ? (
          <p className="max-w-2xl text-sm leading-relaxed text-muted-foreground">
            {blurb}
          </p>
        ) : null}
      </div>
      {action}
    </div>
  );
}

function pickHeroImage(composition?: HomeDiscoveryComposition): string | null {
  if (!composition) return null;
  const fromDestination = composition.destinations.find((d) => d.coverSrc)?.coverSrc;
  if (fromDestination) return fromDestination;
  const fromHotel = composition.hotels.find((h) => h.coverSrc)?.coverSrc;
  if (fromHotel) return fromHotel;
  const fromTour = composition.tours.find((t) => t.coverSrc)?.coverSrc;
  if (fromTour) return fromTour;
  return null;
}

/**
 * Public Home / Discovery — TC-P36-T002 commercial redesign.
 * Server Component. Honesty: no invented prices/availability/ratings.
 */
export function HomeDiscoveryView({
  locale,
  composition,
  includeDevLinks = false,
}: HomeDiscoveryViewProps) {
  const copy = copyFor(locale);
  const trust = trustItems(locale);
  const liveDestinations = composition?.destinations ?? [];
  const liveTours = composition?.tours ?? [];
  const hotels = composition?.hotels ?? [];
  const travelogues = composition?.travelogues ?? [];
  const fallbackDestinations = fallbackDestinationCards(locale);
  const toursAction = `/${locale}/tours`;
  const heroImage = pickHeroImage(composition);

  return (
    <div className="pb-16">
      <section
        aria-labelledby="home-hero-title"
        className="relative isolate overflow-hidden border-b border-border"
      >
        <div className="absolute inset-0 bg-[#0E172A]" aria-hidden />
        {heroImage ? (
          // eslint-disable-next-line @next/next/no-img-element -- owned demo media via app proxy
          <img
            src={heroImage}
            alt=""
            className="absolute inset-0 h-full w-full object-cover"
          />
        ) : (
          <div
            aria-hidden
            className="absolute inset-0 bg-[radial-gradient(ellipse_at_20%_20%,#1D4ED8_0%,transparent_55%),radial-gradient(ellipse_at_80%_10%,#F59E0B_0%,transparent_40%),linear-gradient(135deg,#0E172A_0%,#1D4ED8_55%,#0E172A_100%)]"
          />
        )}
        <div
          aria-hidden
          className="absolute inset-0 bg-gradient-to-r from-[#0E172A]/92 via-[#0E172A]/72 to-[#0E172A]/45"
        />
        <div
          aria-hidden
          className="absolute inset-x-0 bottom-0 h-28 bg-gradient-to-t from-background to-transparent"
        />

        <Container width="wide" className="relative py-12 sm:py-16 lg:py-20">
          <div className="grid gap-8 lg:grid-cols-[1.15fr_0.85fr] lg:items-end">
            <div className="max-w-2xl space-y-5 text-white">
              <p className="text-xs font-semibold uppercase tracking-[0.18em] text-[#FBBF24]">
                {copy.heroEyebrow}
                <span className="mx-2 text-white/40">·</span>
                <span className="font-medium normal-case tracking-normal text-white/80">
                  {copy.brandLine}
                </span>
              </p>
              <h1
                id="home-hero-title"
                className="text-3xl font-semibold tracking-tight sm:text-4xl lg:text-5xl"
              >
                {copy.heroTitle}
              </h1>
              <p className="max-w-xl text-base leading-relaxed text-white/90 sm:text-lg">
                {copy.heroSubtitle}
              </p>
              <div className="flex flex-wrap gap-3 pt-1">
                <Link
                  href={`/${locale}/hotels`}
                  className="min-h-touch inline-flex items-center rounded-lg bg-[#F59E0B] px-5 text-sm font-semibold text-[#0E172A] shadow-sm hover:brightness-105"
                >
                  {copy.heroHotels}
                </Link>
                <Link
                  href={`/${locale}/tours`}
                  className="min-h-touch inline-flex items-center rounded-lg border border-white/35 bg-white/10 px-5 text-sm font-medium text-white backdrop-blur-sm hover:bg-white/20"
                >
                  {copy.heroTours}
                </Link>
              </div>
            </div>

            <form
              action={toursAction}
              method="get"
              className="rounded-2xl border border-white/20 bg-white/95 p-4 text-foreground shadow-2xl backdrop-blur-md sm:p-5"
            >
              <p className="mb-3 text-sm font-semibold text-[#0E172A]">
                {copy.searchCta}
              </p>
              <div className="grid gap-3">
                <label className="block space-y-1.5 text-xs font-medium text-muted-foreground">
                  <span>{copy.searchDest}</span>
                  <input
                    name="destination"
                    placeholder={copy.searchDestPh}
                    className="min-h-touch w-full rounded-lg border border-border bg-surface px-3 text-sm text-foreground outline-none ring-[#1D4ED8] focus:ring-2"
                  />
                </label>
                <fieldset className="space-y-1.5">
                  <legend className="text-xs font-medium text-muted-foreground">
                    {copy.searchIntent}
                  </legend>
                  <div className="grid grid-cols-2 gap-2">
                    <Link
                      href={`/${locale}/hotels`}
                      className="min-h-touch inline-flex items-center justify-center rounded-lg border border-border bg-surface px-3 text-sm font-medium text-foreground hover:border-[#1D4ED8]/40 hover:text-[#1D4ED8]"
                    >
                      {copy.searchHotels}
                    </Link>
                    <button
                      type="submit"
                      className="min-h-touch inline-flex items-center justify-center rounded-lg border border-[#1D4ED8]/30 bg-[#1D4ED8]/8 px-3 text-sm font-medium text-[#1D4ED8] hover:bg-[#1D4ED8]/15"
                    >
                      {copy.searchTours}
                    </button>
                  </div>
                </fieldset>
              </div>
              <button
                type="submit"
                className="mt-4 min-h-touch inline-flex w-full items-center justify-center rounded-lg bg-[#1D4ED8] px-4 text-sm font-semibold text-white hover:bg-[#1E40AF]"
              >
                {copy.searchCta}
              </button>
              <p className="mt-2 text-[11px] leading-relaxed text-muted-foreground">
                {copy.sampleNote}
              </p>
            </form>
          </div>
        </Container>
      </section>

      <Container width="wide" className="space-y-16 pt-12 sm:pt-16">
        <section aria-labelledby="home-destinations-title">
          <SectionHeading
            id="home-destinations-title"
            title={copy.destinationsTitle}
            blurb={copy.destinationsBlurb}
          />
          {liveDestinations.length > 0 ? (
            <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
              {liveDestinations.map((item) => (
                <li key={item.destinationId}>
                  <Link
                    href={`/${locale}/destinations/${encodeURIComponent(item.slug)}`}
                    className="group flex h-full flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-sm transition hover:-translate-y-0.5 hover:border-[#1D4ED8]/35 hover:shadow-lg"
                  >
                    <div className="relative aspect-[4/3] overflow-hidden bg-[#0E172A]/90">
                      {item.coverSrc ? (
                        // eslint-disable-next-line @next/next/no-img-element
                        <img
                          src={item.coverSrc}
                          alt=""
                          className="absolute inset-0 h-full w-full object-cover transition duration-500 group-hover:scale-[1.04]"
                        />
                      ) : (
                        <div
                          aria-hidden
                          className="absolute inset-0 bg-[linear-gradient(145deg,#1D4ED8_0%,#0E172A_55%,#F59E0B_140%)]"
                        />
                      )}
                      <div
                        aria-hidden
                        className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/15 to-transparent"
                      />
                      {item.slug.startsWith("demofeed-") ? (
                        <span className="absolute end-2 top-2 rounded-full bg-black/45 px-2 py-0.5 text-[10px] font-medium tracking-wide text-white/90 backdrop-blur-sm">
                          {copy.demoHint}
                        </span>
                      ) : null}
                      <div className="absolute inset-x-0 bottom-0 p-4">
                        <span className="block text-base font-semibold text-white">
                          {item.name}
                        </span>
                        <span className="mt-0.5 block text-xs text-white/80">
                          {copy.openDestination}
                        </span>
                      </div>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          ) : (
            <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
              {fallbackDestinations.map((item) => (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    className="group flex h-full flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-sm transition hover:border-[#1D4ED8]/35 hover:shadow-md"
                  >
                    <div className="relative aspect-[4/3] overflow-hidden bg-[linear-gradient(145deg,#1D4ED8_0%,#0E172A_60%,#F59E0B_130%)]">
                      <div
                        aria-hidden
                        className="absolute inset-0 bg-gradient-to-t from-black/65 via-transparent to-transparent"
                      />
                      <div className="absolute inset-x-0 bottom-0 p-4">
                        <span className="block text-base font-semibold text-white">
                          {item.title}
                        </span>
                        <span className="mt-0.5 block text-xs text-white/80">
                          {item.blurb}
                        </span>
                      </div>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section aria-labelledby="home-tours-title">
          <SectionHeading
            id="home-tours-title"
            title={copy.toursTitle}
            blurb={copy.toursBlurb}
            action={
              <Link
                href={`/${locale}/tours`}
                className="min-h-touch inline-flex items-center text-sm font-medium text-[#1D4ED8] underline-offset-2 hover:underline"
              >
                {copy.toursCta}
              </Link>
            }
          />
          {liveTours.length === 0 ? (
            <Surface className="overflow-hidden rounded-2xl p-0">
              <div className="grid gap-0 md:grid-cols-[0.9fr_1.3fr]">
                <div className="min-h-40 bg-[linear-gradient(145deg,#1D4ED8,#0E172A_55%,#F59E0B)]" />
                <div className="space-y-3 p-6">
                  <p className="text-sm font-semibold text-foreground">
                    {copy.noToursTitle}
                  </p>
                  <Text role="muted">{copy.noToursBody}</Text>
                  <Link
                    href={`/${locale}/tours`}
                    className="inline-flex text-sm font-medium text-[#1D4ED8] underline-offset-2 hover:underline"
                  >
                    {copy.openCatalog} →
                  </Link>
                </div>
              </div>
            </Surface>
          ) : (
            <ul className="grid grid-cols-1 gap-5 md:grid-cols-2 xl:grid-cols-3">
              {liveTours.map((item) => (
                <li key={item.tourProductId}>
                  <TourCard locale={locale} tour={item} />
                </li>
              ))}
            </ul>
          )}
        </section>

        <section aria-labelledby="home-hotels-title">
          <SectionHeading
            id="home-hotels-title"
            title={copy.hotelsTitle}
            blurb={copy.hotelsBlurb}
            action={
              <Link
                href={`/${locale}/hotels`}
                className="min-h-touch inline-flex items-center text-sm font-medium text-[#1D4ED8] underline-offset-2 hover:underline"
              >
                {copy.seeAllHotels}
              </Link>
            }
          />
          {hotels.length === 0 ? (
            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              {[0, 1, 2].map((i) => (
                <Surface key={i} className="overflow-hidden rounded-2xl p-0">
                  <div className="aspect-[4/3] bg-gradient-to-br from-surface-muted to-[#1D4ED8]/20" />
                  <div className="space-y-2 p-4">
                    {i === 0 ? (
                      <>
                        <p className="text-sm font-semibold text-foreground">
                          {copy.noHotelsTitle}
                        </p>
                        <p className="text-sm text-muted-foreground">
                          {copy.noHotelsBody}
                        </p>
                        <Link
                          href={`/${locale}/hotels`}
                          className="inline-flex text-sm font-medium text-[#1D4ED8] underline-offset-2 hover:underline"
                        >
                          {copy.seeAllHotels}
                        </Link>
                      </>
                    ) : (
                      <div className="space-y-2" aria-hidden>
                        <div className="h-3 w-2/3 rounded bg-surface-muted" />
                        <div className="h-3 w-full rounded bg-surface-muted" />
                        <div className="h-3 w-1/2 rounded bg-surface-muted" />
                      </div>
                    )}
                  </div>
                </Surface>
              ))}
            </div>
          ) : (
            <ul className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
              {hotels.map((item) => (
                <li key={item.placeId}>
                  <HotelCard locale={locale} hotel={item} />
                </li>
              ))}
            </ul>
          )}
        </section>

        <section aria-labelledby="home-trust-title">
          <SectionHeading id="home-trust-title" title={copy.trustTitle} />
          <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {trust.map((item) => (
              <li key={item.title}>
                <Surface className="h-full rounded-2xl border-[#1D4ED8]/10 bg-gradient-to-br from-surface to-[#1D4ED8]/[0.04] p-5">
                  <p className="text-sm font-semibold text-[#1D4ED8]">
                    {item.title}
                  </p>
                  <p className="mt-2 text-sm leading-relaxed text-muted-foreground">
                    {item.body}
                  </p>
                </Surface>
              </li>
            ))}
          </ul>
        </section>

        <section aria-labelledby="home-stories-title">
          <SectionHeading
            id="home-stories-title"
            title={copy.storiesTitle}
            action={
              <Link
                href={`/${locale}/travelogues`}
                className="min-h-touch inline-flex items-center text-sm font-medium text-[#1D4ED8] underline-offset-2 hover:underline"
              >
                {copy.seeAllStories}
              </Link>
            }
          />
          {travelogues.length === 0 ? (
            <Surface className="overflow-hidden rounded-2xl p-0">
              <div className="grid gap-0 md:grid-cols-[0.9fr_1.2fr]">
                <div className="min-h-36 bg-[linear-gradient(145deg,#1D4ED8_10%,#F59E0B_90%)] opacity-90" />
                <div className="space-y-2 p-6">
                  <p className="text-sm font-semibold text-foreground">
                    {copy.noStoriesTitle}
                  </p>
                  <Text role="muted">{copy.noStoriesBody}</Text>
                </div>
              </div>
            </Surface>
          ) : (
            <ul className="grid grid-cols-1 gap-4 md:grid-cols-3">
              {travelogues.map((item) => (
                <li key={item.travelogueId}>
                  <Link
                    href={`/${locale}/travelogues/${encodeURIComponent(item.travelogueId)}`}
                    className="group flex h-full flex-col overflow-hidden rounded-2xl border border-border bg-surface shadow-sm transition hover:border-[#1D4ED8]/35 hover:shadow-md"
                  >
                    <div className="aspect-[16/9] bg-[linear-gradient(145deg,#1D4ED8,#F59E0B)]" />
                    <div className="flex flex-1 flex-col gap-2 p-4">
                      <span className="text-base font-semibold text-foreground group-hover:text-[#1D4ED8]">
                        {item.title}
                      </span>
                      <span className="line-clamp-3 text-sm text-muted-foreground">
                        {item.body.slice(0, 160)}
                      </span>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section aria-labelledby="home-cta-title">
          <div className="overflow-hidden rounded-2xl bg-[#0E172A] px-6 py-10 text-white shadow-md sm:px-10">
            <div className="grid gap-6 lg:grid-cols-[1.4fr_0.6fr] lg:items-center">
              <Stack gap="md">
                <h2
                  id="home-cta-title"
                  className="text-2xl font-semibold tracking-tight sm:text-3xl"
                >
                  {copy.ctaTitle}
                </h2>
                <p className="max-w-2xl text-sm text-white/85 sm:text-base">
                  {copy.ctaBody}
                </p>
                <div className="flex flex-wrap gap-3">
                  <Link
                    href={`/${locale}/plan`}
                    className="min-h-touch inline-flex items-center rounded-lg bg-[#F59E0B] px-5 text-sm font-semibold text-[#0E172A] hover:brightness-105"
                  >
                    {copy.ctaButton}
                  </Link>
                  <Link
                    href={`/${locale}/hotels`}
                    className="min-h-touch inline-flex items-center rounded-lg border border-white/35 px-5 text-sm font-medium text-white hover:bg-white/10"
                  >
                    {copy.heroHotels}
                  </Link>
                </div>
              </Stack>
              <div
                aria-hidden
                className={cn(
                  "hidden min-h-36 rounded-xl lg:block",
                  "bg-[radial-gradient(circle_at_30%_30%,#F59E0B_0%,transparent_45%),linear-gradient(145deg,#1D4ED8,#0E172A)]",
                )}
              />
            </div>
          </div>
        </section>

        {includeDevLinks ? (
          <Surface tone="muted">
            <Text role="muted">Dev links enabled on this surface.</Text>
          </Surface>
        ) : null}
      </Container>
    </div>
  );
}
