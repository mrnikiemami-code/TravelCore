import type { AppLocale } from "@/lib/i18n";

export type PublicHotelBookingCopy = {
  prepareTitle: string;
  prepareNote: string;
  statusTitle: string;
  pendingNote: string;
  notConfirmed: string;
  stayHeading: string;
  checkIn: string;
  checkOut: string;
  contactHeading: string;
  email: string;
  phone: string;
  roomsHeading: string;
  roomLabel: string;
  addRoom: string;
  guestsHeading: string;
  givenName: string;
  familyName: string;
  category: string;
  adult: string;
  child: string;
  ageAtCheckIn: string;
  leadGuest: string;
  addGuest: string;
  submit: string;
  submitting: string;
  loading: string;
  unauthorized: string;
  backToPlace: string;
  monetaryLabel: string;
  holdLabel: string;
  termsHeading: string;
  termsNotExecutable: string;
  payTitle: string;
  payNote: string;
  payAction: string;
  payUnavailable: string;
  payWaiting: string;
  payReceivedPendingConfirm: string;
  payCompensation: string;
  payReturned: string;
  payRetry: string;
  cancelAction: string;
  cancelPending: string;
  cancelBlocked: string;
  refundPending: string;
  refundSucceeded: string;
  confirmationCode: string;
  bookHotel: string;
};

