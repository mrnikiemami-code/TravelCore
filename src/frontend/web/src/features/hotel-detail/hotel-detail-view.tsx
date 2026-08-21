import Link from "next/link";
import { Container, Stack, Surface, Text } from "@/components/ui";
import { HotelCard } from "@/features/hotel-discovery/hotel-card";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";
import { UgcCompositionList } from "@/features/public-experience/ugc-composition-list";
import type { PlaceDetailPageViewModel } from "@/types/pages/place-detail";

function isNoiseDescription(value: string | null | undefined): boolean {
  if (!value) return true;
  const v = value.trim().toLowerCase();
  return (
    v.length === 0 ||
    v.includes("demofeed sample data") ||
    v === "non-production" ||
    v.includes("undefined") ||
    v.includes("null")
  );
}

/**
 * Hotel commerce detail experience (TC-P36-T003 polish).
 * Place catalog SoR · booking CTA is future entry only · no invented commerce facts.
 */
export function HotelDetailView({
  vm,
  similarHotels,
}: {
  vm: PlaceDetailPageViewModel;
  similarHotels: HotelBrowseItemView[];
}) {
  const locale = vm.locale;
  const bookHref = `/${locale}/hotels/${encodeURIComponent(vm.slug)}/book`;

  const copy =
    locale === "fa"
      ? {
          summary: "درباره اقامتگاه",
          gallery: "گالری",
          facilities: "امکانات",
          location: "موقعیت",
          destination: "مقصد",
          reviews: "نظرات و داستان‌ها",
          similar: "هتل‌های مشابه",
          trustTitle: "مسیر رزرو صادقانه",
          trustBody:
            "این صفحه کاتالوگ Place است — نرخ اتاق و موجودی فقط وقتی HotelBooking فعال باشد.",
          cta: "ادامه به آماده‌سازی رزرو",
          ctaNote: "Pending booking · بدون موجودی یا پرداخت جعلی",
          stars: "ستاره",
          noGallery: "گالری تصاویر هنوز برای این هتل منتشر نشده است.",
          demoHint: "نمونه کاتالوگ",
          back: "بازگشت به هتل‌ها",
        }
      : locale === "ar"
        ? {
            summary: "عن الإقامة",
            gallery: "المعرض",
            facilities: "المرافق",
            location: "الموقع",
            destination: "الوجهة",
            reviews: "المراجعات والقصص",
            similar: "فنادق مشابهة",
            trustTitle: "مسار حجز صادق",
            trustBody:
              "هذه صفحة كتالوج Place — أسعار الغرف والتوفر فقط عند تفعيل HotelBooking.",
            cta: "متابعة تجهيز الحجز",
            ctaNote: "حجز معلّق · دون توفر أو دفع وهمي",
            stars: "نجوم",
            noGallery: "معرض الصور غير منشور بعد لهذا الفندق.",
            demoHint: "عينة الكتالوج",
            back: "العودة إلى الفنادق",
          }
        : {
            summary: "About this stay",
            gallery: "Gallery",
            facilities: "Facilities",
            location: "Location",
            destination: "Destination",
            reviews: "Reviews & stories",
            similar: "Similar hotels",
            trustTitle: "Honest booking path",
            trustBody:
              "This is a Place catalog surface — room rates and availability only when HotelBooking is live.",
            cta: "Continue to booking prep",
            ctaNote: "Pending booking · no fake availability or payment",
            stars: "stars",
            noGallery: "Photo gallery is not published for this hotel yet.",
            demoHint: "Sample catalog",
            back: "Back to hotels",
          };

  const galleryItems =
    vm.gallery.length > 0 ? vm.gallery : vm.cover ? [vm.cover] : [];
  const hero = galleryItems[0] ?? null;
  const thumbs = galleryItems.slice(1, 5);
  const isDemo =
    vm.slug.startsWith("demofeed-") || vm.code.startsWith("demofeed-");
  const showDescription = !isNoiseDescription(vm.description);
  const hasFacilities = vm.facilities.length > 0;
  const hasLocation =
    Boolean(vm.destination) || Boolean(vm.addressLine?.trim());
  const hasUgc =
    vm.ugcComposition.reviews.length > 0 ||
    vm.ugcComposition.travelogues.length > 0 ||
    vm.ugcComposition.userPhotos.length > 0;

  return (
    <div className="pb-28">
      <section className="relative isolate overflow-hidden border-b border-border">
        <div className="absolute inset-0 bg-[#0E172A]" aria-hidden />
        {hero?.src ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={hero.src}
            alt=""
            className="absolute inset-0 h-full w-full object-cover opacity-50"
          />
        ) : null}
        <div
          aria-hidden
          className="absolute inset-0 bg-gradient-to-r from-[#0E172A]/95 via-[#0E172A]/78 to-[#0E172A]/45"
        />
        <Container width="wide" className="relative py-8 sm:py-10">
          <Link
            href={`/${locale}/hotels`}
            className="text-xs font-medium text-white/75 underline-offset-2 hover:text-white hover:underline"
          >
            {copy.back}
          </Link>
          <div className="mt-3 flex flex-wrap items-center gap-2">
            {vm.hotelStarRating != null ? (
              <span className="rounded-full bg-white/15 px-2.5 py-1 text-xs font-medium text-white">
                {vm.hotelStarRating} {copy.stars}
              </span>
            ) : null}
            {isDemo ? (
              <span className="rounded-full bg-black/35 px-2.5 py-1 text-[10px] font-medium tracking-wide text-white/90">
                {copy.demoHint}
              </span>
            ) : null}
          </div>
          <h1 className="mt-3 max-w-3xl text-3xl font-semibold tracking-tight text-white sm:text-4xl">
            {vm.name}
          </h1>
          {vm.destination?.name ? (
            <p className="mt-2 text-sm text-white/85">
              {copy.destination}:{" "}
              {vm.destination.slug ? (
                <Link
                  href={`/${locale}/destinations/${encodeURIComponent(vm.destination.slug)}`}
                  className="underline-offset-2 hover:underline"
                >
                  {vm.destination.name}
                </Link>
              ) : (
                vm.destination.name
              )}
            </p>
          ) : null}
        </Container>
      </section>

      <Container width="wide" className="pt-6 sm:pt-8">
        <Stack gap="xl">
          <section aria-labelledby="hotel-gallery-title">
            <h2
              id="hotel-gallery-title"
              className="mb-3 text-lg font-semibold tracking-tight text-foreground"
            >
              {copy.gallery}
            </h2>
            {hero?.src ? (
              <div className="grid gap-3 lg:grid-cols-[1.7fr_0.8fr]">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={hero.src}
                  alt={hero.alt || vm.name}
                  width={hero.width ?? 1200}
                  height={hero.height ?? 675}
                  className="aspect-[16/10] w-full rounded-2xl object-cover shadow-sm"
                />
                {thumbs.length > 0 ? (
                  <ul className="grid grid-cols-2 gap-3 content-start">
                    {thumbs.map((item) =>
                      item.src ? (
                        <li key={item.mediaAssetId}>
                          {/* eslint-disable-next-line @next/next/no-img-element */}
                          <img
                            src={item.src}
                            alt={item.alt || vm.name}
                            width={item.width ?? 640}
                            height={item.height ?? 360}
                            className="aspect-video w-full rounded-xl object-cover"
                          />
                        </li>
                      ) : null,
                    )}
                  </ul>
                ) : (
                  <Surface className="flex min-h-40 items-center justify-center rounded-2xl border-dashed p-4">
                    <Text role="muted" className="text-center text-sm">
                      {copy.trustBody}
                    </Text>
                  </Surface>
                )}
              </div>
            ) : (
              <div className="flex aspect-[16/9] items-center justify-center rounded-2xl border border-dashed border-border bg-muted/30 p-6">
                <Text role="muted">{copy.noGallery}</Text>
              </div>
            )}
          </section>

          {(showDescription || hasLocation || hasFacilities) && (
            <div className="grid gap-5 lg:grid-cols-[1.4fr_0.8fr]">
              <section aria-labelledby="hotel-summary-title" className="space-y-3">
                <h2
                  id="hotel-summary-title"
                  className="text-xl font-semibold tracking-tight text-foreground"
                >
                  {copy.summary}
                </h2>
                {showDescription ? (
                  <p className="text-sm leading-relaxed text-foreground/90 sm:text-base">
                    {vm.description}
                  </p>
                ) : (
                  <p className="text-sm leading-relaxed text-muted-foreground">
                    {copy.trustBody}
                  </p>
                )}
              </section>

              <div className="space-y-4">
                {hasLocation ? (
                  <Surface className="rounded-2xl p-5">
                    <h2 className="text-sm font-semibold text-[#1D4ED8]">
                      {copy.location}
                    </h2>
                    <div className="mt-2 space-y-1 text-sm text-foreground">
                      {vm.destination ? (
                        <p>
                          {copy.destination}:{" "}
                          {vm.destination.slug ? (
                            <Link
                              href={`/${locale}/destinations/${encodeURIComponent(vm.destination.slug)}`}
                              className="underline-offset-2 hover:underline"
                            >
                              {vm.destination.name}
                            </Link>
                          ) : (
                            vm.destination.name
                          )}
                        </p>
                      ) : null}
                      {vm.addressLine ? <p>{vm.addressLine}</p> : null}
                    </div>
                  </Surface>
                ) : null}

                {hasFacilities ? (
                  <Surface className="rounded-2xl p-5">
                    <h2 className="text-sm font-semibold text-[#1D4ED8]">
                      {copy.facilities}
                    </h2>
                    <ul className="mt-3 flex flex-wrap gap-2 text-sm">
                      {vm.facilities.map((f) => (
                        <li
                          key={f}
                          className="rounded-full border border-border bg-background px-3 py-1"
                        >
                          {f}
                        </li>
                      ))}
                    </ul>
                  </Surface>
                ) : null}
              </div>
            </div>
          )}

          <Surface className="rounded-2xl border-[#1D4ED8]/15 bg-gradient-to-br from-surface to-[#1D4ED8]/[0.04] p-5">
            <p className="text-sm font-semibold text-[#1D4ED8]">
              {copy.trustTitle}
            </p>
            <p className="mt-2 text-sm text-muted-foreground">{copy.trustBody}</p>
          </Surface>

          {hasUgc ? (
            <section aria-labelledby="hotel-reviews-title">
              <h2
                id="hotel-reviews-title"
                className="mb-3 text-lg font-semibold tracking-tight text-foreground"
              >
                {copy.reviews}
              </h2>
              <UgcCompositionList
                locale={locale}
                composition={vm.ugcComposition}
              />
            </section>
          ) : null}

          {similarHotels.length > 0 ? (
            <section aria-labelledby="hotel-similar-title">
              <h2
                id="hotel-similar-title"
                className="mb-4 text-lg font-semibold tracking-tight text-foreground"
              >
                {copy.similar}
              </h2>
              <ul className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
                {similarHotels.map((h) => (
                  <li key={h.placeId}>
                    <HotelCard locale={locale} hotel={h} />
                  </li>
                ))}
              </ul>
            </section>
          ) : null}
        </Stack>
      </Container>

      <div className="pointer-events-none fixed inset-x-0 bottom-0 z-40 p-3">
        <div className="pointer-events-auto mx-auto flex max-w-3xl flex-col gap-3 rounded-2xl border border-border bg-background/95 p-4 shadow-xl backdrop-blur sm:flex-row sm:items-center sm:justify-between">
          <p className="text-sm text-muted-foreground">{copy.ctaNote}</p>
          <Link
            href={bookHref}
            className="inline-flex min-h-touch items-center justify-center rounded-lg bg-[#F59E0B] px-5 py-2 text-sm font-semibold text-[#0E172A] hover:brightness-105 sm:min-w-48"
          >
            {copy.cta}
          </Link>
        </div>
      </div>
    </div>
  );
}
