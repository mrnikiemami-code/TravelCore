import { LtrValue, Stack, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";
import type { UgcCompositionView } from "./load-ugc-composition";

/**
 * P16-R8: PublicExperience composes eligible UGC facts only.
 * UGC remains fact owner. Not SEO pages, not Search ranking, not likes.
 */
export function UgcCompositionList({
  locale,
  composition,
}: {
  locale: AppLocale;
  composition: UgcCompositionView;
}) {
  const { summary, reviews, travelogues, userPhotos } = composition;
  const hasSummary = summary.eligibleReviewCount > 0;

  return (
    <Stack gap="lg">
      <Stack gap="sm">
        <Text as="h2" role="heading">
          {locale === "fa" ? "نظرهای مسافران" : "Traveler reviews"}
        </Text>
        <Text role="caption">
          {locale === "fa"
            ? "نمایش واجدشرایط · مالکیت با UGC می‌ماند · ایندکس SEO نیست"
            : "Eligible display only · UGC keeps ownership · not an SEO index"}
        </Text>
        {hasSummary ? (
          <Text>
            {locale === "fa"
              ? `میانگین مشتق‌شده ${summary.averageOverallRating} از ${summary.eligibleReviewCount} نظر`
              : `Derived average ${summary.averageOverallRating} from ${summary.eligibleReviewCount} reviews`}
          </Text>
        ) : (
          <Text role="muted">
            {locale === "fa"
              ? "نظر واجدشرایطی برای نمایش نیست."
              : "No eligible reviews to display."}
          </Text>
        )}
        {reviews.length > 0 ? (
          <ul className="flex flex-col gap-3">
            {reviews.map((review) => (
              <li
                key={review.reviewId}
                className="rounded-md border border-border p-3 text-sm"
              >
                <Stack gap="sm">
                  <Text>
                    {review.title ||
                      (locale === "fa" ? "نظر مسافر" : "Traveler review")}{" "}
                    · {review.overallRating}/5
                  </Text>
                  {review.body ? <Text>{review.body}</Text> : null}
                  {review.dimensionRatings.length > 0 ? (
                    <Text role="caption">
                      {review.dimensionRatings
                        .map((item) => `${item.dimensionCode} ${item.value}`)
                        .join(" · ")}
                    </Text>
                  ) : null}
                  <Text role="caption">
                    <LtrValue>{review.actorId}</LtrValue>
                  </Text>
                  {review.comments.length > 0 ? (
                    <ul className="flex flex-col gap-2">
                      {review.comments.map((comment) => (
                        <li key={comment.commentId}>
                          <Text role="muted">{comment.body}</Text>
                        </li>
                      ))}
                    </ul>
                  ) : null}
                </Stack>
              </li>
            ))}
          </ul>
        ) : null}
      </Stack>

      <Stack gap="sm">
        <Text as="h2" role="heading">
          {locale === "fa" ? "سفرنامه‌ها" : "Travelogues"}
        </Text>
        <Text role="caption">
          {locale === "fa"
            ? "روایت کاربرساخت · Article تحریری نیست"
            : "User-authored narrative · not an editorial Article"}
        </Text>
        {travelogues.length === 0 ? (
          <Text role="muted">
            {locale === "fa"
              ? "سفرنامه واجدشرایطی نیست."
              : "No eligible travelogues."}
          </Text>
        ) : (
          <ul className="flex flex-col gap-3">
            {travelogues.map((item) => (
              <li
                key={item.travelogueId}
                className="rounded-md border border-border p-3 text-sm"
              >
                <Stack gap="sm">
                  <Text>{item.title}</Text>
                  <Text role="muted">{item.body}</Text>
                  {item.comments.length > 0 ? (
                    <ul className="flex flex-col gap-2">
                      {item.comments.map((comment) => (
                        <li key={comment.commentId}>
                          <Text role="caption">{comment.body}</Text>
                        </li>
                      ))}
                    </ul>
                  ) : null}
                </Stack>
              </li>
            ))}
          </ul>
        )}
      </Stack>

      <Stack gap="sm">
        <Text as="h2" role="heading">
          {locale === "fa" ? "عکس مسافران" : "Traveler photos"}
        </Text>
        <Text role="caption">
          {locale === "fa"
            ? "رابطه UGC · مالکیت بایت با Media می‌ماند"
            : "UGC relationship · Media keeps asset bytes"}
        </Text>
        {userPhotos.length === 0 ? (
          <Text role="muted">
            {locale === "fa"
              ? "عکس واجدشرایطی نیست."
              : "No eligible traveler photos."}
          </Text>
        ) : (
          <ul className="flex flex-col gap-2">
            {userPhotos.map((item) => (
              <li key={item.userPhotoId}>
                <Text role="caption">
                  <LtrValue>{item.mediaAssetId}</LtrValue>
                </Text>
              </li>
            ))}
          </ul>
        )}
      </Stack>
    </Stack>
  );
}