const COPY: Record<AppLocale, PublicHotelBookingCopy> = {
  fa: {
    prepareTitle: "شروع رزرو هتل",
    prepareNote:
      "این مرحله فقط یک رزرو موقت Pending می‌سازد — نه موجودی قطعی، نه نرخ پذیرفته‌شده، نه پرداخت.",
    statusTitle: "وضعیت رزرو هتل",
    pendingNote: "رزرو هنوز قطعی نیست.",
    notConfirmed: "رزرو قطعی نیست",
    stayHeading: "تاریخ اقامت",
    checkIn: "ورود",
    checkOut: "خروج",
    contactHeading: "اطلاعات تماس (جدا از مهمان اصلی)",
    email: "ایمیل",
    phone: "تلفن",
    roomsHeading: "اتاق‌ها و مهمانان",
    roomLabel: "اتاق",
    addRoom: "افزودن اتاق",
    guestsHeading: "مهمانان این اتاق",
    givenName: "نام",
    familyName: "نام خانوادگی",
    category: "دسته",
    adult: "بزرگسال",
    child: "کودک",
    ageAtCheckIn: "سن در زمان ورود",
    leadGuest: "مهمان اصلی",
    addGuest: "افزودن مهمان به این اتاق",
    submit: "ثبت رزرو موقت",
    submitting: "در حال ثبت…",
    loading: "در حال بارگذاری…",
    unauthorized: "این رزرو در دسترس نیست.",
    backToPlace: "بازگشت به هتل",
    monetaryLabel: "مبلغ ثبت‌شده",
    holdLabel: "نگه‌داشت موجودی",
    termsHeading: "شرایط لغو",
    termsNotExecutable: "این بازه فعلاً قابل اجرا نیست.",
    payTitle: "پرداخت رزرو هتل",
    payNote: "مبلغ از نرخ ثبت‌شده خوانده می‌شود. کارت بانکی در TravelCore وارد نمی‌شود.",
    payAction: "ادامه به درگاه پرداخت",
    payUnavailable: "پرداخت آنلاین فعلاً در دسترس نیست.",
    payWaiting: "وضعیت پرداخت در حال بررسی است.",
    payReceivedPendingConfirm: "پرداخت دریافت شد؛ تأیید هتل در حال پردازش است.",
    payCompensation: "بازگشت وجه در حال انجام است.",
    payReturned: "بازگشت از درگاه. این صفحه پرداخت را موفق اعلام نمی‌کند.",
    payRetry: "تلاش دوباره برای پرداخت",
    cancelAction: "درخواست لغو",
    cancelPending: "لغو در حال پردازش است. رزرو هنوز قطعی است.",
    cancelBlocked: "این لغو به بازپرداخت جزئی نیاز دارد و فعلاً قابل اجرا نیست.",
    refundPending: "بازگشت وجه در انتظار است.",
    refundSucceeded: "مبلغ بازگردانده شد.",
    confirmationCode: "کد تأیید تأمین‌کننده",
    bookHotel: "رزرو این هتل",
  },
  en: {
    prepareTitle: "Prepare hotel booking",
    prepareNote:
      "This step only prepares a Pending hotel booking — not availability, an accepted rate, or payment.",
    statusTitle: "Hotel booking status",
    pendingNote: "This hotel booking is not confirmed.",
    notConfirmed: "Not confirmed",
    stayHeading: "Stay dates",
    checkIn: "Check-in",
    checkOut: "Check-out",
    contactHeading: "Booking contact (separate from the lead guest)",
    email: "Email",
    phone: "Phone",
    roomsHeading: "Rooms and guests",
    roomLabel: "Room",
    addRoom: "Add room",
    guestsHeading: "Guests in this room",
    givenName: "Given name",
    familyName: "Family name",
    category: "Category",
    adult: "Adult",
    child: "Child",
    ageAtCheckIn: "Age at check-in",
    leadGuest: "Lead guest",
    addGuest: "Add guest to this room",
    submit: "Prepare pending booking",
    submitting: "Preparing…",
    loading: "Loading…",
    unauthorized: "This hotel booking is not available.",
    backToPlace: "Back to hotel",
    monetaryLabel: "Recorded total",
    holdLabel: "Availability hold",
    termsHeading: "Cancellation terms",
    termsNotExecutable: "This interval is not currently executable.",
    payTitle: "Hotel booking payment",
    payNote:
      "The amount comes from the recorded hotel rate. TravelCore does not collect card details.",
    payAction: "Continue to payment provider",
    payUnavailable: "Online payment is not currently available.",
    payWaiting: "Payment status is being checked.",
    payReceivedPendingConfirm: "Payment received; hotel confirmation is being processed.",
    payCompensation: "A money return is in progress.",
    payReturned: "Returned from the provider. This page does not mark payment successful.",
    payRetry: "Try payment again",
    cancelAction: "Request cancellation",
    cancelPending: "Cancellation is being processed. The booking is not cancelled yet.",
    cancelBlocked: "This cancellation needs a partial refund and is not currently executable.",
    refundPending: "A refund is pending.",
    refundSucceeded: "The money has been returned.",
    confirmationCode: "Supplier confirmation code",
    bookHotel: "Book this hotel",
  },
  ar: {
    prepareTitle: "إعداد حجز فندق",
    prepareNote:
      "هذه الخطوة تُنشئ حجز فندق مؤقت بحالة Pending فقط — ليست توفّرًا وليست سعرًا مقبولًا وليست دفعًا.",
    statusTitle: "حالة حجز الفندق",
    pendingNote: "حجز الفندق غير مؤكد بعد.",
    notConfirmed: "غير مؤكد",
    stayHeading: "تواريخ الإقامة",
    checkIn: "تسجيل الوصول",
    checkOut: "تسجيل المغادرة",
    contactHeading: "جهة اتصال الحجز (منفصلة عن الضيف الرئيسي)",
    email: "البريد",
    phone: "الهاتف",
    roomsHeading: "الغرف والضيوف",
    roomLabel: "غرفة",
    addRoom: "إضافة غرفة",
    guestsHeading: "ضيوف هذه الغرفة",
    givenName: "الاسم",
    familyName: "اسم العائلة",
    category: "الفئة",
    adult: "بالغ",
    child: "طفل",
    ageAtCheckIn: "العمر عند تسجيل الوصول",
    leadGuest: "الضيف الرئيسي",
    addGuest: "إضافة ضيف إلى هذه الغرفة",
    submit: "تسجيل حجز مؤقت",
    submitting: "جارٍ التسجيل…",
    loading: "جارٍ التحميل…",
    unauthorized: "هذا الحجز غير متاح.",
    backToPlace: "العودة إلى الفندق",
    monetaryLabel: "المبلغ المسجل",
    holdLabel: "احتجاز التوفر",
    termsHeading: "شروط الإلغاء",
    termsNotExecutable: "هذه الفترة غير قابلة للتنفيذ حالياً.",
    payTitle: "دفع حجز الفندق",
    payNote: "المبلغ مأخوذ من السعر المسجل. لا تجمع TravelCore بيانات البطاقة.",
    payAction: "المتابعة إلى مزود الدفع",
    payUnavailable: "الدفع عبر الإنترنت غير متاح حالياً.",
    payWaiting: "جارٍ التحقق من حالة الدفع.",
    payReceivedPendingConfirm: "تم استلام الدفع؛ تأكيد الفندق قيد المعالجة.",
    payCompensation: "جاري إرجاع المبلغ.",
    payReturned: "عودة من المزود. هذه الصفحة لا تعتبر الدفع ناجحاً.",
    payRetry: "إعادة محاولة الدفع",
    cancelAction: "طلب الإلغاء",
    cancelPending: "الإلغاء قيد المعالجة. الحجز لم يُلغ بعد.",
    cancelBlocked: "يتطلب هذا الإلغاء استرداداً جزئياً وهو غير قابل للتنفيذ حالياً.",
    refundPending: "الاسترداد قيد الانتظار.",
    refundSucceeded: "تم إرجاع المبلغ.",
    confirmationCode: "رمز تأكيد المورد",
    bookHotel: "احجز هذا الفندق",
  },
};

export function getPublicHotelBookingCopy(locale: AppLocale): PublicHotelBookingCopy {
  return COPY[locale];
}
