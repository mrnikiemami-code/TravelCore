import { LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { ExperiencePresentationView } from "@/features/tour-detail/load-tour-detail";

/**
 * Experience-kind sections composed into the shared public Detail shell (P14-R4).
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

  return (
    <Stack gap="lg">
      <Stack gap="sm">
        <Text as="h2" role="heading">
          {locale === "fa" ? "برنامه روزبه‌روز" : "Itinerary"}
        </Text>
        {!experience || experience.itineraryDays.length === 0 ? (
          <Text role="muted">{empty}</Text>
        ) : (
          <ul className="flex flex-col gap-3">
            {experience.itineraryDays.map((day) => (
              <li key={day.dayNumber} className="rounded-md border border-border p-3 text-sm">
                <Stack gap="sm">
                  <Text role="label">
                    {locale === "fa" ? "روز" : "Day"} {day.dayNumber}
                  </Text>
                  {day.stops.length > 0 ? (
                    <ul className="list-inside list-disc">
                      {day.stops.map((stop) => (
                        <li key={`${day.dayNumber}-${stop.sortOrder}`}>
                          <LtrValue>
                            #{stop.sortOrder}
                            {stop.destinationId ? ` · dest ${stop.destinationId}` : ""}
                            {stop.placeId ? ` · place ${stop.placeId}` : ""}
                          </LtrValue>
                        </li>
                      ))}
                    </ul>
                  ) : null}
                  {day.meals.length > 0 ? (
                    <Text>
                      {locale === "fa" ? "وعده‌ها" : "Meals"}: {day.meals.join(" · ")}
                    </Text>
                  ) : null}
                </Stack>
              </li>
            ))}
          </ul>
        )}
      </Stack>

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
    <Stack gap="sm">
      <Text as="h2" role="heading">
        {title}
      </Text>
      {lines.length === 0 ? (
        <Text role="muted">{empty}</Text>
      ) : (
        <ul className="list-inside list-disc">
          {lines.map((line) => (
            <li key={line}>
              <LtrValue>{line}</LtrValue>
            </li>
          ))}
        </ul>
      )}
    </Stack>
  );
}
