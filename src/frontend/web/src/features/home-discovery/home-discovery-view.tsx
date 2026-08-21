import Link from "next/link";
import type { ReactNode } from "react";
import { Container, LtrValue, Stack, Surface, Text } from "@/components/ui";
import type { HomeDiscoveryComposition } from "@/features/home-discovery/types";
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
      brandLine: "کشف · اعتماد · اقدام",
      heroEyebrow: "Travel marketplace",
      heroTitle: "سفر را حرفه‌ای شروع کنید",
      heroSubtitle:
        "مقصد، هتل و تور در یک تجربهٔ تجاری شفاف — کاتالوگ واقعی، بدون قیمت یا موجودی جعلی.",
      searchDest: "مقصد",
      searchDestPh: "مثلاً استانبول",
      searchDate: "تاریخ",
      searchGuests: "مسافر",
      searchGuestsPh: "۲ بزرگسال",
      searchCta: "جستجوی سفر",
      heroHotels: "مشاهده هتل‌ها",
      heroTours: "مشاهده تورها",
      destinationsTitle: "مقاصد الهام‌بخش",
      destinationsBlurb:
        "مقاصد واقعی از کاتالوگ — وقتی داده عمومی نباشد، مسیر کشف صادقانه می‌ماند.",
      toursTitle: "تورهای منتخب",
      toursBlurb:
        "محصولات تور منتشرشده از کاتالوگ — قیمت فقط وقتی مالک Pricing داده واقعی بدهد.",
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
      noDestinationsTitle: "مقاصد کاتالوگ به‌زودی",
      trustTitle: "اعتماد در مسیر خرید سفر",
      storiesTitle: "سفرنامه‌ها",
      seeAllStories: "همه سفرنامه‌ها",
      noStoriesTitle: "الهام سفر به‌زودی",
      noStoriesBody:
        "وقتی سفرنامهٔ منتشرشده باشد، اینجا کارت‌های الهام نمایش داده می‌شود.",
      ctaTitle: "می‌خواهید این پلتفرم را برای آژانس ببینید؟",
      ctaBody:
        "از کشف مقصد تا هتل و تور — مسیر تجربهٔ تجاری آماده دمو است.",
      ctaButton: "شروع برنامه‌ریزی",
      stars: "ستاره",
      openTour: "مشاهده تور",
      openHotel: "مشاهده هتل",
      openDestination: "ورود به مقصد",
      openCatalog: "ورود به کاتالوگ",
      sampleNote: "جستجو به مسیرهای موجود هدایت می‌شود — موتور رزرو جعلی نیست.",
      packageLabel: "پکیج",
      demoHint: "نمونه DEMOFEED",
    };
  }
  if (locale === "ar") {
    return {
      brandLine: "اكتشف · ثق · ابدأ",
      heroEyebrow: "Travel marketplace",
      heroTitle: "ابدأ رحلتك باحتراف",
      heroSubtitle:
        "وجهات وفنادق وجولات في تجربة تجارية شفافة — كتالوج حقيقي دون أسعار أو توفر وهمي.",
      searchDest: "الوجهة",
      searchDestPh: "مثل إسطنبول",
      searchDate: "التاريخ",
      searchGuests: "المسافرون",
      searchGuestsPh: "بالغان",
      searchCta: "ابحث عن رحلة",
      heroHotels: "عرض الفنادق",
      heroTours: "عرض الجولات",
      destinationsTitle: "وجهات ملهمة",
      destinationsBlurb:
        "وجهات حقيقية من الكتالوج — ومسارات اكتشاف صادقة عند غياب البيانات.",
      toursTitle: "جولات مختارة",
      toursBlurb: "منتجات جولات منشورة — السعر فقط عند توفر بيانات التسعير الحقيقية.",
      toursCta: "كل الجولات",
      hotelsTitle: "فنادق مميزة",
      hotelsBlurb: "كتالوج Place نشط؛ دون تقييمات أو أسعار مصطنعة.",
      seeAllHotels: "كل الفنادق",
      noHotelsTitle: "الفنادق قريباً في المعاينة العامة",
      noHotelsBody: "هيكل البطاقة جاهز. لا أسعار أو تقييمات وهمية.",
      noToursTitle: "الجولات المنشورة قريباً",
      noToursBody: "عند توفر جولات منشورة مرتبطة بوجهة ستظهر هنا.",
      noDestinationsTitle: "وجهات الكتالوج قريباً",
      trustTitle: "الثقة في مسار شراء السفر",
      storiesTitle: "قصص السفر",
      seeAllStories: "كل القصص",
      noStoriesTitle: "إلهام السفر قريباً",
      noStoriesBody: "عند توفر القصص المنشورة ستظهر هنا بطاقات الإلهام.",
      ctaTitle: "هل تريد عرض المنصة لوكالة سفر؟",
      ctaBody: "من الوجهة إلى الفندق والجولة — مسار تجربة تجارية جاهز للعرض.",
      ctaButton: "ابدأ التخطيط",
      stars: "نجوم",
      openTour: "عرض الجولة",
      openHotel: "عرض الفندق",
      openDestination: "افتح الوجهة",
      openCatalog: "افتح الكتالوج",
      sampleNote: "البحث يوجّه إلى مسارات موجودة — ليس محرك حجز وهمي.",
      packageLabel: "باقة",
      demoHint: "عينة DEMOFEED",
    };
  }
  return {
    brandLine: "Discover · Trust · Act",
    heroEyebrow: "Travel marketplace",
    heroTitle: "Start travel the professional way",
    heroSubtitle:
      "Destinations, hotels, and tours in one transparent commerce experience — real catalog, no fake prices or availability.",
    searchDest: "Destination",
    searchDestPh: "e.g. Istanbul",
    searchDate: "Date",
    searchGuests: "Travelers",
    searchGuestsPh: "2 adults",
    searchCta: "Search trips",
    heroHotels: "Browse hotels",
    heroTours: "Browse tours",
    destinationsTitle: "Inspiring destinations",
    destinationsBlurb:
      "Live catalog destinations when available — honest discovery paths otherwise.",
    toursTitle: "Featured tours",
    toursBlurb:
      "Published tour products from the catalog — price only when Pricing owns real data.",
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
    noDestinationsTitle: "Catalog destinations coming soon",
    trustTitle: "Trust on the travel purchase path",
    storiesTitle: "Travel stories",
    seeAllStories: "All stories",
    noStoriesTitle: "Inspiration coming soon",
    noStoriesBody: "Published travelogues will appear here as inspiration cards.",
    ctaTitle: "Ready to show this platform to an agency?",
    ctaBody:
      "From destination discovery to hotels and tours — a commercial demo path.",
    ctaButton: "Start planning",
    stars: "stars",
    openTour: "View tour",
    openHotel: "View hotel",
    openDestination: "Open destination",
    openCatalog: "Open catalog",
    sampleNote: "Search routes to existing surfaces — not a fake booking engine.",
    packageLabel: "Package",
    demoHint: "DEMOFEED sample",
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
        title: "آماده دمو آژانس",
        body: "تجربهٔ عمومی برای نمایش محصول قابل فروش.",
      },
      {
        title: "بدون ادعای دروغ",
        body: "امتیاز، تخفیف و موجودی جعلی نمایش داده نمی‌شود.",
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
        title: "جاهز لعرض الوكالة",
        body: "تجربة عامة لعرض منتج قابل للبيع.",
      },
      {
        title: "بدون ادعاءات كاذبة",
        body: "لا تقييمات أو خصومات أو توفر وهمي.",
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
      title: "Agency-demo ready",
      body: "Public experience shaped for a sellable product walkthrough.",
    },
    {
      title: "No fake claims",
      body: "No invented ratings, discounts, or inventory counts.",
    },
  ];
}

