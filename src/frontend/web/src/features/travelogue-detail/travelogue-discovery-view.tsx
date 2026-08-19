import Link from "next/link";
import { Container, LtrValue, Stack, Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { UgcTravelogueView } from "@/features/public-experience/load-ugc-composition";

/**
 * Discovery index for eligible travelogues — not Search, not editorial CMS.
 */
export function TravelogueDiscoveryView({
  locale,
  travelogues,
}: {
  locale: AppLocale;
  travelogues: UgcTravelogueView[];
}) {
  const title =
    locale === "fa" ? "سفرنامه‌ها" : "Travelogues";

  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {title}
            </Text>
            <Text role="caption">
              {locale === "fa"
                ? "کشف UGC · نه Article تحریری · نه موتور جستجو"
                : "UGC discovery · not editorial Article · not a search engine"}
            </Text>
          </Stack>

          {travelogues.length === 0 ? (
            <Text role="muted">
              {locale === "fa"
                ? "سفرنامه واجدشرایطی برای نمایش نیست."
                : "No eligible travelogues to display."}
            </Text>
          ) : (
            <ul className="flex flex-col gap-3">
              {travelogues.map((item) => (
                <li key={item.travelogueId}>
                  <Surface>
                    <Stack gap="sm">
                      <Link
                        href={`/${locale}/travelogues/${encodeURIComponent(item.travelogueId)}`}
                        className="min-h-touch inline-flex underline-offset-2 hover:underline"
                      >
                        <Text role="label">{item.title}</Text>
                      </Link>
                      <Text role="muted">{item.body.slice(0, 200)}</Text>
                      <Text role="caption">
                        <LtrValue>{item.travelogueId}</LtrValue>
                      </Text>
                    </Stack>
                  </Surface>
                </li>
              ))}
            </ul>
          )}
        </Stack>
      </Container>
    </div>
  );
}
