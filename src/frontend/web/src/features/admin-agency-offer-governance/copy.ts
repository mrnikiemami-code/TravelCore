import type { AppLocale } from "@/lib/i18n";

export type AdminAgencyOfferGovernanceCopy = {
  pageTitle: string;
  pageIntro: string;
  navLabel: string;
  backToAgencies: string;
  stepQueue: string;
  refreshQueue: string;
  takeLabel: string;
  statusFilterLabel: string;
  statusSubmitted: string;
  statusApproved: string;
  statusRejected: string;
  statusSuspended: string;
  statusRetired: string;
  noItems: string;
  selectItem: string;
  agencyProfileLabel: string;
  tourProductLabel: string;
  publicationStatusLabel: string;
  visibilityLabel: string;
  salesChannelLabel: string;
  highlightLabel: string;
  lastDecisionLabel: string;
  historyAvailableLabel: string;
  historyAvailableYes: string;
  historyAvailableNo: string;
  approveAction: string;
  rejectAction: string;
  suspendAction: string;
  evaluatePolicyAction: string;
  loadHistoryAction: string;
  historyStep: string;
  noHistory: string;
  policyStep: string;
  policyAggregateLabel: string;
  policyHooksLabel: string;
  noPolicyReport: string;
  boundaryNote: string;
  apiMissing: string;
  errorPrefix: string;
  busy: string;
  authRequired: string;
};

const FA: AdminAgencyOfferGovernanceCopy = {
  pageTitle: "حاکمیت Offer آژانس",
  pageIntro:
    "جستجو و بررسی Offer بر اساس وضعیت عملیاتی. تأیید/رد/تعلیق بدون کمیسیون یا تسویه. Agency ≠ Admin.",
  navLabel: "ناوبری حاکمیت Offer",
  backToAgencies: "بازگشت به مدیریت آژانس",
  stepQueue: "جستجوی عملیاتی Offer",
  refreshQueue: "بروزرسانی فهرست",
  takeLabel: "تعداد",
  statusFilterLabel: "وضعیت انتشار",
  statusSubmitted: "در انتظار بررسی",
  statusApproved: "تأییدشده",
  statusRejected: "ردشده",
  statusSuspended: "تعلیق‌شده",
  statusRetired: "بازنشسته‌شده",
  noItems: "موردی با این فیلتر نیست.",
  selectItem: "یک Offer را انتخاب کنید.",
  agencyProfileLabel: "شناسهٔ AgencyProfile",
  tourProductLabel: "شناسهٔ TourProduct",
  publicationStatusLabel: "وضعیت انتشار",
  visibilityLabel: "نمایش",
  salesChannelLabel: "کانال فروش",
  highlightLabel: "هایلایت",
  lastDecisionLabel: "آخرین تصمیم Governance",
  historyAvailableLabel: "سابقه موجود",
  historyAvailableYes: "بله",
  historyAvailableNo: "خیر",
  approveAction: "تأیید",
  rejectAction: "رد",
  suspendAction: "تعلیق Published",
  evaluatePolicyAction: "ارزیابی Policy",
  loadHistoryAction: "سابقه Governance",
  historyStep: "سابقه عملیاتی Offer",
  noHistory: "سابقه‌ای ثبت نشده است.",
  policyStep: "نتیجهٔ Policy (عملیاتی)",
  policyAggregateLabel: "نتیجهٔ تجمعی",
  policyHooksLabel: "Hookها",
  noPolicyReport: "هنوز ارزیابی Policy انجام نشده است.",
  boundaryNote:
    "AgencyOffer ≠ Price · Audit ≠ Ledger · Commission/Settlement خارج از این سطح.",
  apiMissing: "API پیکربندی نشده است.",
  errorPrefix: "خطا:",
  busy: "در حال پردازش…",
  authRequired: "برای این عملیات باید وارد شوید.",
};

const EN: AdminAgencyOfferGovernanceCopy = {
  pageTitle: "Agency offer governance",
  pageIntro:
    "Find and review AgencyOffers by operational status. Approve, reject, or suspend without commission or settlement. Agency ≠ Admin.",
  navLabel: "Offer governance navigation",
  backToAgencies: "Back to agency management",
  stepQueue: "Operational offer search",
  refreshQueue: "Refresh list",
  takeLabel: "Take",
  statusFilterLabel: "Publication status",
  statusSubmitted: "Pending review",
  statusApproved: "Approved",
  statusRejected: "Rejected",
  statusSuspended: "Suspended",
  statusRetired: "Retired",
  noItems: "No items for this filter.",
  selectItem: "Select an offer.",
  agencyProfileLabel: "AgencyProfile id",
  tourProductLabel: "TourProduct id",
  publicationStatusLabel: "Publication status",
  visibilityLabel: "Visibility",
  salesChannelLabel: "Sales channel",
  highlightLabel: "Highlight",
  lastDecisionLabel: "Last governance decision",
  historyAvailableLabel: "History available",
  historyAvailableYes: "Yes",
  historyAvailableNo: "No",
  approveAction: "Approve",
  rejectAction: "Reject",
  suspendAction: "Suspend published",
  evaluatePolicyAction: "Evaluate policies",
  loadHistoryAction: "Governance history",
  historyStep: "Operational offer history",
  noHistory: "No governance history yet.",
  policyStep: "Policy evaluation (operational)",
  policyAggregateLabel: "Aggregate decision",
  policyHooksLabel: "Hooks",
  noPolicyReport: "No policy evaluation loaded yet.",
  boundaryNote:
    "AgencyOffer ≠ Price · Audit ≠ Ledger · Commission/Settlement out of scope.",
  apiMissing: "API is not configured.",
  errorPrefix: "Error:",
  busy: "Working…",
  authRequired: "Sign in required for this action.",
};

const AR: AdminAgencyOfferGovernanceCopy = {
  ...EN,
  pageTitle: "حوكمة عروض الوكالة",
  pageIntro:
    "ابحث وراجع العروض حسب الحالة التشغيلية. موافقة/رفض/تعليق دون عمولة أو تسوية.",
  navLabel: "تنقل حوكمة العروض",
  backToAgencies: "العودة لإدارة الوكالات",
  stepQueue: "بحث تشغيلي للعروض",
  refreshQueue: "تحديث القائمة",
  statusFilterLabel: "حالة النشر",
  statusSubmitted: "بانتظار المراجعة",
  statusApproved: "موافق عليه",
  statusRejected: "مرفوض",
  statusSuspended: "معلق",
  statusRetired: "متقاعد",
  noItems: "لا عناصر لهذا التصفية.",
  selectItem: "اختر عرضاً.",
  lastDecisionLabel: "آخر قرار حوكمة",
  historyAvailableLabel: "السجل متاح",
  historyAvailableYes: "نعم",
  historyAvailableNo: "لا",
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