function fallbackDestinationCards(locale: AppLocale) {
  return [
    {
      href: `/${locale}/tours`,
      title: locale === "fa" ? "تورهای مقصد" : locale === "ar" ? "جولات الوجهات" : "Destination tours",
      blurb:
        locale === "fa"
          ? "ورود به کاتالوگ تور"
          : locale === "ar"
            ? "ادخل كتالوج الجولات"
            : "Enter the tour catalog",
      tone: "from-[#0d47a1] via-[#1565c0] to-[#42a5f5]",
    },
    {
      href: `/${locale}/hotels`,
      title: locale === "fa" ? "اقامتگاه‌ها" : locale === "ar" ? "الإقامة" : "Stays",
      blurb:
        locale === "fa"
          ? "کشف هتل‌های عمومی"
          : locale === "ar"
            ? "اكتشف الفنادق العامة"
            : "Browse public hotels",
      tone: "from-[#1b5e20] via-[#2e7d32] to-[#c0ca33]",
    },
    {
      href: `/${locale}/plan`,
      title: locale === "fa" ? "برنامه سفر" : locale === "ar" ? "خطة الرحلة" : "Trip plan",
      blurb:
        locale === "fa"
          ? "شروع طراحی برنامه"
          : locale === "ar"
            ? "ابدأ التخطيط"
            : "Start planning",
      tone: "from-[#4a148c] via-[#6a1b9a] to-[#f9a825]",
    },
    {
      href: `/${locale}/travelogues`,
      title: locale === "fa" ? "الهام سفر" : locale === "ar" ? "إلهام السفر" : "Travel inspiration",
      blurb:
        locale === "fa"
          ? "سفرنامه‌ها و داستان‌ها"
          : locale === "ar"
            ? "قصص السفر"
            : "Stories and travelogues",
      tone: "from-[#e65100] via-[#fb8c00] to-[#f9a825]",
    },
  ];
}

