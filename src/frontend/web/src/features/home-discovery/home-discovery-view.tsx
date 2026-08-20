import Link from "next/link";
import { Container, LtrValue, Stack, Surface, Text } from "@/components/ui";
import type { HomeDiscoveryComposition } from "@/features/home-discovery/types";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";

type DiscoveryLink = {
  href: string;
  label: string;
  hint: string;
};

export type HomeDiscoveryViewProps = {
  locale: AppLocale;
  composition?: HomeDiscoveryComposition;
  /** UIVAL dev route may include `/dev/*` links; production home must not. */
  includeDevLinks?: boolean;
};

type Copy = {
  heroTitle: string;
  heroSubtitle: string;
  heroPrimary: string;
  heroSecondary: string;
  discoverTitle: string;
  destinationsTitle: string;
  destinationsBlurb: string;
  toursTitle: string;
  toursBlurb: string;
  toursCta: string;
  hotelsTitle: string;
  hotelsBlurb: string;
  seeAllHotels: string;
  noHotels: string;
  trustTitle: string;
  storiesTitle: string;
  seeAllStories: string;
  noStories: string;
  ctaTitle: string;
  ctaBody: string;
  ctaButton: string;
  stars: string;
};

function copyFor(locale: AppLocale): Copy {
  if (locale === "fa") {
    return {
      heroTitle: "سفر بعدی‌تان را با اطمینان پیدا کنید",
      heroSubtitle:
        "تور، هتل و الهام سفر — در یک تجربهٔ گردشگری مدرن، شفاف و قابل اعتماد.",
      heroPrimary: "کشف تورها",
      heroSecondary: "مشاهده هتل‌ها",
      discoverTitle: "از کجا شروع کنیم؟",
      destinationsTitle: "مقاصد الهام‌بخش",
      destinationsBlurb: "مسیرهای شناخته‌شده برای شروع کشف — بدون وعدهٔ جعلی موجودی.",
      toursTitle: "تورهای منتخب",
      toursBlurb: "کشف تورهای عمومی با مسیر شفاف خرید.",
      toursCta: "ورود به فهرست تورها",
      hotelsTitle: "هتل‌ها",
      hotelsBlurb: "کاتالوگ هتل‌های فعال — فقط حقایق موجود.",
      seeAllHotels: "همه هتل‌ها",
      noHotels: "هنوز هتلی برای نمایش عمومی آماده نیست.",
      trustTitle: "چرا TravelCore؟",
      storiesTitle: "سفرنامه‌ها و الهام",
      seeAllStories: "همه سفرنامه‌ها",
      noStories: "هنوز سفرنامه‌ای برای پیش‌نمایش نیست.",
      ctaTitle: "آماده‌اید برنامه سفر بسازید؟",
      ctaBody: "از کشف تا اعتماد تا رزرو — مسیر محصول واضح است.",
      ctaButton: "شروع برنامه‌ریزی",
      stars: "ستاره",
    };
  }
  if (locale === "ar") {
    return {
      heroTitle: "اعثر على رحلتك التالية بثقة",
      heroSubtitle: "جولات وفنادق وإلهام سفر — تجربة سياحية حديثة وواضحة.",
      heroPrimary: "استكشف الجولات",
      heroSecondary: "عرض الفنادق",
      discoverTitle: "من أين تبدأ؟",
      destinationsTitle: "وجهات ملهمة",
      destinationsBlurb: "مسارات معروفة لبدء الاكتشاف — بدون وعود مخزون وهمية.",
      toursTitle: "جولات مختارة",
      toursBlurb: "اكتشف الجولات العامة بمسار شراء واضح.",
      toursCta: "قائمة الجولات",
      hotelsTitle: "فنادق",
      hotelsBlurb: "كتالوج الفنادق النشطة — الحقائق المتوفرة فقط.",
      seeAllHotels: "كل الفنادق",
      noHotels: "لا فنادق جاهزة للعرض العام بعد.",
      trustTitle: "لماذا TravelCore؟",
      storiesTitle: "قصص السفر",
      seeAllStories: "كل القصص",
      noStories: "لا قصص للمعاينة بعد.",
      ctaTitle: "جاهز لتخطيط رحلتك؟",
      ctaBody: "اكتشف · ثق · احجز — مسار منتج واضح.",
      ctaButton: "ابدأ التخطيط",
      stars: "نجوم",
    };
  }
  return {
    heroTitle: "Find your next trip with confidence",
    heroSubtitle:
      "Tours, hotels, and travel inspiration — a modern, trustworthy commerce experience.",
    heroPrimary: "Explore tours",
    heroSecondary: "Browse hotels",
    discoverTitle: "Where do you want to start?",
    destinationsTitle: "Inspiring destinations",
    destinationsBlurb: "Known discovery paths to begin — no fake inventory promises.",
    toursTitle: "Featured tours",
    toursBlurb: "Public tour discovery with a clear path to book.",
    toursCta: "Open tour listing",
    hotelsTitle: "Hotels",
    hotelsBlurb: "Active hotel catalog — only facts we actually have.",
    seeAllHotels: "All hotels",
    noHotels: "No hotels are ready for public preview yet.",
    trustTitle: "Why TravelCore?",
    storiesTitle: "Travel stories",
    seeAllStories: "All stories",
    noStories: "No travelogues to preview yet.",
    ctaTitle: "Ready to plan your trip?",
    ctaBody: "Discover · Trust · Book — a clear product path.",
    ctaButton: "Start planning",
    stars: "stars",
  };
}

