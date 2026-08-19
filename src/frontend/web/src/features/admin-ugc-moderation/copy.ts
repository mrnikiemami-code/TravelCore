import type { AppLocale } from "@/lib/i18n";

export type AdminUgcModerationCopy = {
  pageTitle: string;
  pageIntro: string;
  navLabel: string;
  backToHub: string;
  catalogLink: string;
  accountsJob: string;
  hubCta: string;
  stepQueue: string;
  refreshQueue: string;
  takeLabel: string;
  noItems: string;
  selectItem: string;
  moderationStatusLabel: string;
  publicationStatusLabel: string;
  localeLabel: string;
  actorLabel: string;
  bodyPreviewLabel: string;
  approveAction: string;
  rejectAction: string;
  publishAction: string;
  apiMissing: string;
  errorPrefix: string;
  busy: string;
  authRequired: string;
};

const FA: AdminUgcModerationCopy = {
  pageTitle: "مدیریت محتوای کاربران",
  pageIntro:
    "صف تأیید سفرنامه‌ها. تأیید، رد یا انتشار فقط از طریق چرخهٔ moderation ماژول UGC انجام می‌شود — نه CMS محتوا.",
  navLabel: "ناوبری مدیریت UGC",
  backToHub: "بازگشت به کاتالوگ",
  catalogLink: "کاتالوگ",
  accountsJob: "حساب‌ها",
  hubCta: "مدیریت UGC",
  stepQueue: "صف سفرنامه‌های در انتظار",
  refreshQueue: "بروزرسانی صف",
  takeLabel: "تعداد",
  noItems: "موردی در صف نیست.",
  selectItem: "یک سفرنامه را انتخاب کنید.",
  moderationStatusLabel: "وضعیت moderation",
  publicationStatusLabel: "وضعیت انتشار",
  localeLabel: "زبان",
  actorLabel: "شناسهٔ actor",
  bodyPreviewLabel: "پیش‌نمایش متن",
  approveAction: "تأیید",
  rejectAction: "رد",
  publishAction: "انتشار",
  apiMissing: "API پیکربندی نشده است.",
  errorPrefix: "خطا:",
  busy: "در حال پردازش…",
  authRequired: "برای این عملیات باید وارد شوید.",
};

const EN: AdminUgcModerationCopy = {
  pageTitle: "UGC moderation",
  pageIntro:
    "Pending travelogue queue. Approve, reject, or publish through the UGC module moderation lifecycle — not the Content CMS.",
  navLabel: "UGC admin navigation",
  backToHub: "Back to catalog hub",
  catalogLink: "Catalog hub",
  accountsJob: "Accounts",
  hubCta: "UGC moderation",
  stepQueue: "Pending travelogue queue",
  refreshQueue: "Refresh queue",
  takeLabel: "Take",
  noItems: "No items in queue.",
  selectItem: "Select a travelogue.",
  moderationStatusLabel: "Moderation status",
  publicationStatusLabel: "Publication status",
  localeLabel: "Locale",
  actorLabel: "Actor id",
  bodyPreviewLabel: "Body preview",
  approveAction: "Approve",
  rejectAction: "Reject",
  publishAction: "Publish",
  apiMissing: "API is not configured.",
  errorPrefix: "Error:",
  busy: "Working…",
  authRequired: "Sign in is required for this action.",
};

export function getAdminUgcModerationCopy(locale: AppLocale): AdminUgcModerationCopy {
  return locale === "fa" ? FA : EN;
}