function SectionHeading({
  title,
  blurb,
  action,
}: {
  title: string;
  blurb?: string;
  action?: ReactNode;
}) {
  return (
    <div className="mb-5 flex flex-wrap items-end justify-between gap-3">
      <div className="space-y-1">
        <h2 className="text-xl font-semibold tracking-tight text-foreground sm:text-2xl">
          {title}
        </h2>
        {blurb ? <p className="max-w-2xl text-sm text-muted-foreground">{blurb}</p> : null}
      </div>
      {action}
    </div>
  );
}

/**
 * Public Home / Discovery — TC-P31-T003 commercial demonstration surface.
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

  return (
    <div className="pb-14">
      <section
        aria-labelledby="home-hero-title"
        className="relative overflow-hidden border-b border-border"
      >
        <div
          aria-hidden
          className="absolute inset-0 bg-[linear-gradient(135deg,#071a3a_0%,#0d47a1_38%,#1565c0_68%,#f9a825_125%)]"
        />
        <div
          aria-hidden
          className="absolute inset-0 opacity-50"
          style={{
            backgroundImage:
              "radial-gradient(ellipse at 12% 18%, rgba(255,255,255,0.28) 0%, transparent 42%), radial-gradient(ellipse at 88% 8%, rgba(249,168,37,0.6) 0%, transparent 38%), radial-gradient(ellipse at 72% 85%, rgba(30,136,229,0.35) 0%, transparent 48%)",
          }}
        />
        <div
          aria-hidden
          className="absolute inset-x-0 bottom-0 h-24 bg-gradient-to-t from-background/25 to-transparent"
        />

        <Container width="wide" className="relative py-12 sm:py-16 lg:py-20">
          <div className="grid gap-8 lg:grid-cols-[1.2fr_0.8fr] lg:items-end">
            <div className="max-w-2xl space-y-5 text-primary-foreground">
              <p className="text-xs font-semibold uppercase tracking-[0.2em] text-accent">
                {copy.heroEyebrow} · {copy.brandLine}
              </p>
              <h1
                id="home-hero-title"
                className="text-3xl font-semibold tracking-tight sm:text-4xl lg:text-5xl"
              >
                {copy.heroTitle}
              </h1>
              <p className="max-w-xl text-base text-primary-foreground/90 sm:text-lg">
                {copy.heroSubtitle}
              </p>
              <div className="flex flex-wrap gap-3 pt-1">
                <Link
                  href={`/${locale}/hotels`}
                  className="min-h-touch inline-flex items-center rounded-md bg-accent px-5 text-sm font-semibold text-accent-foreground hover:opacity-95"
                >
                  {copy.heroHotels}
                </Link>
                <Link
                  href={`/${locale}/tours`}
                  className="min-h-touch inline-flex items-center rounded-md border border-primary-foreground/35 bg-primary-foreground/10 px-5 text-sm font-medium text-primary-foreground hover:bg-primary-foreground/20"
                >
                  {copy.heroTours}
                </Link>
              </div>
            </div>

            <form
              action={toursAction}
              method="get"
              className="rounded-2xl border border-white/25 bg-white/95 p-4 text-foreground shadow-xl backdrop-blur sm:p-5"
            >
              <p className="mb-3 text-sm font-semibold text-primary">{copy.searchCta}</p>
              <div className="grid gap-3 sm:grid-cols-2">
                <label className="block space-y-1 text-xs font-medium text-muted-foreground">
                  <span>{copy.searchDest}</span>
                  <input
                    name="destination"
                    placeholder={copy.searchDestPh}
                    className="min-h-touch w-full rounded-md border border-border bg-surface px-3 text-sm text-foreground outline-none ring-primary focus:ring-2"
                  />
                </label>
                <label className="block space-y-1 text-xs font-medium text-muted-foreground">
                  <span>{copy.searchDate}</span>
                  <input
                    type="date"
                    name="date"
                    className="min-h-touch w-full rounded-md border border-border bg-surface px-3 text-sm text-foreground outline-none ring-primary focus:ring-2"
                  />
                </label>
                <label className="block space-y-1 text-xs font-medium text-muted-foreground sm:col-span-2">
                  <span>{copy.searchGuests}</span>
                  <input
                    name="guests"
                    placeholder={copy.searchGuestsPh}
                    className="min-h-touch w-full rounded-md border border-border bg-surface px-3 text-sm text-foreground outline-none ring-primary focus:ring-2"
                  />
                </label>
              </div>
              <button
                type="submit"
                className="mt-4 min-h-touch inline-flex w-full items-center justify-center rounded-md bg-primary px-4 text-sm font-semibold text-primary-foreground hover:opacity-95"
              >
                {copy.searchCta}
              </button>
              <p className="mt-2 text-[11px] text-muted-foreground">{copy.sampleNote}</p>
            </form>
          </div>
        </Container>
      </section>

      <Container width="wide" className="space-y-14 pt-10 sm:pt-14">
        <section aria-labelledby="home-destinations-title">
          <SectionHeading
            title={copy.destinationsTitle}
            blurb={copy.destinationsBlurb}
          />
          {liveDestinations.length > 0 ? (
            <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
              {liveDestinations.map((item, index) => (
                <li key={item.destinationId}>
                  <Link
                    href={`/${locale}/destinations/${encodeURIComponent(item.slug)}`}
                    className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition hover:border-primary/40 hover:shadow-md"
                  >
                    <div
                      className={cn(
                        "relative h-40 bg-gradient-to-br",
                        index % 2 === 0
                          ? "from-[#0d47a1] via-[#1565c0] to-[#f9a825]"
                          : "from-[#01579b] via-[#0288d1] to-[#81d4fa]",
                      )}
                    >
                      <div className="absolute inset-0 bg-[radial-gradient(circle_at_30%_20%,rgba(255,255,255,0.28),transparent_55%)]" />
                      {item.slug.startsWith("demofeed-") ? (
                        <span className="absolute bottom-3 start-3 rounded-md bg-background/85 px-2 py-1 text-[11px] font-medium text-foreground">
                          {copy.demoHint}
                        </span>
                      ) : null}
                    </div>
                    <div className="flex flex-1 flex-col gap-1 p-4">
                      <span className="text-base font-semibold text-foreground group-hover:text-primary">
                        {item.name}
                      </span>
                      {item.description ? (
                        <span className="line-clamp-2 text-sm text-muted-foreground">
                          {item.description}
                        </span>
                      ) : (
                        <span className="text-sm text-muted-foreground">
                          {copy.openDestination}
                        </span>
                      )}
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
                    className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition hover:border-primary/40 hover:shadow-md"
                  >
                    <div className={cn("relative h-36 bg-gradient-to-br", item.tone)}>
                      <div className="absolute inset-0 bg-[radial-gradient(circle_at_30%_20%,rgba(255,255,255,0.28),transparent_55%)]" />
                    </div>
                    <div className="flex flex-1 flex-col gap-1 p-4">
                      <span className="text-base font-semibold text-foreground group-hover:text-primary">
                        {item.title}
                      </span>
                      <span className="text-sm text-muted-foreground">{item.blurb}</span>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section aria-labelledby="home-tours-title">
          <SectionHeading
            title={copy.toursTitle}
            blurb={copy.toursBlurb}
            action={
              <Link
                href={`/${locale}/tours`}
                className="min-h-touch inline-flex items-center text-sm font-medium text-primary underline-offset-2 hover:underline"
              >
                {copy.toursCta}
              </Link>
            }
          />
          {liveTours.length === 0 ? (
            <Surface className="overflow-hidden p-0">
              <div className="grid gap-0 md:grid-cols-[1fr_1.4fr]">
                <div className="min-h-36 bg-gradient-to-br from-primary via-primary/80 to-accent" />
                <div className="space-y-3 p-5">
                  <p className="text-sm font-semibold text-foreground">{copy.noToursTitle}</p>
                  <Text role="muted">{copy.noToursBody}</Text>
                  <Link
                    href={`/${locale}/tours`}
                    className="inline-flex text-sm font-medium text-primary underline-offset-2 hover:underline"
                  >
                    {copy.openCatalog} →
                  </Link>
                </div>
              </div>
            </Surface>
          ) : (
            <ul className="grid grid-cols-1 gap-4 md:grid-cols-3">
              {liveTours.map((item, index) => (
                <li key={item.tourProductId}>
                  <Link
                    href={`/${locale}/tours/${encodeURIComponent(item.slug)}`}
                    className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition hover:border-primary/40 hover:shadow-md"
                  >
                    <div
                      className={cn(
                        "relative h-36 bg-gradient-to-br",
                        index % 3 === 0
                          ? "from-[#0d47a1] to-[#42a5f5]"
                          : index % 3 === 1
                            ? "from-[#1b5e20] to-[#66bb6a]"
                            : "from-[#e65100] to-[#f9a825]",
                      )}
                    >
                      {item.code.startsWith("demofeed-") ? (
                        <span className="absolute bottom-3 start-3 rounded-md bg-background/85 px-2 py-1 text-[11px] font-medium text-foreground">
                          {copy.demoHint}
                        </span>
                      ) : null}
                    </div>
                    <div className="flex flex-1 flex-col gap-2 p-4">
                      <span className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                        {item.kind || copy.packageLabel}
                      </span>
                      <span className="text-base font-semibold text-foreground group-hover:text-primary">
                        {item.name}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        <LtrValue>{item.code}</LtrValue>
                      </span>
                      <span className="mt-auto pt-2 text-sm font-medium text-primary">
                        {copy.openTour} →
                      </span>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section aria-labelledby="home-hotels-title">
          <SectionHeading
            title={copy.hotelsTitle}
            blurb={copy.hotelsBlurb}
            action={
              <Link
                href={`/${locale}/hotels`}
                className="min-h-touch inline-flex items-center text-sm font-medium text-primary underline-offset-2 hover:underline"
              >
                {copy.seeAllHotels}
              </Link>
            }
          />
          {hotels.length === 0 ? (
            <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
              {[0, 1, 2].map((i) => (
                <Surface key={i} className="overflow-hidden p-0">
                  <div className="h-32 bg-gradient-to-br from-surface-muted to-primary/20" />
                  <div className="space-y-2 p-4">
                    {i === 0 ? (
                      <>
                        <p className="text-sm font-semibold text-foreground">
                          {copy.noHotelsTitle}
                        </p>
                        <p className="text-sm text-muted-foreground">{copy.noHotelsBody}</p>
                        <Link
                          href={`/${locale}/hotels`}
                          className="inline-flex text-sm font-medium text-primary underline-offset-2 hover:underline"
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
            <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {hotels.map((item) => (
                <li key={item.placeId}>
                  <Link
                    href={`/${locale}/hotels/${encodeURIComponent(item.slug)}`}
                    className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition hover:border-primary/40 hover:shadow-md"
                  >
                    <div className="relative h-36 bg-gradient-to-br from-surface-muted via-primary/25 to-accent/50">
                      {item.slug.startsWith("demofeed-") ? (
                        <span className="absolute bottom-3 start-3 rounded-md bg-background/85 px-2 py-1 text-[11px] font-medium text-foreground">
                          {copy.demoHint}
                        </span>
                      ) : null}
                    </div>
                    <div className="flex flex-1 flex-col gap-2 p-4">
                      <span className="text-base font-semibold text-foreground group-hover:text-primary">
                        {item.name}
                      </span>
                      {item.description ? (
                        <span className="line-clamp-2 text-sm text-muted-foreground">
                          {item.description}
                        </span>
                      ) : null}
                      {item.starRating != null ? (
                        <span className="text-xs text-muted-foreground">
                          {item.starRating} {copy.stars}
                        </span>
                      ) : null}
                      <span className="mt-auto pt-1 text-sm font-medium text-primary">
                        {copy.openHotel} →
                      </span>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section aria-labelledby="home-trust-title">
          <SectionHeading title={copy.trustTitle} />
          <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {trust.map((item) => (
              <li key={item.title}>
                <Surface className="h-full border-primary/10 bg-gradient-to-br from-surface to-primary/5">
                  <p className="text-sm font-semibold text-primary">{item.title}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{item.body}</p>
                </Surface>
              </li>
            ))}
          </ul>
        </section>

        <section aria-labelledby="home-stories-title">
          <SectionHeading
            title={copy.storiesTitle}
            action={
              <Link
                href={`/${locale}/travelogues`}
                className="min-h-touch inline-flex items-center text-sm font-medium text-primary underline-offset-2 hover:underline"
              >
                {copy.seeAllStories}
              </Link>
            }
          />
          {travelogues.length === 0 ? (
            <Surface className="overflow-hidden p-0">
              <div className="grid gap-0 md:grid-cols-[1fr_1.2fr]">
                <div className="min-h-32 bg-gradient-to-br from-primary/80 to-accent/70" />
                <div className="space-y-2 p-5">
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
                    className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition hover:border-primary/40 hover:shadow-md"
                  >
                    <div className="h-28 bg-gradient-to-br from-primary/70 to-accent/60" />
                    <div className="flex flex-1 flex-col gap-2 p-4">
                      <span className="text-base font-semibold text-foreground group-hover:text-primary">
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
          <div className="rounded-2xl bg-gradient-to-r from-primary via-primary to-accent/80 px-6 py-10 text-primary-foreground shadow-md sm:px-10">
            <Stack gap="md">
              <h2 id="home-cta-title" className="text-2xl font-semibold tracking-tight">
                {copy.ctaTitle}
              </h2>
              <p className="max-w-2xl text-sm text-primary-foreground/90 sm:text-base">
                {copy.ctaBody}
              </p>
              <div className="flex flex-wrap gap-3">
                <Link
                  href={`/${locale}/plan`}
                  className="min-h-touch inline-flex items-center rounded-md bg-accent px-5 text-sm font-semibold text-accent-foreground hover:opacity-95"
                >
                  {copy.ctaButton}
                </Link>
                <Link
                  href={`/${locale}/hotels`}
                  className="min-h-touch inline-flex items-center rounded-md border border-primary-foreground/40 px-5 text-sm font-medium text-primary-foreground hover:bg-primary-foreground/10"
                >
                  {copy.heroHotels}
                </Link>
              </div>
            </Stack>
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