function trustItems(locale: AppLocale) {
  if (locale === "fa") {
    return [
      { title: "شفافیت", body: "بدون موجودی، قیمت یا امتیاز جعلی." },
      { title: "کشف واقعی", body: "ترکیب curated — نه فید شخصی‌سازی‌شده گمراه‌کننده." },
      { title: "مسیر خرید", body: "از کشف تا اعتماد تا اقدام — واضح و قابل پیگیری." },
      { title: "موبایل‌اول", body: "تجربهٔ لمسی و خوانا برای سفر روزمره." },
    ];
  }
  if (locale === "ar") {
    return [
      { title: "شفافية", body: "بدون مخزون أو أسعار أو تقييمات وهمية." },
      { title: "اكتشاف حقيقي", body: "تكوين منظم — ليس خلاصة مخصصة مضللة." },
      { title: "مسار شراء", body: "من الاكتشاف إلى الثقة إلى الإجراء." },
      { title: "الجوال أولاً", body: "تجربة لمسية وواضحة للسفر اليومي." },
    ];
  }
  return [
    { title: "Transparency", body: "No fake inventory, prices, or ratings." },
    { title: "Honest discovery", body: "Curated composition — not a misleading personalized feed." },
    { title: "Clear path", body: "Discover → Trust → Book — actionable and trackable." },
    { title: "Mobile-first", body: "Touch-friendly travel browsing for everyday trips." },
  ];
}

function destinationEntries(locale: AppLocale) {
  // Honest entry points only — no invented /destinations index (slug pages exist).
  if (locale === "fa") {
    return [
      {
        href: `/${locale}/destinations/fixture-istanbul`,
        label: "نمونه مقصد الهام‌بخش",
        tone: "primary" as const,
      },
      { href: `/${locale}/tours`, label: "تورهای مقصد", tone: "muted" as const },
      { href: `/${locale}/hotels`, label: "هتل‌های مقصد", tone: "muted" as const },
      { href: `/${locale}/plan`, label: "برنامه مقصد من", tone: "muted" as const },
    ];
  }
  if (locale === "ar") {
    return [
      {
        href: `/${locale}/destinations/fixture-istanbul`,
        label: "وجهة نموذجية ملهمة",
        tone: "primary" as const,
      },
      { href: `/${locale}/tours`, label: "جولات الوجهات", tone: "muted" as const },
      { href: `/${locale}/hotels`, label: "فنادق الوجهات", tone: "muted" as const },
      { href: `/${locale}/plan`, label: "خطط وجهتي", tone: "muted" as const },
    ];
  }
  return [
    {
      href: `/${locale}/destinations/fixture-istanbul`,
      label: "Sample destination",
      tone: "primary" as const,
    },
    { href: `/${locale}/tours`, label: "Destination tours", tone: "muted" as const },
    { href: `/${locale}/hotels`, label: "Destination hotels", tone: "muted" as const },
    { href: `/${locale}/plan`, label: "Plan my destination", tone: "muted" as const },
  ];
}

