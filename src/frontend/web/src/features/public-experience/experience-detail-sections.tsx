import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { ExperiencePresentationView } from "@/features/tour-detail/load-tour-detail";

/**
 * Experience-kind sections composed into the shared public Detail shell (P14-R4 · TC-P31-T005 polish).
 * Tour remains owner of facts. Not a second Detail page. Not Package specialty.
 */
export function ExperienceTourDetailSections({
  locale,
  experience,
}: {
  locale: AppLocale;
  experience: ExperiencePresentationView | null;
}) {
  const empty = locale === "fa" ? "ثبت نشده است." : "None published.";
  const dayLabel = locale === "fa" ? "روز" : locale === "ar" ? "يوم" : "Day";
  const mealsLabel =
    locale === "fa" ? "وعده‌ها" : locale === "ar" ? "الوجبات" : "Meals";
  const itineraryTitle =
    locale === "fa"
      ? "برنامه روزبه‌روز"
      : locale === "ar"
        ? "البرنامج اليومي"
        : "Itinerary";

  return (
    <Stack gap="lg">
      <section aria-labelledby="tour-itinerary-title">
        <h2
          id="tour-itinerary-title"
          className="mb-3 text-lg font-semibold tracking-tight text-foreground"
        >
          {itineraryTitle}
        </h2>
        {!experience || experience.itineraryDays.length === 0 ? (
          <Surface>
            <Text role="muted">{empty}</Text>
          </Surface>
        ) : (
          <ol className="flex flex-col gap-3">
            {experience.itineraryDays.map((day) => (
              <li key={day.dayNumber}>
                <Surface>
                  <Stack gap="sm">
                    <Text role="label" className="text-primary">
                      {dayLabel} {day.dayNumber}
                    </Text>
                    {day.stops.length > 0 ? (
                      <ul className="flex flex-wrap gap-2 text-sm">
                        {day.stops.map((stop) => (
                          <li
                            key={`${day.dayNumber}-${stop.sortOrder}`}
                            className="rounded-full border border-border bg-background px-3 py-1"
                          >
                            <LtrValue>
                              #{stop.sortOrder}
                              {stop.destinationId
                                ? ` · dest ${stop.destinationId}`
                                : ""}
                              {stop.placeId ? ` · place ${stop.placeId}` : ""}
                            </LtrValue>
                          </li>
                        ))}
                      </ul>
                    ) : null}
                    {day.meals.length > 0 ? (
                      <Text role="caption">
                        {mealsLabel}: {day.meals.join(" · ")}
                      </Text>
                    ) : null}
                  </Stack>
                </Surface>
              </li>
            ))}
          </ol>
        )}
      </section>

      <div className="grid gap-4 lg:grid-cols-2">
        <FactBlock
          title={locale === "fa" ? "سختی" : "Difficulty"}
          empty={empty}
          lines={experience?.difficulty ? [experience.difficulty] : []}
        />
        <FactBlock
          title={locale === "fa" ? "شرایط شرکت" : "Eligibility"}
          empty={empty}
          lines={(experience?.eligibility ?? []).map((item) =>
            [item.code, item.value, item.detail].filter(Boolean).join(" · "),
          )}
        />
        <FactBlock
          title={locale === "fa" ? "تجهیزات" : "Equipment"}
          empty={empty}
          lines={(experience?.equipment ?? []).map((item) =>
            [item.code, item.kind, item.detail].filter(Boolean).join(" · "),
          )}
        />
        <FactBlock
          title={locale === "fa" ? "حمل‌ونقل محلی" : "Local transport"}
          empty={empty}
          lines={(experience?.localTransport ?? []).map((item) =>
            [item.code, item.detail].filter(Boolean).join(" · "),
          )}
        />
        <FactBlock
          title={locale === "fa" ? "راهنما" : "Guide"}
          empty={empty}
          lines={(experience?.guides ?? []).map((item) =>
            [item.role, item.guidePartyId, item.note].filter(Boolean).join(" · "),
          )}
        />
        <FactBlock
          title={locale === "fa" ? "برنامه اقامت" : "Accommodation plan"}
          empty={empty}
          lines={(experience?.accommodationPlan ?? []).map((item) =>
            `n${item.sortOrder}${item.placeId ? ` · place ${item.placeId}` : ""}`,
          )}
        />
      </div>
    </Stack>
  );
}

function FactBlock({
  title,
  empty,
  lines,
}: {
  title: string;
  empty: string;
  lines: string[];
}) {
  return (
    <Surface>
      <Stack gap="sm">
        <Text as="h2" role="heading">
          {title}
        </Text>
        {lines.length === 0 ? (
          <Text role="muted">{empty}</Text>
        ) : (
          <ul className="flex flex-wrap gap-2 text-sm">
            {lines.map((line) => (
              <li
                key={line}
                className="rounded-full border border-border bg-background px-3 py-1"
              >
                {line}
              </li>
            ))}
          </ul>
        )}
      </Stack>
    </Surface>
  );
}
