import Link from "next/link";
import {
  Container,
  LtrValue,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import { HotelCard } from "@/features/hotel-discovery/hotel-card";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";
import { UgcCompositionList } from "@/features/public-experience/ugc-composition-list";
import type { PlaceDetailPageViewModel } from "@/types/pages/place-detail";

/**
 * Hotel commerce detail experience (TC-P30-T006 · TC-P31-T004 polish).
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
          summary: "خلاصه هتل",
          gallery: "گالری",
          facilities: "امکانات",
          location: "موقعیت",
          destination: "مقصد",
          reviews: "نظرات و داستان‌ها",
          reviewsEmpty:
            "هنوز نظری ثبت نشده است. امتیاز یا نظر ساختگی نشان داده نمی‌شود.",
          similar: "هتل‌های مشابه",
          similarEmpty: "فعلاً هتل مشابه دیگری در فهرست نیست.",
          trustTitle: "اعتماد در مسیر اقامت",
          trustBody:
            "این صفحه کاتالوگ Place است — نرخ اتاق و موجودی فقط وقتی HotelBooking فعال باشد.",
          cta: "ادامه به رزرو",
          ctaNote: "ورود به مسیر رزرو · موجودی و پرداخت در مرحله بعد",
          stars: "ستاره",
          noGallery: "گالری تصاویر هنوز برای این هتل منتشر نشده است.",
          demoHint: "نمونه DEMOFEED",
        }
      : locale === "ar"
        ? {
            summary: "ملخص الفندق",
            gallery: "المعرض",
            facilities: "المرافق",
            location: "الموقع",
            destination: "الوجهة",
            reviews: "المراجعات والقصص",
            reviewsEmpty:
              "لا مراجعات بعد. لا نعرض تقييمات أو تعليقات وهمية.",
            similar: "فنادق مشابهة",
            similarEmpty: "لا فنادق مشابهة في القائمة حالياً.",
            trustTitle: "الثقة في مسار الإقامة",
            trustBody:
              "هذه صفحة كتالوج Place — أسعار الغرف والتوفر فقط عند تفعيل HotelBooking.",
            cta: "متابعة الحجز",
            ctaNote: "الدخول إلى مسار الحجز · التوفر والدفع لاحقاً",
            stars: "نجوم",
            noGallery: "معرض الصور غير منشور بعد لهذا الفندق.",
            demoHint: "عينة DEMOFEED",
          }
        : {
            summary: "Hotel summary",
            gallery: "Gallery",
            facilities: "Facilities",
            location: "Location",
            destination: "Destination",
            reviews: "Reviews & stories",
            reviewsEmpty:
              "No reviews yet. We do not invent ratings or fake comments.",
            similar: "Similar hotels",
            similarEmpty: "No other hotels in this list yet.",
            trustTitle: "Trust on the stay path",
            trustBody:
              "This is a Place catalog surface — room rates and availability only when HotelBooking is live.",
            cta: "Continue to booking",
            ctaNote: "Enter the booking path · availability and payment come later",
            stars: "stars",
            noGallery: "Photo gallery is not published for this hotel yet.",
            demoHint: "DEMOFEED sample",
          };

  const galleryItems =
    vm.gallery.length > 0 ? vm.gallery : vm.cover ? [vm.cover] : [];
  const hero = galleryItems[0] ?? null;
  const thumbs = galleryItems.slice(1, 5);
  const isDemo = vm.slug.startsWith("demofeed-") || vm.code.startsWith("demofeed-");

  return (
    <div className="pb-28">
      <section className="border-b border-border bg-gradient-to-br from-primary/95 via-primary to-primary/80 text-primary-foreground">
        <Container width="wide" className="py-6 sm:py-8">
          <p className="text-xs font-semibold uppercase tracking-[0.18em] text-accent">
            Hotel commerce
          </p>
          <h1 className="mt-2 text-2xl font-semibold tracking-tight sm:text-4xl">
            {vm.name}
          </h1>
          <p className="mt-2 text-sm text-primary-foreground/90">
            {copy.summary}
            {vm.hotelStarRating != null ? (
              <>
                {" · "}
                {vm.hotelStarRating} {copy.stars}
              </>
            ) : null}
            {" · "}
            <LtrValue>{vm.code}</LtrValue>
            {isDemo ? (
              <>
                {" · "}
                {copy.demoHint}
              </>
            ) : null}
          </p>
        </Container>
      </section>

      <Container width="wide" className="pt-6 sm:pt-8">
        <Stack gap="lg">
          <section aria-labelledby="hotel-gallery-title">
            <h2
              id="hotel-gallery-title"
              className="mb-3 text-lg font-semibold tracking-tight text-foreground"
            >
              {copy.gallery}
            </h2>
            {hero?.src ? (
              <div className="grid gap-3 lg:grid-cols-[1.6fr_0.8fr]">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={hero.src}
                  alt={hero.alt || vm.name}
                  width={hero.width ?? 1200}
                  height={hero.height ?? 675}
                  className="aspect-[16/10] w-full rounded-2xl object-cover shadow-sm"
                />
                <ul className="grid grid-cols-2 gap-3">
                  {thumbs.length > 0
                    ? thumbs.map((item) =>
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
                      )
                    : [0, 1, 2, 3].map((i) => (
                        <li
                          key={i}
                          aria-hidden
                          className="aspect-video rounded-xl bg-gradient-to-br from-surface-muted to-primary/20"
                        />
                      ))}
                </ul>
              </div>
            ) : (
              <div className="flex aspect-[16/9] items-center justify-center rounded-2xl border border-dashed border-border bg-muted/30 p-6">
                <Text role="muted">{copy.noGallery}</Text>
              </div>
            )}
          </section>

          <section aria-labelledby="hotel-summary-title">
            <Stack gap="sm">
              <h2
                id="hotel-summary-title"
                className="text-xl font-semibold tracking-tight text-foreground"
              >
                {copy.summary}
              </h2>
              {vm.description ? <Text as="p">{vm.description}</Text> : null}
            </Stack>
          </section>

          <div className="grid gap-4 lg:grid-cols-2">
            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {copy.facilities}
                </Text>
                {vm.classificationCode ? (
                  <Text role="caption">
                    <LtrValue>{vm.classificationCode}</LtrValue>
                  </Text>
                ) : null}
                {vm.facilities.length > 0 ? (
                  <ul className="flex flex-wrap gap-2 text-sm">
                    {vm.facilities.map((f) => (
                      <li
                        key={f}
                        className="rounded-full border border-border bg-background px-3 py-1"
                      >
                        <LtrValue>{f}</LtrValue>
                      </li>
                    ))}
                  </ul>
                ) : (
                  <Text role="muted">
                    {locale === "fa"
                      ? "امکانات کاتالوگ هنوز ثبت نشده است."
                      : "Catalog facilities are not listed yet."}
                  </Text>
                )}
              </Stack>
            </Surface>

            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {copy.location}
                </Text>
                {vm.destination ? (
                  <Text as="p">
                    {copy.destination}:{" "}
                    {vm.destination.slug ? (
                      <Link
                        href={`/${locale}/destinations/${encodeURIComponent(vm.destination.slug)}`}
                        className="min-h-touch underline-offset-2 hover:underline"
                      >
                        {vm.destination.name}
                      </Link>
                    ) : (
                      vm.destination.name
                    )}
                  </Text>
                ) : null}
                {vm.addressLine ? <Text as="p">{vm.addressLine}</Text> : null}
                {vm.latitude != null && vm.longitude != null ? (
                  <Text role="caption">
                    <LtrValue>
                      {vm.latitude}, {vm.longitude}
                    </LtrValue>
                  </Text>
                ) : null}
                {!vm.destination && !vm.addressLine && vm.latitude == null ? (
                  <Text role="muted">
                    {locale === "fa"
                      ? "موقعیت کاتالوگ هنوز کامل نشده است."
                      : "Catalog location is not complete yet."}
                  </Text>
                ) : null}
              </Stack>
            </Surface>
          </div>

          <Surface className="border-primary/15 bg-gradient-to-br from-surface to-primary/5">
            <Text as="h2" role="heading" className="text-primary">
              {copy.trustTitle}
            </Text>
            <Text role="muted" className="mt-2">
              {copy.trustBody}
            </Text>
          </Surface>

          <section aria-labelledby="hotel-reviews-title">
            <h2
              id="hotel-reviews-title"
              className="mb-3 text-lg font-semibold tracking-tight text-foreground"
            >
              {copy.reviews}
            </h2>
            {vm.ugcComposition.reviews.length > 0 ||
            vm.ugcComposition.travelogues.length > 0 ||
            vm.ugcComposition.userPhotos.length > 0 ? (
              <UgcCompositionList locale={locale} composition={vm.ugcComposition} />
            ) : (
              <Surface>
                <Text role="muted">{copy.reviewsEmpty}</Text>
              </Surface>
            )}
          </section>

          <section aria-labelledby="hotel-similar-title">
            <h2
              id="hotel-similar-title"
              className="mb-3 text-lg font-semibold tracking-tight text-foreground"
            >
              {copy.similar}
            </h2>
            {similarHotels.length === 0 ? (
              <Surface>
                <Text role="muted">{copy.similarEmpty}</Text>
              </Surface>
            ) : (
              <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                {similarHotels.map((h) => (
                  <li key={h.placeId}>
                    <HotelCard locale={locale} hotel={h} />
                  </li>
                ))}
              </ul>
            )}
          </section>
        </Stack>
      </Container>

      <div className="pointer-events-none fixed inset-x-0 bottom-0 z-40 p-3">
        <div className="pointer-events-auto mx-auto flex max-w-3xl flex-col gap-2 rounded-xl border border-border bg-background/95 p-3 shadow-lg backdrop-blur sm:flex-row sm:items-center sm:justify-between">
          <Text role="caption">{copy.ctaNote}</Text>
          <Link
            href={bookHref}
            className="inline-flex min-h-touch items-center justify-center rounded-md bg-accent px-4 py-2 text-sm font-semibold text-accent-foreground hover:opacity-95 sm:min-w-44"
          >
            {copy.cta}
          </Link>
        </div>
      </div>
    </div>
  );
}
