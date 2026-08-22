import type { AppLocale } from "@/lib/i18n";

export type AdminAgencyOfferGovernanceCopy = {
  pageTitle: string;
  pageIntro: string;
  navLabel: string;
  backToAgencies: string;
  stepQueue: string;
  refreshQueue: string;
  takeLabel: string;
  noItems: string;
  selectItem: string;
  agencyProfileLabel: string;
  tourProductLabel: string;
  publicationStatusLabel: string;
  visibilityLabel: string;
  salesChannelLabel: string;
  highlightLabel: string;
  approveAction: string;
  rejectAction: string;
  suspendAction: string;
  boundaryNote: string;
  apiMissing: string;
  errorPrefix: string;
  busy: string;
  authRequired: string;
};

const FA: AdminAgencyOfferGovernanceCopy = {
  pageTitle: "حاکمیت Offer آژانس",
  pageIntro:
    "صف Offerهای Submitted برای بررسی Admin. تأیید/رد/تعلیق بدون کمیسیون یا تسویه. Agency ≠ Admin.",
  navLabel: "ناوبری حاکمیت Offer",
  backToAgencies: "بازگشت به مدیریت آژانس",
  stepQueue: "صف Offerهای در انتظار",
  refreshQueue: "بروزرسانی صف",
  takeLabel: "تعداد",
  noItems: "موردی در صف نیست.",
  selectItem: "یک Offer را انتخاب کنید.",
  agencyProfileLabel: "شناسهٔ AgencyProfile",
  tourProductLabel: "شناسهٔ TourProduct",
  publicationStatusLabel: "وضعیت انتشار",
  visibilityLabel: "نمایش",
  salesChannelLabel: "کانال فروش",
  highlightLabel: "هایلایت",
  approveAction: "تأیید",
  rejectAction: "رد",
  suspendAction: "تعلیق Published",
  boundaryNote:
    "AgencyOffer ≠ Price · Admin Approval ≠ Agency Ownership · Commission/Settlement خارج از این سطح.",
  apiMissing: "API پیکربندی نشده است.",
  errorPrefix: "خطا:",
  busy: "در حال پردازش…",
  authRequired: "برای این عملیات باید وارد شوید.",
};

const EN: AdminAgencyOfferGovernanceCopy = {
  pageTitle: "Agency offer governance",
  pageIntro:
    "Submitted AgencyOffer review queue. Approve, reject, or suspend without commission or settlement. Agency ≠ Admin.",
  navLabel: "Offer governance navigation",
  backToAgencies: "Back to agency management",
  stepQueue: "Pending offer queue",
  refreshQueue: "Refresh queue",
  takeLabel: "Take",
  noItems: "No items in queue.",
  selectItem: "Select an offer.",
  agencyProfileLabel: "AgencyProfile id",
  tourProductLabel: "TourProduct id",
  publicationStatusLabel: "Publication status",
  visibilityLabel: "Visibility",
  salesChannelLabel: "Sales channel",
  highlightLabel: "Highlight",
  approveAction: "Approve",
  rejectAction: "Reject",
  suspendAction: "Suspend published",
  boundaryNote:
    "AgencyOffer ≠ Price · Admin Approval ≠ Agency Ownership · Commission/Settlement out of scope.",
  apiMissing: "API is not configured.",
  errorPrefix: "Error:",
  busy: "Working…",
  authRequired: "Sign in required for this action.",
};

const AR: AdminAgencyOfferGovernanceCopy = {
  ...EN,
  pageTitle: "حوكمة عروض الوكالة",
  pageIntro:
    "قائمة العروض المقدمة لمراجعة المسؤول. موافقة/رفض/تعليق دون عمولة أو تسوية.",
  navLabel: "تنقل حوكمة العروض",
  backToAgencies: "العودة لإدارة الوكالات",
  stepQueue: "قائمة العروض المعلقة",
  refreshQueue: "تحديث القائمة",
  noItems: "لا عناصر في القائمة.",
  selectItem: "اختر عرضاً.",
  approveAction: "موافقة",
  rejectAction: "رفض",
  suspendAction: "تعليق المنشور",
  authRequired: "يلزم تسجيل الدخول.",
};

export function getAdminAgencyOfferGovernanceCopy(
  locale: AppLocale,
): AdminAgencyOfferGovernanceCopy {
  if (locale === "fa") return FA;
  if (locale === "ar") return AR;
  return EN;
}
