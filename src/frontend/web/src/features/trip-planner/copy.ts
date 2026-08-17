import type { AppLocale } from "@/lib/i18n";

export type TripPlannerWorkflowCopy = {
  pageTitle: string;
  pageIntro: string;
  honestCtaNote: string;
  stepDestination: string;
  stepTiming: string;
  stepTravelers: string;
  stepPreferences: string;
  stepBudget: string;
  stepContact: string;
  stepConsent: string;
  stepReview: string;
  next: string;
  back: string;
  startPlanning: string;
  submitLead: string;
  submittedTitle: string;
  submittedBody: string;
  destinationUndecided: string;
  destinationIdsLabel: string;
  timingKindLabel: string;
  timingUndecided: string;
  timingExact: string;
  timingFlexible: string;
  timingApproximate: string;
  exactStartLabel: string;
  exactEndLabel: string;
  flexibleEarliestLabel: string;
  flexibleLatestLabel: string;
  adultsLabel: string;
  childrenLabel: string;
  infantsLabel: string;
  budgetMinLabel: string;
  budgetMaxLabel: string;
  currencyLabel: string;
  accommodationLabel: string;
  transportLabel: string;
  tripStyleLabel: string;
  interestsLabel: string;
  travelerNoteLabel: string;
  displayNameLabel: string;
  emailLabel: string;
  phoneLabel: string;
  followUpLabel: string;
  marketingLabel: string;
  privacyVersionLabel: string;
  preferredChannelLabel: string;
  reviewHeading: string;
  apiMissing: string;
  busy: string;
  errorPrefix: string;
};

const fa: TripPlannerWorkflowCopy = {
  pageTitle: "برنامه‌ریزی سفر",
  pageIntro:
    "نیت سفر خود را مرحله‌به‌مرحله ثبت کنید. این فرم درخواست پیگیری است — نه رزرو، پرداخت یا خرید.",
  honestCtaNote: "بدون «رزرو الآن» · بدون پرداخت · بدون قیمت نهایی",
  stepDestination: "مقصد",
  stepTiming: "زمان",
  stepTravelers: "مسافران",
  stepPreferences: "ترجیحات",
  stepBudget: "بودجه",
  stepContact: "تماس",
  stepConsent: "رضایت",
  stepReview: "بازبینی",
  next: "بعدی",
  back: "قبلی",
  startPlanning: "شروع",
  submitLead: "ارسال درخواست پیگیری",
  submittedTitle: "درخواست ثبت شد",
  submittedBody: "تیم ما برای هماهنگی با شما تماس می‌گیرد. این مرحله رزرو یا پرداخت نیست.",
  destinationUndecided: "هنوز تصمیم نگرفته‌ام",
  destinationIdsLabel: "شناسه‌های منطقی مقصد (اختیاری، با کاما)",
  timingKindLabel: "نوع زمان‌بندی",
  timingUndecided: "نامشخص",
  timingExact: "تاریخ دقیق",
  timingFlexible: "بازه انعطاف‌پذیر",
  timingApproximate: "تقریبی (فصل/ماه)",
  exactStartLabel: "شروع (yyyy-MM-dd)",
  exactEndLabel: "پایان (yyyy-MM-dd)",
  flexibleEarliestLabel: "اولین شروع ممکن",
  flexibleLatestLabel: "آخرین شروع ممکن",
  adultsLabel: "بزرگسال",
  childrenLabel: "کودک",
  infantsLabel: "نوزاد",
  budgetMinLabel: "حداقل بودجه",
  budgetMaxLabel: "حداکثر بودجه",
  currencyLabel: "ارز (ISO)",
  accommodationLabel: "اقامت",
  transportLabel: "حمل‌ونقل",
  tripStyleLabel: "سبک سفر",
  interestsLabel: "علاقه‌ها (با کاما)",
  travelerNoteLabel: "یادداشت مسافر",
  displayNameLabel: "نام",
  emailLabel: "ایمیل",
  phoneLabel: "تلفن",
  followUpLabel: "اجازه پیگیری تماس",
  marketingLabel: "پیام‌های بازاریابی (اختیاری)",
  privacyVersionLabel: "نسخه اطلاع‌رسانی حریم خصوصی",
  preferredChannelLabel: "کانال ترجیحی (Email/Phone)",
  reviewHeading: "خلاصه قبل از ارسال",
  apiMissing: "آدرس API تنظیم نشده است.",
  busy: "در حال انجام…",
  errorPrefix: "خطا:",
};

