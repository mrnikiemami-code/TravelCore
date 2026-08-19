import type { UgcTravelogueView } from "@/features/public-experience/load-ugc-composition";
import type { AppLocale } from "@/lib/i18n";

const faTravelogue: UgcTravelogueView = {
  travelogueId: "fixture-tlg-001",
  actorId: "actor-traveler-42",
  localeCode: "fa",
  title: "سه روز در استانبول — روایت شخصی",
  body:
    "روز اول: گشت در منطقه تاریخی. روز دوم: سفر به آسیایی. این متن نمونه UIVAL است و Article تحریری نیست.",
  comments: [
    {
      commentId: "c1",
      actorId: "actor-reader-7",
      body: "ممنون از اشتراک تجربه!",
      createdAt: "2026-08-01T10:00:00Z",
    },
  ],
  createdAt: "2026-07-28T08:00:00Z",
};

const enTravelogue: UgcTravelogueView = {
  travelogueId: "fixture-tlg-001",
  actorId: "actor-traveler-42",
  localeCode: "en",
  title: "Three Days in Istanbul — A Personal Story",
  body:
    "Day one: historic peninsula. Day two: Asian side ferry. UIVAL sample — not an editorial Article.",
  comments: [
    {
      commentId: "c1",
      actorId: "actor-reader-7",
      body: "Thanks for sharing!",
      createdAt: "2026-08-01T10:00:00Z",
    },
  ],
  createdAt: "2026-07-28T08:00:00Z",
};

export function loadTravelogueDetailFixture(locale: AppLocale): UgcTravelogueView {
  return locale === "fa" ? faTravelogue : enTravelogue;
}
