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
      brandLine: "کشف · اعتماد · رزرو",
      heroEyebrow: "Travel commerce",
      heroTitle: "کجا می‌خواهید سفر کنید؟",
      heroSubtitle:
        "تور، هتل و الهام سفر — تجربهٔ گردشگری مدرن با مسیر شفاف خرید.",
      searchDest: "مقصد",
      searchDestPh: "مثلاً استانبول",
      searchDate: "تاریخ",
      searchGuests: "مسافر",
      searchGuestsPh: "۲ بزرگسال",
      searchCta: "جستجوی سفر",
      heroSecondary: "مشاهده هتل‌ها",
      discoverTitle: "شروع کشف",
      destinationsTitle: "مقاصد الهام‌بخش",
      destinationsBlurb: "کارت‌های بصری برای ورود به کشف — بدون موجودی جعلی.",
      toursTitle: "تورهای منتخب",
      toursBlurb: "نمایش محصول‌محور کاتالوگ تور — قیمت فقط اگر داده واقعی باشد.",
      toursCta: "همه تورها",
      hotelsTitle: "هتل‌های محبوب",
      hotelsBlurb: "کاتالوگ هتل فعال؛ خالی بودن صادقانه ولی با UI آماده.",
      seeAllHotels: "همه هتل‌ها",
      noHotelsTitle: "هتل‌ها به‌زودی در پیش‌نمایش عمومی",
      noHotelsBody: "ساختار کارت آماده است. تا دادهٔ عمومی واقعی، قیمت یا امتیاز جعلی نشان نمی‌دهیم.",
      trustTitle: "اعتماد در مسیر سفر",
      storiesTitle: "سفرنامه‌ها",
      seeAllStories: "همه سفرنامه‌ها",
      noStoriesTitle: "الهام سفر به‌زودی",
      noStoriesBody: "وقتی سفرنامهٔ منتشرشده باشد، اینجا کارت‌های الهام نمایش داده می‌شود.",
      ctaTitle: "آماده‌اید برنامه سفر بسازید؟",
      ctaBody: "از کشف تا اعتماد تا اقدام — مسیر محصول واضح است.",
      ctaButton: "شروع برنامه‌ریزی",
      stars: "ستاره",
      viewTour: "مشاهده تور",
      openCatalog: "ورود به کاتالوگ",
      sampleNote: "نمونه مسیر کشف",
    };
  }
  if (locale === "ar") {
    return {
      brandLine: "اكتشف · ثق · احجز",
      heroEyebrow: "Travel commerce",
      heroTitle: "إلى أين تريد أن تسافر؟",
      heroSubtitle: "جولات وفنادق وإلهام سفر — تجربة حديثة بمسار شراء واضح.",
      searchDest: "الوجهة",
      searchDestPh: "مثل إسطنبول",
      searchDate: "التاريخ",
      searchGuests: "المسافرون",
      searchGuestsPh: "بالغان",
      searchCta: "ابحث عن رحلة",
      heroSecondary: "عرض الفنادق",
      discoverTitle: "ابدأ الاكتشاف",
      destinationsTitle: "وجهات ملهمة",
      destinationsBlurb: "بطاقات بصرية لاكتشاف صادق — دون مخزون وهمي.",
      toursTitle: "جولات مختارة",
      toursBlurb: "عرض منتجي للكتالوج — السعر فقط عند توفر بيانات حقيقية.",
      toursCta: "كل الجولات",
      hotelsTitle: "فنادق شائعة",
      hotelsBlurb: "كتالوج الفنادق النشطة؛ فراغ صادق مع واجهة جاهزة.",
      seeAllHotels: "كل الفنادق",
      noHotelsTitle: "الفنادق قريباً في المعاينة العامة",
      noHotelsBody: "هيكل البطاقة جاهز. لا أسعار أو تقييمات وهمية.",
      trustTitle: "الثقة في مسار السفر",
      storiesTitle: "قصص السفر",
      seeAllStories: "كل القصص",
      noStoriesTitle: "إلهام السفر قريباً",
      noStoriesBody: "عند توفر القصص المنشورة ستظهر هنا بطاقات الإلهام.",
      ctaTitle: "جاهز لتخطيط رحلتك؟",
      ctaBody: "اكتشف · ثق · ابدأ.",
      ctaButton: "ابدأ التخطيط",
      stars: "نجوم",
      viewTour: "عرض الجولة",
      openCatalog: "افتح الكتالوج",
      sampleNote: "مسار اكتشاف نموذجي",
    };
  }
  return {
    brandLine: "Discover · Trust · Book",
    heroEyebrow: "Travel commerce",
    heroTitle: "Where do you want to travel?",
    heroSubtitle:
      "Tours, hotels, and travel inspiration — a modern commerce path with honest empty states.",
    searchDest: "Destination",
    searchDestPh: "e.g. Istanbul",
    searchDate: "Date",
    searchGuests: "Travelers",
    searchGuestsPh: "2 adults",
    searchCta: "Search trips",
    heroSecondary: "Browse hotels",
    discoverTitle: "Start discovering",
    destinationsTitle: "Inspiring destinations",
    destinationsBlurb: "Visual entry cards for discovery — no fake inventory.",
    toursTitle: "Featured tours",
    toursBlurb: "Product-like tour presentation — price only when real.",
    toursCta: "All tours",
    hotelsTitle: "Popular hotels",
    hotelsBlurb: "Active hotel catalog; honest empty with ready UI.",
    seeAllHotels: "All hotels",
    noHotelsTitle: "Hotels coming to public preview",
    noHotelsBody:
      "Card structure is ready. No invented prices or ratings until public data exists.",
    trustTitle: "Trust on the journey",
    storiesTitle: "Travel stories",
    seeAllStories: "All stories",
    noStoriesTitle: "Inspiration coming soon",
    noStoriesBody: "Published travelogues will appear here as inspiration cards.",
    ctaTitle: "Ready to plan your trip?",
    ctaBody: "Discover · Trust · Act — a clear product path.",
    ctaButton: "Start planning",
    stars: "stars",
    viewTour: "View tour",
    openCatalog: "Open catalog",
    sampleNote: "Sample discovery path",
  };
}