const en: TripPlannerWorkflowCopy = {
  pageTitle: "Plan your trip",
  pageIntro:
    "Capture your travel intent step by step. This is a follow-up request — not booking, checkout, or payment.",
  honestCtaNote: "No Book Now · No checkout · No final price",
  stepDestination: "Destination",
  stepTiming: "Timing",
  stepTravelers: "Travelers",
  stepPreferences: "Preferences",
  stepBudget: "Budget",
  stepContact: "Contact",
  stepConsent: "Consent",
  stepReview: "Review",
  next: "Next",
  back: "Back",
  startPlanning: "Start",
  submitLead: "Submit follow-up request",
  submittedTitle: "Request submitted",
  submittedBody: "Our team will contact you to follow up. This is not a reservation or payment step.",
  destinationUndecided: "Not decided yet",
  destinationIdsLabel: "Logical destination IDs (optional, comma-separated)",
  timingKindLabel: "Timing kind",
  timingUndecided: "Undecided",
  timingExact: "Exact dates",
  timingFlexible: "Flexible range",
  timingApproximate: "Approximate (season/month)",
  exactStartLabel: "Start (yyyy-MM-dd)",
  exactEndLabel: "End (yyyy-MM-dd)",
  flexibleEarliestLabel: "Earliest start",
  flexibleLatestLabel: "Latest start",
  adultsLabel: "Adults",
  childrenLabel: "Children",
  infantsLabel: "Infants",
  budgetMinLabel: "Minimum budget",
  budgetMaxLabel: "Maximum budget",
  currencyLabel: "Currency (ISO)",
  accommodationLabel: "Accommodation",
  transportLabel: "Transport",
  tripStyleLabel: "Trip style",
  interestsLabel: "Interests (comma-separated)",
  travelerNoteLabel: "Traveler note",
  displayNameLabel: "Display name",
  emailLabel: "Email",
  phoneLabel: "Phone",
  followUpLabel: "Allow follow-up contact",
  marketingLabel: "Marketing messages (optional)",
  privacyVersionLabel: "Privacy notice version",
  preferredChannelLabel: "Preferred channel (Email/Phone)",
  reviewHeading: "Summary before submit",
  apiMissing: "API base URL is not configured.",
  busy: "Working…",
  errorPrefix: "Error:",
};

const ar: TripPlannerWorkflowCopy = {
  pageTitle: "خطط رحلتك",
  pageIntro:
    "سجّل نيت سفرك خطوة بخطوة. هذا طلب متابعة — وليس حجزاً أو دفعاً أو شراءً.",
  honestCtaNote: "بدون «احجز الآن» · بدون دفع · بدون سعر نهائي",
  stepDestination: "الوجهة",
  stepTiming: "التوقيت",
  stepTravelers: "المسافرون",
  stepPreferences: "التفضيلات",
  stepBudget: "الميزانية",
  stepContact: "التواصل",
  stepConsent: "الموافقة",
  stepReview: "مراجعة",
  next: "التالي",
  back: "السابق",
  startPlanning: "ابدأ",
  submitLead: "إرسال طلب المتابعة",
  submittedTitle: "تم إرسال الطلب",
  submittedBody: "سيتواصل معك فريقنا للمتابعة. هذه ليست خطوة حجز أو دفع.",
  destinationUndecided: "لم أقرر بعد",
  destinationIdsLabel: "معرّفات الوجهة المنطقية (اختياري، مفصولة بفاصلة)",
  timingKindLabel: "نوع التوقيت",
  timingUndecided: "غير محدد",
  timingExact: "تواريخ محددة",
  timingFlexible: "نطاق مرن",
  timingApproximate: "تقريبي (موسم/شهر)",
  exactStartLabel: "البداية (yyyy-MM-dd)",
  exactEndLabel: "النهاية (yyyy-MM-dd)",
  flexibleEarliestLabel: "أبكر بداية",
  flexibleLatestLabel: "آخر بداية",
  adultsLabel: "بالغون",
  childrenLabel: "أطفال",
  infantsLabel: "رضّع",
  budgetMinLabel: "الحد الأدنى للميزانية",
  budgetMaxLabel: "الحد الأقصى للميزانية",
  currencyLabel: "العملة (ISO)",
  accommodationLabel: "الإقامة",
  transportLabel: "النقل",
  tripStyleLabel: "أسلوب الرحلة",
  interestsLabel: "الاهتمامات (مفصولة بفاصلة)",
  travelerNoteLabel: "ملاحظة المسافر",
  displayNameLabel: "الاسم",
  emailLabel: "البريد الإلكتروني",
  phoneLabel: "الهاتف",
  followUpLabel: "السماح بمتابعة التواصل",
  marketingLabel: "رسائل تسويقية (اختياري)",
  privacyVersionLabel: "إصدار إشعار الخصوصية",
  preferredChannelLabel: "قناة مفضلة (Email/Phone)",
  reviewHeading: "ملخص قبل الإرسال",
  apiMissing: "لم يتم ضبط عنوان API.",
  busy: "جارٍ التنفيذ…",
  errorPrefix: "خطأ:",
};

const COPY: Record<AppLocale, TripPlannerWorkflowCopy> = { fa, en, ar };

export function getTripPlannerWorkflowCopy(locale: AppLocale): TripPlannerWorkflowCopy {
  return COPY[locale];
}
