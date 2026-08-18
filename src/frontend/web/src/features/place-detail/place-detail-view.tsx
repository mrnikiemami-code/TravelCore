import Link from "next/link";
import {
  Container,
  LtrValue,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import type { PlaceDetailPageViewModel } from "@/types/pages/place-detail";
import { UgcCompositionList } from "@/features/public-experience/ugc-composition-list";

function kindLabel(kind: string, locale: string): string {
  if (locale === "fa") {
    switch (kind) {
      case "Hotel":
        return "هتل";
      case "Restaurant":
        return "رستوران";
      case "Attraction":
        return "جاذبه";
      default:
        return kind;
    }
  }
  return kind;
}

/**
 * Server-only public Place catalog detail (TC-P07-T007).
 * Catalog ≠ booking. App-proxy media only. Cover + ordered Gallery (no hero role).
 */
export function PlaceDetailView({ vm }: { vm: PlaceDetailPageViewModel }) {
  const locale = vm.locale;

  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          {vm.cover?.src ? (
            // eslint-disable-next-line @next/next/no-img-element -- app-proxy public media
            <img
              src={vm.cover.src}
              alt={vm.cover.alt || vm.name}
              width={vm.cover.width ?? 960}
              height={vm.cover.height ?? 540}
              className="aspect-video w-full rounded-lg object-cover"
            />
          ) : null}

          <Stack gap="sm">
            <Text as="h1" role="heading">
              {vm.name}
            </Text>
            <Text role="muted">
              {kindLabel(vm.kind, locale)} · <LtrValue>{vm.code}</LtrValue>
              {vm.hotelStarRating != null ? (
                <>
                  {" · "}
                  {locale === "fa"
                    ? `${vm.hotelStarRating} ستاره`
                    : `${vm.hotelStarRating}-star`}
                </>
              ) : null}
              {vm.restaurantCuisineType ? (
                <>
                  {" · "}
                  {vm.restaurantCuisineType}
                </>
              ) : null}
              {vm.attractionCategoryCode ? (
                <>
                  {" · "}
                  <LtrValue>{vm.attractionCategoryCode}</LtrValue>
                </>
              ) : null}
            </Text>
            {vm.description ? <Text as="p">{vm.description}</Text> : null}
            {vm.kind === "Hotel" ? (
              <a
                className="min-h-touch inline-flex items-center underline underline-offset-2"
                href={`/${locale}/places/${encodeURIComponent(vm.slug)}/book`}
              >
                {locale === "fa"
                  ? "رزرو این هتل"
                  : locale === "ar"
                    ? "احجز هذا الفندق"
                    : "Book this hotel"}
              </a>
            ) : null}
          </Stack>

          {vm.destination ? (
            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {locale === "fa" ? "مقصد" : "Destination"}
                </Text>
                {vm.destination.slug ? (
                  <Link
                    href={`/${locale}/destinations/${encodeURIComponent(vm.destination.slug)}`}
                    className="min-h-touch inline-flex items-center underline-offset-2 hover:underline"
                  >
                    {vm.destination.name}
                  </Link>
                ) : (
                  <Text as="p">{vm.destination.name}</Text>
                )}
              </Stack>
            </Surface>
          ) : null}

          {vm.addressLine || vm.latitude != null ? (
            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {locale === "fa" ? "موقعیت" : "Location"}
                </Text>
                {vm.addressLine ? <Text as="p">{vm.addressLine}</Text> : null}
                {vm.latitude != null && vm.longitude != null ? (
                  <Text role="caption">
                    <LtrValue>
                      {vm.latitude}, {vm.longitude}
                    </LtrValue>
                  </Text>
                ) : null}
              </Stack>
            </Surface>
          ) : null}

          {vm.classificationCode || vm.facilities.length > 0 ? (
            <Surface>
              <Stack gap="sm">
                <Text as="h2" role="heading">
                  {locale === "fa" ? "طبقه‌بندی و امکانات" : "Classification & facilities"}
                </Text>
                {vm.classificationCode ? (
                  <Text role="caption">
                    <LtrValue>{vm.classificationCode}</LtrValue>
                  </Text>
                ) : null}
                {vm.facilities.length > 0 ? (
                  <ul className="flex flex-wrap gap-2 text-sm">
                    {vm.facilities.map((f) => (
                      <li key={f}>
                        <LtrValue>{f}</LtrValue>
                      </li>
                    ))}
                  </ul>
                ) : null}
              </Stack>
            </Surface>
          ) : null}

          {vm.gallery.length > 0 ? (
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {locale === "fa" ? "گالری" : "Gallery"}
              </Text>
              <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                {vm.gallery.map((item) =>
                  item.src ? (
                    <li key={item.mediaAssetId}>
                      {/* eslint-disable-next-line @next/next/no-img-element */}
                      <img
                        src={item.src}
                        alt={item.alt || vm.name}
                        width={item.width ?? 640}
                        height={item.height ?? 360}
                        className="aspect-video w-full rounded-md object-cover"
                      />
                    </li>
                  ) : null,
                )}
              </ul>
            </Stack>
          ) : null}

          <UgcCompositionList locale={locale} composition={vm.ugcComposition} />
        </Stack>
      </Container>
    </div>
  );
}