function trustItems(locale: AppLocale) {
  if (locale === "fa") {
    return [
      { title: "پشتیبانی مسیر سفر", body: "همراهی در کشف و برنامه‌ریزی." },
      { title: "پرداخت امن", body: "مسیر رزرو/پرداخت وقتی فعال شود — بدون ادعای جعلی." },
      { title: "کشف curated", body: "ترکیب صادقانه، نه فید گمراه‌کننده." },
      { title: "تجربه شفاف", body: "بدون قیمت و امتیاز ساختگی." },
    ];
  }
  if (locale === "ar") {
    return [
      { title: "دعم مسار السفر", body: "مرافقة في الاكتشاف والتخطيط." },
      { title: "دفع آمن", body: "مسار الحجز/الدفع عند التفعيل — دون ادعاءات وهمية." },
      { title: "اكتشاف منظم", body: "تكوين صادق وليس خلاصة مضللة." },
      { title: "تجربة شفافة", body: "بدون أسعار أو تقييمات مصطنعة." },
    ];
  }
  return [
    { title: "Travel support", body: "Guidance through discovery and planning." },
    { title: "Secure payment posture", body: "Booking/payment path when live — no fake claims." },
    { title: "Curated discovery", body: "Honest composition, not a misleading feed." },
    { title: "Transparent experience", body: "No invented prices or ratings." },
  ];
}