function productionLinks(locale: AppLocale): DiscoveryLink[] {
  if (locale === "fa") {
    return [
      { href: `/${locale}/tours`, label: "تور", hint: "کشف و مقایسه تورها" },
      { href: `/${locale}/hotels`, label: "هتل", hint: "کاتالوگ هتل" },
      { href: `/${locale}/plan`, label: "برنامه‌ریز", hint: "شروع برنامه سفر" },
      { href: `/${locale}/flights`, label: "پرواز", hint: "جستجوی پرواز" },
      { href: `/${locale}/travelogues`, label: "سفرنامه", hint: "الهام از تجربه واقعی" },
      { href: `/${locale}/visas/TR`, label: "ویزا", hint: "اطلاعات ویزا" },
    ];
  }
  if (locale === "ar") {
    return [
      { href: `/${locale}/tours`, label: "جولات", hint: "اكتشاف الجولات" },
      { href: `/${locale}/hotels`, label: "فنادق", hint: "كتالوج الفنادق" },
      { href: `/${locale}/plan`, label: "مخطط", hint: "ابدأ التخطيط" },
      { href: `/${locale}/flights`, label: "رحلات", hint: "بحث الرحلات" },
      { href: `/${locale}/travelogues`, label: "قصص", hint: "إلهام حقيقي" },
      { href: `/${locale}/visas/TR`, label: "تأشيرة", hint: "معلومات التأشيرة" },
    ];
  }
  return [
    { href: `/${locale}/tours`, label: "Tours", hint: "Discover tours" },
    { href: `/${locale}/hotels`, label: "Hotels", hint: "Hotel catalog" },
    { href: `/${locale}/plan`, label: "Planner", hint: "Start trip planning" },
    { href: `/${locale}/flights`, label: "Flights", hint: "Flight search" },
    { href: `/${locale}/travelogues`, label: "Stories", hint: "Real travel inspiration" },
    { href: `/${locale}/visas/TR`, label: "Visa", hint: "Visa information" },
  ];
}

function devValidationLinks(locale: AppLocale): DiscoveryLink[] {
  return [
    {
      href: `/${locale}/destinations/fixture-istanbul`,
      label: locale === "fa" ? "مقصد نمونه (fixture)" : "Sample destination (fixture)",
      hint: "Destination landing fixture",
    },
    {
      href: `/${locale}/dev/foundation`,
      label: locale === "fa" ? "اعتبارسنجی primitives" : "Foundation validation",
      hint: "dev-only",
    },
    {
      href: `/${locale}/dev/shells`,
      label: locale === "fa" ? "بورد Shell" : "Shell board",
      hint: "dev-only",
    },
  ];
}

function SectionHeading({
  title,
  blurb,
}: {
  title: string;
  blurb?: string;
}) {
  return (
    <div className="mb-4 space-y-1">
      <h2 className="text-xl font-semibold tracking-tight text-foreground sm:text-2xl">
        {title}
      </h2>
      {blurb ? <p className="max-w-2xl text-sm text-muted-foreground">{blurb}</p> : null}
    </div>
  );
}

/**
 * Public Home / Discovery surface — TC-P30-T005 sellable experience.
 * Curated composition from public loaders — not personalized feed · not Search engine.
 * Server Component.
 */
