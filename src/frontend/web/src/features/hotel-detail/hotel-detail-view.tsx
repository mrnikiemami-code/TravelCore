import Link from "next/link";
import {
  Container,
  LtrValue,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";
import { UgcCompositionList } from "@/features/public-experience/ugc-composition-list";
import type { PlaceDetailPageViewModel } from "@/types/pages/place-detail";

/**
 * Hotel commerce detail experience (TC-P30-T006).
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
            "بخش نظرات آماده است. تا UGC واقعی، امتیاز یا نظر جعلی نشان داده نمی‌شود.",
          similar: "هتل‌های مشابه",
          similarEmpty: "هتل مشابه دیگری در کاتالوگ این locale نیست.",
          cta: "ادامه به رزرو (ورود آینده)",
          ctaNote: "CTA نمایشی برای مسیر رزرو آینده · نه پرداخت و نه موجودی زنده",
          stars: "ستاره",
          noGallery: "گالری رسانه هنوز برای این هتل منتشر نشده است.",
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
              "قسم المراجعات جاهز. لا نعرض تقييمات أو تعليقات وهمية.",
            similar: "فنادق مشابهة",
            similarEmpty: "لا فنادق مشابهة في هذا الكتالوج.",
            cta: "متابعة الحجز (دخول مستقبلي)",
            ctaNote: "زر عرضي لمسار الحجز المستقبلي · ليس دفعاً أو توفراً حياً",
            stars: "نجوم",
            noGallery: "معرض الوسائط غير منشور بعد لهذا الفندق.",
          }
        : {
            summary: "Hotel summary",
            gallery: "Gallery",
            facilities: "Facilities",
            location: "Location",
            destination: "Destination",
            reviews: "Reviews & stories",
            reviewsEmpty:
              "Reviews section is ready. We do not invent ratings or fake reviews.",
            similar: "Similar hotels",
            similarEmpty: "No other hotels in this locale catalog yet.",
            cta: "Continue to booking (future entry)",
            ctaNote:
              "Presentation CTA for future booking entry · not payment or live availability",
            stars: "stars",
            noGallery: "Media gallery is not published for this hotel yet.",
          };

  const galleryItems =
    vm.gallery.length > 0
      ? vm.gallery
      : vm.cover
        ? [vm.cover]
        : [];

  return (
    <div className="pb-28 pt-6 sm:pt-8">
      <Container width="content">
        <Stack gap="lg">
          {/* Gallery pattern */}
          <section aria-labelledby="hotel-gallery-title">
            <h2 id="hotel-gallery-title" className="mb-3 text-lg font-semibold tracking-tight text-foreground">
              {copy.gallery}
            </h2>
            {galleryItems.length > 0 ? (
              <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                {galleryItems.map((item) =>
                  item.src ? (
                    <li key={item.mediaAssetId}>
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img
                        src={item.src}
                        alt={item.alt || vm.name}
                        width={item.width ?? 960}
                        height={item.height ?? 540}
                        className="aspect-video w-full rounded-xl object-cover"
                      />
                    </li>
                  ) : null,
                )}
              </ul>
            ) : (
              <div className="flex aspect-video items-center justify-center rounded-xl border border-dashed border-border bg-muted/30 p-6">
                <Text role="muted">{copy.noGallery}</Text>
              </div>
            )}
          </section>

          {/* Summary */}
          <section aria-labelledby="hotel-summary-title">
            <Stack gap="sm">
              <h1 id="hotel-summary-title" className="text-2xl font-semibold tracking-tight text-foreground sm:text-3xl">
                {vm.name}
              </h1>
              <Text role="muted">
                {copy.summary}
                {vm.hotelStarRating != null ? (
                  <>
                    {" · "}
                    {vm.hotelStarRating} {copy.stars}
                  </>
                ) : null}
                {" · "}
                <LtrValue>{vm.code}</LtrValue>
              </Text>
              {vm.description ? <Text as="p">{vm.description}</Text> : null}
            </Stack>
          </section>

          {/* Facilities */}
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

          {/* Location */}
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

          {/* Reviews pattern */}
          <section aria-labelledby="hotel-reviews-title">
            <h2 id="hotel-reviews-title" className="mb-3 text-lg font-semibold tracking-tight text-foreground">
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

          {/* Similar hotels pattern */}
          <section aria-labelledby="hotel-similar-title">
            <h2 id="hotel-similar-title" className="mb-3 text-lg font-semibold tracking-tight text-foreground">
              {copy.similar}
            </h2>
            {similarHotels.length === 0 ? (
              <Surface>
                <Text role="muted">{copy.similarEmpty}</Text>
              </Surface>
            ) : (
              <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
                {similarHotels.map((h) => (
                  <li key={h.placeId}>
                    <Surface>
                      <Stack gap="sm">
                        <Link
                          href={`/${locale}/hotels/${encodeURIComponent(h.slug)}`}
                          className="min-h-touch font-medium underline-offset-2 hover:underline"
                        >
                          {h.name}
                        </Link>
                        {h.starRating != null ? (
                          <Text role="caption">
                            {h.starRating} {copy.stars}
                          </Text>
                        ) : null}
                      </Stack>
                    </Surface>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </Stack>
      </Container>

      {/* Booking CTA placement — future entry only */}
      <div className="pointer-events-none fixed inset-x-0 bottom-0 z-40 p-3">
        <div className="pointer-events-auto mx-auto max-w-3xl rounded-xl border border-border bg-background/95 p-3 shadow-lg backdrop-blur">
          <Text role="caption">{copy.ctaNote}</Text>
          <Link
            href={bookHref}
            className="mt-2 inline-flex min-h-touch w-full items-center justify-center rounded-md border border-border bg-surface px-4 py-2 text-sm font-medium underline-offset-2 hover:underline"
          >
            {copy.cta}
          </Link>
        </div>
      </div>
    </div>
  );
}
