import { Container, LtrValue, Stack, Text } from "@/components/ui";
import type { TourDetailPageViewModel } from "./load-tour-detail";

/**
 * Server-only public TourProduct catalog detail (TC-P09-T008/T010 · TC-P11-T009).
 * Catalog Published ≠ bookable. Published executions ≠ bookable (P11-R8).
 * App-proxy media only. Cover + ordered Gallery (no hero role).
 */
export function TourDetailView({ vm }: { vm: TourDetailPageViewModel }) {
  const locale = vm.locale;
  const departuresHeading =
    locale === "fa" ? "اجراهای منتشرشده" : "Published departures";
  const noDepartures =
    locale === "fa"
      ? "فعلاً اجرای منتشرشده‌ای ثبت نشده است."
      : "No published departures yet.";
  const scheduleLabel = locale === "fa" ? "برنامه" : "Schedule";
  const capacityLabel = locale === "fa" ? "ظرفیت برنامه‌ای" : "Planned capacity";
  const transportLabel = locale === "fa" ? "حمل‌ونقل" : "Transport";
  const stayLabel = locale === "fa" ? "اقامت" : "Stay";
  const daysLabel = locale === "fa" ? "روز" : "days";

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

          <Stack gap="sm">
            <Text as="h2" role="heading">
              {departuresHeading}
            </Text>
            <Text role="caption">
              {locale === "fa"
                ? "نمایش اطلاعات اجرایی · بدون موتور فروش"
                : "Execution facts only · not a sales engine"}
            </Text>
            {vm.publishedDepartures.length === 0 ? (
              <Text role="muted">{noDepartures}</Text>
            ) : (
              <ul className="flex flex-col gap-3">
                {vm.publishedDepartures.map((d) => (
                  <li
                    key={d.id}
                    className="rounded-md border border-border p-3 text-sm"
                  >
                    <Stack gap="sm">
                      <LtrValue>
                        <Text role="caption">{d.id}</Text>
                      </LtrValue>
                      <Text>
                        {scheduleLabel}:{" "}
                        <LtrValue>
                          {d.startDate ?? "—"} → {d.endDate ?? "—"}
                          {d.timeZoneId ? ` · ${d.timeZoneId}` : ""}
                        </LtrValue>
                        {d.durationDays != null
                          ? ` · ${d.durationDays} ${daysLabel}`
                          : null}
                      </Text>
                      {(d.minimumPax != null || d.maximumPax != null) && (
                        <Text>
                          {capacityLabel}:{" "}
                          <LtrValue>
                            {d.minimumPax ?? "—"}–{d.maximumPax ?? "—"}
                          </LtrValue>
                        </Text>
                      )}
                      {d.transport.length > 0 ? (
                        <Stack gap="sm">
                          <Text role="label">{transportLabel}</Text>
                          <ul className="list-inside list-disc">
                            {d.transport.map((t) => (
                              <li key={`${d.id}-t-${t.sequence}`}>
                                <LtrValue>
                                  #{t.sequence} {t.transportMode}: {t.origin} →{" "}
                                  {t.destination}
                                </LtrValue>
                              </li>
                            ))}
                          </ul>
                        </Stack>
                      ) : null}
                      {d.accommodation.length > 0 ? (
                        <Stack gap="sm">
                          <Text role="label">{stayLabel}</Text>
                          <ul className="list-inside list-disc">
                            {d.accommodation.map((a) => (
                              <li key={`${d.id}-a-${a.placeId}-${a.nights}`}>
                                <LtrValue>
                                  {a.nights}n · {a.boardType} · place {a.placeId}
                                </LtrValue>
                              </li>
                            ))}
                          </ul>
                        </Stack>
                      ) : null}
                    </Stack>
                  </li>
                ))}
              </ul>
            )}
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
