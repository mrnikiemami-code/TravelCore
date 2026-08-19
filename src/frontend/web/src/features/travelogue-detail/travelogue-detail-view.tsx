import { Container, LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { UgcTravelogueView } from "@/features/public-experience/load-ugc-composition";

/**
 * UIVAL-T009 TraveloguePage presentation (user narrative · not Article).
 */
export function TravelogueDetailView({
  locale,
  travelogue,
}: {
  locale: AppLocale;
  travelogue: UgcTravelogueView;
}) {
  return (
    <div className="py-6 sm:py-8">
      <Container width="narrow">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {travelogue.title}
            </Text>
            <Text role="caption">
              {locale === "fa" ? "سفرنامه" : "Travelogue"} ·{" "}
              <LtrValue>{travelogue.travelogueId}</LtrValue>
            </Text>
            <Text role="muted">
              {locale === "fa"
                ? "روایت کاربر · Article تحریری نیست · SEO quality-gated"
                : "User narrative · not editorial Article · SEO quality-gated"}
            </Text>
          </Stack>

          <Text as="article">{travelogue.body}</Text>

          {travelogue.comments.length > 0 ? (
            <Stack gap="sm">
              <Text as="h2" role="heading">
                {locale === "fa" ? "دیدگاه‌ها" : "Comments"}
              </Text>
              <ul className="flex flex-col gap-2">
                {travelogue.comments.map((comment) => (
                  <li
                    key={comment.commentId}
                    className="rounded-md border border-border p-3 text-sm"
                  >
                    <Text>{comment.body}</Text>
                    <Text role="caption">
                      <LtrValue>{comment.actorId}</LtrValue>
                    </Text>
                  </li>
                ))}
              </ul>
            </Stack>
          ) : null}
        </Stack>
      </Container>
    </div>
  );
}
