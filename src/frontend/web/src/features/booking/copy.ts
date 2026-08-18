import type { AppLocale } from "@/lib/i18n";

export type PublicBookingCopy = {
  prepareTitle: string;
  prepareNote: string;
  statusTitle: string;
  pendingNote: string;
  notConfirmed: string;
  selectDeparture: string;
  contactHeading: string;
  displayName: string;
  email: string;
  phone: string;
  passengersHeading: string;
  givenName: string;
  familyName: string;
  category: string;
  adult: string;
  child: string;
  infant: string;
  addPassenger: string;
  submit: string;
  submitting: string;
  missingDeparture: string;
  backToTour: string;
  monetaryLabel: string;
  holdLabel: string;
  unauthorized: string;
};

const COPY: Record<AppLocale, PublicBookingCopy> = {
  fa: {
    prepareTitle: "شروع رزرو موقت",
    prepareNote:
      "این مرحله فقط یک رزرو موقت Pending می‌سازد — نه قطعی، نه پرداخت، نه تایید نهایی.",
    statusTitle: "وضعیت رزرو موقت",
    pendingNote: "وضعیت فعلی Pending است. رزرو قطعی نشده است.",
    notConfirmed: "رزرو قطعی نیست",
    selectDeparture: "انتخاب اجرا",
    contactHeading: "اطلاعات تماس",
    displayName: "نام نمایشی",
    email: "ایمیل",
    phone: "تلفن",
    passengersHeading: "مسافران",
    givenName: "نام",
    familyName: "نام خانوادگی",
    category: "دسته",
    adult: "بزرگسال",
    child: "کودک",
    infant: "نوزاد",
    addPassenger: "افزودن مسافر",
    submit: "ثبت رزرو موقت",
    submitting: "در حال ثبت…",
    missingDeparture: "اجرای منتشرشده‌ای برای شروع رزرو موقت وجود ندارد.",
    backToTour: "بازگشت به تور",
    monetaryLabel: "تصویر مالی ثبت‌شده",
    holdLabel: "نگه‌داشت ظرفیت",
    unauthorized: "این رزرو در دسترس نیست.",
  },
  en: {
    prepareTitle: "Prepare booking",
    prepareNote:
      "This step only prepares a Pending booking — not a confirmed booking and not a completed payment.",
    statusTitle: "Pending booking status",
    pendingNote: "Current status is Pending. This booking is not confirmed.",
    notConfirmed: "Not confirmed",
    selectDeparture: "Choose departure",
    contactHeading: "Contact",
    displayName: "Display name",
    email: "Email",
    phone: "Phone",
    passengersHeading: "Passengers",
    givenName: "Given name",
    familyName: "Family name",
    category: "Category",
    adult: "Adult",
    child: "Child",
    infant: "Infant",
    addPassenger: "Add passenger",
    submit: "Prepare pending booking",
    submitting: "Preparing…",
    missingDeparture: "No published departure is available to prepare a booking.",
    backToTour: "Back to tour",
    monetaryLabel: "Recorded monetary snapshot",
    holdLabel: "Capacity hold",
    unauthorized: "This booking is not available.",
  },
  ar: {
    prepareTitle: "إعداد حجز مؤقت",
    prepareNote:
      "هذه الخطوة تُنشئ حجزًا مؤقتًا بحالة Pending فقط — ليست تأكيدًا وليست عملية دفع مكتملة.",
    statusTitle: "حالة الحجز المؤقت",
    pendingNote: "الحالة الحالية Pending. لم يتم تأكيد الحجز.",
    notConfirmed: "غير مؤكد",
    selectDeparture: "اختيار الرحلة",
    contactHeading: "بيانات التواصل",
    displayName: "الاسم المعروض",
    email: "البريد",
    phone: "الهاتف",
    passengersHeading: "المسافرون",
    givenName: "الاسم",
    familyName: "اسم العائلة",
    category: "الفئة",
    adult: "بالغ",
    child: "طفل",
    infant: "رضيع",
    addPassenger: "إضافة مسافر",
    submit: "تسجيل حجز مؤقت",
    submitting: "جارٍ التسجيل…",
    missingDeparture: "لا توجد رحلة منشورة لإعداد الحجز.",
    backToTour: "العودة إلى الجولة",
    monetaryLabel: "اللقطة المالية المسجلة",
    holdLabel: "احتجاز السعة",
    unauthorized: "هذا الحجز غير متاح.",
  },
};

export function getPublicBookingCopy(locale: AppLocale): PublicBookingCopy {
  return COPY[locale];
}
