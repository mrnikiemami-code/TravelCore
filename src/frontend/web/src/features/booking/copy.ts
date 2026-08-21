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
  paymentBoundaryTitle: string;
  paymentBoundaryBody: string;
  paymentBoundaryNote: string;
  payTitle: string;
  payNote: string;
  payAction: string;
  payUnavailable: string;
  payWaiting: string;
  payReceivedPendingConfirm: string;
  payCompensation: string;
  payReturned: string;
  payRetry: string;
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
    paymentBoundaryTitle: "مرز پرداخت",
    paymentBoundaryBody:
      "رزرو در وضعیت Pending است. پرداخت آنلاین برای این مسیر فروش هنوز فعال نیست — نه تراکنش جعلی، نه رسید جعلی، نه Confirm.",
    paymentBoundaryNote:
      "Booking ≠ Payment · Payment initiation ≠ موفقیت پرداخت · Payment success ≠ Confirm خودکار",
    payTitle: "پرداخت رزرو",
    payNote: "مبلغ از رزرو ثبت‌شده خوانده می‌شود. کارت بانکی در TravelCore وارد نمی‌شود.",
    payAction: "ادامه به درگاه پرداخت",
    payUnavailable: "پرداخت آنلاین فعلاً در دسترس نیست.",
    payWaiting: "وضعیت پرداخت در حال بررسی است.",
    payReceivedPendingConfirm: "پرداخت دریافت شد؛ تأیید رزرو در حال پردازش است.",
    payCompensation: "بازگشت وجه در حال انجام است. رزرو قطعی نیست.",
    payReturned: "بازگشت از درگاه. این صفحه پرداخت را موفق اعلام نمی‌کند.",
    payRetry: "تلاش دوباره برای پرداخت",
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
    paymentBoundaryTitle: "Payment boundary",
    paymentBoundaryBody:
      "This booking stays Pending. Online payment is not activated for this sell path — no fake transaction, receipt, or Confirm.",
    paymentBoundaryNote:
      "Booking ≠ Payment · Payment initiation ≠ payment success · Payment success ≠ automatic Confirm",
    payTitle: "Booking payment",
    payNote: "The amount comes from the recorded booking. TravelCore does not collect card details.",
    payAction: "Continue to payment provider",
    payUnavailable: "Online payment is not currently available.",
    payWaiting: "Payment status is being checked.",
    payReceivedPendingConfirm: "Payment received; booking confirmation is being processed.",
    payCompensation: "A money return is in progress. This booking is not confirmed.",
    payReturned: "Returned from the provider. This page does not mark payment successful.",
    payRetry: "Try payment again",
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
    paymentBoundaryTitle: "حدود الدفع",
    paymentBoundaryBody:
      "الحجز يبقى Pending. الدفع عبر الإنترنت غير مفعّل لهذا المسار — بلا معاملة وهمية وبلا إيصال وبلا تأكيد.",
    paymentBoundaryNote:
      "Booking ≠ Payment · بدء الدفع ≠ نجاح الدفع · نجاح الدفع ≠ تأكيد تلقائي",
    payTitle: "دفع الحجز",
    payNote: "المبلغ مأخوذ من الحجز المسجل. لا تجمع TravelCore بيانات البطاقة.",
    payAction: "المتابعة إلى مزود الدفع",
    payUnavailable: "الدفع عبر الإنترنت غير متاح حالياً.",
    payWaiting: "جارٍ التحقق من حالة الدفع.",
    payReceivedPendingConfirm: "تم استلام الدفع؛ تأكيد الحجز قيد المعالجة.",
    payCompensation: "جاري إرجاع المبلغ. الحجز غير مؤكد.",
    payReturned: "عودة من المزود. هذه الصفحة لا تعتبر الدفع ناجحاً.",
    payRetry: "إعادة محاولة الدفع",
  },
};

export function getPublicBookingCopy(locale: AppLocale): PublicBookingCopy {
  return COPY[locale];
}