function destinationCards(locale: AppLocale) {
  const base = [
    {
      href: `/${locale}/destinations/fixture-istanbul`,
      title: locale === "fa" ? "استانبول" : locale === "ar" ? "إسطنبول" : "Istanbul",
      blurb:
        locale === "fa"
          ? "نمونه مقصد الهام‌بخش"
          : locale === "ar"
            ? "وجهة نموذجية ملهمة"
            : "Sample inspiring destination",
      tone: "from-[#0d47a1] via-[#1565c0] to-[#f9a825]",
    },
    {
      href: `/${locale}/tours`,
      title: locale === "fa" ? "تورهای مقصد" : locale === "ar" ? "جولات الوجهات" : "Destination tours",
      blurb:
        locale === "fa"
          ? "ورود بصری به کاتالوگ تور"
          : locale === "ar"
            ? "دخول بصري إلى كتالوج الجولات"
            : "Visual entry to the tour catalog",
      tone: "from-[#01579b] via-[#0277bd] to-[#4fc3f7]",
    },
    {
      href: `/${locale}/hotels`,
      title: locale === "fa" ? "اقامتگاه‌ها" : locale === "ar" ? "الإقامة" : "Stays",
      blurb:
        locale === "fa"
          ? "کشف هتل‌های عمومی"
          : locale === "ar"
            ? "اكتشاف الفنادق العامة"
            : "Browse public hotels",
      tone: "from-[#1b5e20] via-[#2e7d32] to-[#c0ca33]",
    },
    {
      href: `/${locale}/plan`,
      title: locale === "fa" ? "برنامه سفر من" : locale === "ar" ? "خطة رحلتي" : "My trip plan",
      blurb:
        locale === "fa"
          ? "شروع طراحی برنامه"
          : locale === "ar"
            ? "ابدأ تصميم الخطة"
            : "Start designing your plan",
      tone: "from-[#4a148c] via-[#6a1b9a] to-[#f9a825]",
    },
  ];
  return base;
}

