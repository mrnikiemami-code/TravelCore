import { Container, LtrValue, Stack, Text } from "@/components/ui";
import type { TourDetailPageViewModel } from "./load-tour-detail";

/**
 * Server-only public TourProduct catalog detail (TC-P09-T008/T010).
 * Published ≠ bookable. App-proxy media only. Cover + ordered Gallery (no hero role).
 */
export function TourDetailView({ vm }: { vm: TourDetailPageViewModel }) {
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
            <Text role="caption">
              {vm.kind} · <LtrValue>{vm.code}</LtrValue> ·{" "}
              <LtrValue>{vm.slug}</LtrValue>
            </Text>
            {vm.description ? <Text as="p">{vm.description}</Text> : null}
          </Stack>

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
        </Stack>
      </Container>
    </div>
  );
}