export function HomeDiscoveryView({
  locale,
  composition,
  includeDevLinks = false,
}: HomeDiscoveryViewProps) {
  const copy = copyFor(locale);
  const trust = trustItems(locale);
  const destinations = destinationEntries(locale);
  const links = includeDevLinks
    ? [...productionLinks(locale), ...devValidationLinks(locale)]
    : productionLinks(locale);

  const travelogues = composition?.travelogues ?? [];
  const hotels = composition?.hotels ?? [];

  return (
    <div className="pb-12">
      {/* —— 1. Hero —— */}
      <section
        aria-labelledby="home-hero-title"
        className="relative overflow-hidden border-b border-border bg-primary text-primary-foreground"
      >
        <div
          aria-hidden
          className="pointer-events-none absolute inset-0 opacity-30"
          style={{
            backgroundImage:
              "radial-gradient(ellipse at 20% 20%, #1e88e5 0%, transparent 55%), radial-gradient(ellipse at 80% 0%, #f9a825 0%, transparent 40%)",
          }}
        />
        <Container width="wide" className="relative py-14 sm:py-20">
          <div className="max-w-3xl space-y-5">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-accent">
              Discover · Trust · Book
            </p>
            <h1
              id="home-hero-title"
              className="text-3xl font-semibold tracking-tight sm:text-4xl lg:text-5xl"
            >
              {copy.heroTitle}
            </h1>
            <p className="max-w-2xl text-base text-primary-foreground/90 sm:text-lg">
              {copy.heroSubtitle}
            </p>
            <div className="flex flex-wrap gap-3 pt-2">
              <Link
                href={`/${locale}/tours`}
                className="min-h-touch inline-flex items-center rounded-md bg-accent px-5 text-sm font-semibold text-accent-foreground shadow-sm hover:opacity-95"
              >
                {copy.heroPrimary}
              </Link>
              <Link
                href={`/${locale}/hotels`}
                className="min-h-touch inline-flex items-center rounded-md border border-primary-foreground/30 bg-primary-foreground/10 px-5 text-sm font-medium text-primary-foreground hover:bg-primary-foreground/20"
              >
                {copy.heroSecondary}
              </Link>
            </div>
          </div>
        </Container>
      </section>

      <Container width="wide" className="space-y-14 pt-10 sm:pt-14">
        {/* —— 2. Discovery entry —— */}
        <section aria-labelledby="home-discover-title">
          <SectionHeading title={copy.discoverTitle} />
          <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {links.map((item) => (
              <li key={item.href}>
                <Link
                  href={item.href}
                  className="group min-h-touch flex h-full flex-col rounded-lg border border-border bg-surface p-4 shadow-sm transition hover:border-primary/40 hover:shadow-md"
                >
                  <span className="text-base font-semibold text-foreground group-hover:text-primary">
                    {item.label}
                  </span>
                  <span className="mt-1 text-sm text-muted-foreground">{item.hint}</span>
                </Link>
              </li>
            ))}
          </ul>
        </section>

        {/* —— 3. Destinations entry —— */}
        <section aria-labelledby="home-destinations-title">
          <SectionHeading
            title={copy.destinationsTitle}
            blurb={copy.destinationsBlurb}
          />
          <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {destinations.map((item) => (
              <li key={item.href}>
                <Link
                  href={item.href}
                  className={cn(
                    "min-h-touch flex items-center justify-center rounded-lg px-4 py-8 text-center text-sm font-semibold shadow-sm",
                    item.tone === "primary"
                      ? "bg-primary text-primary-foreground hover:opacity-95"
                      : "border border-border bg-surface-muted text-foreground hover:border-primary/30",
                  )}
                >
                  {item.label}
                </Link>
              </li>
            ))}
          </ul>
        </section>

        {/* —— 4. Tours discovery entry —— */}
        <section aria-labelledby="home-tours-title">
          <Surface className="overflow-hidden p-0">
            <div className="grid gap-0 lg:grid-cols-[1.2fr_1fr]">
              <div className="space-y-4 p-6 sm:p-8">
                <SectionHeading title={copy.toursTitle} blurb={copy.toursBlurb} />
                <Link
                  href={`/${locale}/tours`}
                  className="min-h-touch inline-flex items-center rounded-md bg-primary px-4 text-sm font-semibold text-primary-foreground hover:opacity-95"
                >
                  {copy.toursCta}
                </Link>
              </div>
              <div className="flex items-end bg-gradient-to-br from-primary/90 to-primary p-6 text-primary-foreground sm:p-8">
                <p className="text-sm opacity-90">
                  TravelCore · curated tour discovery · honest empty when catalog is quiet
                </p>
              </div>
            </div>
          </Surface>
        </section>

        {/* —— 5. Hotels —— */}
        <section aria-labelledby="home-hotels-title">
          <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
            <SectionHeading title={copy.hotelsTitle} blurb={copy.hotelsBlurb} />
            <Link
              href={`/${locale}/hotels`}
              className="min-h-touch inline-flex items-center text-sm font-medium text-primary underline-offset-2 hover:underline"
            >
              {copy.seeAllHotels}
            </Link>
          </div>
          {hotels.length === 0 ? (
            <Surface tone="muted">
              <Text role="muted">{copy.noHotels}</Text>
            </Surface>
          ) : (
            <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {hotels.map((item) => (
                <li key={item.placeId}>
                  <Link
                    href={`/${locale}/hotels/${encodeURIComponent(item.slug)}`}
                    className="group flex h-full flex-col rounded-lg border border-border bg-surface shadow-sm transition hover:border-primary/40 hover:shadow-md"
                  >
                    <div className="h-28 rounded-t-lg bg-gradient-to-br from-surface-muted to-primary/15" />
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

        {/* —— 6. Trust —— */}
        <section aria-labelledby="home-trust-title">
          <SectionHeading title={copy.trustTitle} />
          <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {trust.map((item) => (
              <li key={item.title}>
                <Surface className="h-full">
                  <p className="text-sm font-semibold text-primary">{item.title}</p>
                  <p className="mt-2 text-sm text-muted-foreground">{item.body}</p>
                </Surface>
              </li>
            ))}
          </ul>
        </section>

        {/* —— 7. Stories —— */}
        <section aria-labelledby="home-stories-title">
          <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
            <SectionHeading title={copy.storiesTitle} />
            <Link
              href={`/${locale}/travelogues`}
              className="min-h-touch inline-flex items-center text-sm font-medium text-primary underline-offset-2 hover:underline"
            >
              {copy.seeAllStories}
            </Link>
          </div>
          {travelogues.length === 0 ? (
            <Surface tone="muted">
              <Text role="muted">{copy.noStories}</Text>
            </Surface>
          ) : (
            <ul className="grid grid-cols-1 gap-4 md:grid-cols-3">
              {travelogues.map((item) => (
                <li key={item.travelogueId}>
                  <Link
                    href={`/${locale}/travelogues/${encodeURIComponent(item.travelogueId)}`}
                    className="group flex h-full flex-col rounded-lg border border-border bg-surface p-4 shadow-sm transition hover:border-primary/40 hover:shadow-md"
                  >
                    <span className="text-base font-semibold text-foreground group-hover:text-primary">
                      {item.title}
                    </span>
                    <span className="mt-2 line-clamp-3 text-sm text-muted-foreground">
                      {item.body.slice(0, 160)}
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </section>

        {/* —— 8. Conversion CTA —— */}
        <section aria-labelledby="home-cta-title">
          <div className="rounded-xl bg-gradient-to-r from-primary to-primary/85 px-6 py-10 text-primary-foreground shadow-md sm:px-10">
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
      </Container>
    </div>
  );
}