function tourCatalogCards(locale: AppLocale) {
  return [
    {
      href: `/${locale}/tours`,
      title:
        locale === "fa"
          ? "تورهای شهری"
          : locale === "ar"
            ? "جولات المدن"
            : "City tours",
      meta:
        locale === "fa"
          ? "کاتالوگ عمومی"
          : locale === "ar"
            ? "كتالوج عام"
            : "Public catalog",
      tone: "from-[#0d47a1] to-[#42a5f5]",
    },
    {
      href: `/${locale}/tours`,
      title:
        locale === "fa"
          ? "تورهای طبیعت"
          : locale === "ar"
            ? "جولات الطبيعة"
            : "Nature tours",
      meta:
        locale === "fa"
          ? "کشف مسیرهای تجربه"
          : locale === "ar"
            ? "اكتشاف مسارات التجربة"
            : "Explore experience paths",
      tone: "from-[#1b5e20] to-[#66bb6a]",
    },
    {
      href: `/${locale}/tours`,
      title:
        locale === "fa"
          ? "تورهای ترکیبی"
          : locale === "ar"
            ? "جولات مجمّعة"
            : "Combined tours",
      meta:
        locale === "fa"
          ? "ورود به فهرست کامل"
          : locale === "ar"
            ? "افتح القائمة الكاملة"
            : "Open full listing",
      tone: "from-[#e65100] to-[#f9a825]",
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
 * Public Home / Discovery — TC-P30-T005-REWORK travel commerce surface.
 * Server Component. Honesty: no invented prices/availability/ratings.
 */
export function HomeDiscoveryView({
  locale,
  composition,
  includeDevLinks = false,
}: HomeDiscoveryViewProps) {
  const copy = copyFor(locale);
  const trust = trustItems(locale);
  const destinations = destinationCards(locale);
  const tourCards = tourCatalogCards(locale);
  const hotels = composition?.hotels ?? [];
  const travelogues = composition?.travelogues ?? [];
  const toursAction = `/${locale}/tours`;

  return (
    <div className="pb-14">
      {/* —— Hero: travel visual + search intent + conversion —— */}
      <section
        aria-labelledby="home-hero-title"
        className="relative overflow-hidden border-b border-border"
      >
        <div
          aria-hidden
          className="absolute inset-0 bg-[linear-gradient(135deg,#0a2f6b_0%,#0d47a1_42%,#1565c0_70%,#f9a825_140%)]"
        />
        <div
          aria-hidden
          className="absolute inset-0 opacity-40"
          style={{
            backgroundImage:
              "radial-gradient(ellipse at 15% 20%, rgba(255,255,255,0.25) 0%, transparent 45%), radial-gradient(ellipse at 85% 10%, rgba(249,168,37,0.55) 0%, transparent 40%), radial-gradient(ellipse at 70% 80%, rgba(30,136,229,0.35) 0%, transparent 50%)",
          }}
        />
        <div
          aria-hidden
          className="absolute -end-10 top-8 h-56 w-56 rounded-full bg-accent/30 blur-3xl"
        />
        <div
          aria-hidden
          className="absolute -start-16 bottom-0 h-64 w-64 rounded-full bg-sky-300/20 blur-3xl"
        />

        <Container width="wide" className="relative py-12 sm:py-16 lg:py-20">
          <div className="grid gap-8 lg:grid-cols-[1.15fr_0.85fr] lg:items-end">
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
                  className="min-h-touch inline-flex items-center rounded-md border border-primary-foreground/30 bg-primary-foreground/10 px-5 text-sm font-medium text-primary-foreground hover:bg-primary-foreground/20"
                >
                  {copy.heroSecondary}
                </Link>
              </div>
            </div>

            {/* Search experience — routes to existing honest surfaces */}
            <form
              action={toursAction}
              method="get"
              className="rounded-2xl border border-white/20 bg-white/95 p-4 text-foreground shadow-xl backdrop-blur sm:p-5"
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
                className="mt-4 min-h-touch inline-flex w-full items-center justify-center rounded-md bg-accent px-4 text-sm font-semibold text-accent-foreground hover:opacity-95"
              >
                {copy.searchCta}
              </button>
              <p className="mt-2 text-[11px] text-muted-foreground">{copy.sampleNote}</p>
            </form>
          </div>
        </Container>
      </section>

      <Container width="wide" className="space-y-14 pt-10 sm:pt-14">
        {/* —— Destinations cards —— */}
        <section aria-labelledby="home-destinations-title">
          <SectionHeading title={copy.destinationsTitle} blurb={copy.destinationsBlurb} />
          <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {destinations.map((item) => (
              <li key={item.href}>
                <Link
                  href={item.href}
                  className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition hover:border-primary/40 hover:shadow-md"
                >
                  <div
                    className={cn(
                      "relative h-36 bg-gradient-to-br",
                      item.tone,
                    )}
                  >
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
        </section>

        {/* —— Tours product cards —— */}
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
          <ul className="grid grid-cols-1 gap-4 md:grid-cols-3">
            {tourCards.map((item) => (
              <li key={item.title}>
                <Link
                  href={item.href}
                  className="group flex h-full flex-col overflow-hidden rounded-xl border border-border bg-surface shadow-sm transition hover:border-primary/40 hover:shadow-md"
                >
                  <div className={cn("h-32 bg-gradient-to-br", item.tone)} />
                  <div className="flex flex-1 flex-col gap-2 p-4">
                    <span className="text-base font-semibold text-foreground group-hover:text-primary">
                      {item.title}
                    </span>
                    <span className="text-sm text-muted-foreground">{item.meta}</span>
                    <span className="mt-auto pt-2 text-sm font-medium text-primary">
                      {copy.openCatalog} →
                    </span>
                  </div>
                </Link>
              </li>
            ))}
          </ul>
        </section>

        {/* —— Hotels —— */}
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
                  <div className="h-28 bg-gradient-to-br from-surface-muted to-primary/20" />
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
                    <div className="h-28 bg-gradient-to-br from-surface-muted to-primary/20" />
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
                      <span className="mt-auto text-xs text-muted-foreground">
                        <LtrValue>{item.slug}</LtrValue>
                      </span>
                    </div>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        {/* —— Trust strip —— */}
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

        {/* —— Stories —— */}
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
                    <div className="h-24 bg-gradient-to-br from-primary/70 to-accent/60" />
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

        {/* —— Conversion CTA —— */}
        <section aria-labelledby="home-cta-title">
          <div className="rounded-2xl bg-gradient-to-r from-primary via-primary to-primary/80 px-6 py-10 text-primary-foreground shadow-md sm:px-10">
            <Stack gap="md">
              <h2 id="home-cta-title" className="text-2xl font-semibold tracking-tight">
                {copy.ctaTitle}
              </h2>
              <p className="max-w-2xl text-sm text-primary-foreground/90 sm:text-base">
                {copy.ctaBody}
              </p>
              <div>
                <Link
                  href={`/${locale}/plan`}
                  className="min-h-touch inline-flex items-center rounded-md bg-accent px-5 text-sm font-semibold text-accent-foreground hover:opacity-95"
                >
                  {copy.ctaButton}
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
