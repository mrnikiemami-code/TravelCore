import type { AppLocale } from "@/lib/i18n";

export type PublicFlightBookingCopy = {
  searchTitle: string;
  searchNote: string;
  origin: string;
  destination: string;
  tripType: string;
  oneWay: string;
  roundTrip: string;
  departureDate: string;
  returnDate: string;
  adults: string;
  children: string;
  infants: string;
  passengerCounts: string;
  searchAction: string;
  searching: string;
  noResults: string;
  unavailable: string;
  selectOption: string;
  selected: string;
  passengersHeading: string;
  givenName: string;
  familyName: string;
  category: string;
  adult: string;
  child: string;
  infant: string;
  submit: string;
  submitting: string;
  loading: string;
  unauthorized: string;
  statusTitle: string;
  pendingNote: string;
  notConfirmed: string;
  itineraryHeading: string;
  departure: string;
  arrival: string;
  timezone: string;
  monetaryLabel: string;
  baggageHeading: string;
  fareRulesHeading: string;
  refundable: string;
  changeable: string;
  offerExpiry: string;
  ticketingDeadline: string;
  reservationLabel: string;
  ticketsHeading: string;
  ticketPending: string;
  ticketIssued: string;
  ticketNumber: string;
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
  acceptOffer: string;
  requestReservation: string;
};

const COPY: Record<AppLocale, PublicFlightBookingCopy> = {
  fa: {
    searchTitle: "جستجوی پرواز",
    searchNote:
      "جستجو موجودی زنده را قطعی نمی‌کند. بدون منبع پیکربندی‌شده گزینه‌ای ساخته نمی‌شود.",
    origin: "مبدأ (IATA)",
    destination: "مقصد (IATA)",
    tripType: "نوع سفر",
    oneWay: "یک‌طرفه",
    roundTrip: "رفت‌وبرگشت",
    departureDate: "تاریخ رفت",
    returnDate: "تاریخ برگشت",
    adults: "بزرگسال",
    children: "کودک",
    infants: "نوزاد",
    passengerCounts: "تعداد مسافران",
    searchAction: "جستجو",
    searching: "در حال جستجو…",
    noResults: "پروازی مطابق این جستجو پیدا نشد.",
    unavailable: "جستجوی پرواز فعلاً در دسترس نیست.",
    selectOption: "انتخاب این گزینه",
    selected: "انتخاب‌شده",
    passengersHeading: "نام مسافران",
    givenName: "نام",
    familyName: "نام خانوادگی",
    category: "دسته",
    adult: "بزرگسال",
    child: "کودک",
    infant: "نوزاد",
    submit: "ثبت رزرو موقت",
    submitting: "در حال ثبت…",
    loading: "در حال بارگذاری…",
    unauthorized: "این رزرو در دسترس نیست.",
    statusTitle: "وضعیت رزرو پرواز",
    pendingNote: "رزرو هنوز قطعی نیست.",
    notConfirmed: "رزرو قطعی نیست",
    itineraryHeading: "مسیر پرواز",
    departure: "پرواز",
    arrival: "ورود",
    timezone: "منطقه زمانی فرودگاه",
    monetaryLabel: "مبلغ ثبت‌شده",
    baggageHeading: "بار مجاز",
    fareRulesHeading: "قوانین نرخ و لغو",
    refundable: "قابل استرداد",
    changeable: "قابل تغییر",
    offerExpiry: "انقضای پیشنهاد",
    ticketingDeadline: "مهلت صدور بلیت",
    reservationLabel: "وضعیت رزرو تأمین‌کننده",
    ticketsHeading: "بلیت‌ها",
    ticketPending: "در حال پردازش",
    ticketIssued: "صادر شده",
    ticketNumber: "شماره بلیت",
    payTitle: "پرداخت رزرو پرواز",
    payNote: "مبلغ از پیشنهاد ثبت‌شده خوانده می‌شود. کارت بانکی در TravelCore وارد نمی‌شود.",
    payAction: "ادامه به درگاه پرداخت",
    payUnavailable: "پرداخت آنلاین فعلاً در دسترس نیست.",
    payWaiting: "وضعیت پرداخت در حال بررسی است.",
    payReceivedPendingConfirm: "پرداخت دریافت شد؛ صدور بلیت در حال پردازش است.",
    payCompensation: "بازگشت وجه در حال انجام است.",
    payReturned: "بازگشت از درگاه. این صفحه پرداخت را موفق اعلام نمی‌کند.",
    payRetry: "تلاش دوباره برای پرداخت",
    cancelAction: "درخواست لغو",
    cancelPending: "لغو در حال پردازش است. رزرو هنوز قطعی است.",
    cancelBlocked: "این لغو به بازپرداخت جزئی نیاز دارد و فعلاً قابل اجرا نیست.",
    refundPending: "بازگشت وجه در انتظار است.",
    refundSucceeded: "مبلغ بازگردانده شد.",
    acceptOffer: "پذیرش پیشنهاد",
    requestReservation: "درخواست رزرو",
  },
  en: {
    searchTitle: "Search flights",
    searchNote:
      "Search does not confirm live inventory. With no configured source, no options are fabricated.",
    origin: "Origin (IATA)",
    destination: "Destination (IATA)",
    tripType: "Trip type",
    oneWay: "One way",
    roundTrip: "Round trip",
    departureDate: "Departure date",
    returnDate: "Return date",
    adults: "Adults",
    children: "Children",
    infants: "Infants",
    passengerCounts: "Passenger counts",
    searchAction: "Search",
    searching: "Searching…",
    noResults: "No flights matched this search.",
    unavailable: "Flight search is not currently available.",
    selectOption: "Select this option",
    selected: "Selected",
    passengersHeading: "Passenger names",
    givenName: "Given name",
    familyName: "Family name",
    category: "Category",
    adult: "Adult",
    child: "Child",
    infant: "Infant",
    submit: "Prepare pending booking",
    submitting: "Preparing…",
    loading: "Loading…",
    unauthorized: "This flight booking is not available.",
    statusTitle: "Flight booking status",
    pendingNote: "This flight booking is not confirmed.",
    notConfirmed: "Not confirmed",
    itineraryHeading: "Itinerary",
    departure: "Departs",
    arrival: "Arrives",
    timezone: "Airport time zone",
    monetaryLabel: "Recorded total",
    baggageHeading: "Baggage",
    fareRulesHeading: "Fare and cancellation rules",
    refundable: "Refundable",
    changeable: "Changeable",
    offerExpiry: "Offer expiry",
    ticketingDeadline: "Ticketing deadline",
    reservationLabel: "Reservation status",
    ticketsHeading: "Tickets",
    ticketPending: "Processing",
    ticketIssued: "Issued",
    ticketNumber: "E-ticket number",
    payTitle: "Flight booking payment",
    payNote:
      "The amount comes from the recorded flight offer. TravelCore does not collect card details.",
    payAction: "Continue to payment provider",
    payUnavailable: "Online payment is not currently available.",
    payWaiting: "Payment status is being checked.",
    payReceivedPendingConfirm: "Payment received; tickets are being processed.",
    payCompensation: "A money return is in progress.",
    payReturned: "Returned from the provider. This page does not mark payment successful.",
    payRetry: "Try payment again",
    cancelAction: "Request cancellation",
    cancelPending: "Cancellation is being processed. The booking is not cancelled yet.",
    cancelBlocked: "This cancellation needs a partial refund and is not currently executable.",
    refundPending: "A refund is pending.",
    refundSucceeded: "The money has been returned.",
    acceptOffer: "Accept offer",
    requestReservation: "Request reservation",
  },
  ar: {
    searchTitle: "البحث عن رحلات",
    searchNote:
      "البحث لا يؤكد المخزون الحي. بدون مصدر مُهيأ لا تُختلق خيارات.",
    origin: "المغادرة (IATA)",
    destination: "الوصول (IATA)",
    tripType: "نوع الرحلة",
    oneWay: "اتجاه واحد",
    roundTrip: "ذهاب وعودة",
    departureDate: "تاريخ المغادرة",
    returnDate: "تاريخ العودة",
    adults: "بالغون",
    children: "أطفال",
    infants: "رضع",
    passengerCounts: "عدد المسافرين",
    searchAction: "بحث",
    searching: "جارٍ البحث…",
    noResults: "لا توجد رحلات مطابقة.",
    unavailable: "بحث الرحلات غير متاح حالياً.",
    selectOption: "اختيار هذا الخيار",
    selected: "مختار",
    passengersHeading: "أسماء المسافرين",
    givenName: "الاسم",
    familyName: "اسم العائلة",
    category: "الفئة",
    adult: "بالغ",
    child: "طفل",
    infant: "رضيع",
    submit: "تسجيل حجز مؤقت",
    submitting: "جارٍ التسجيل…",
    loading: "جارٍ التحميل…",
    unauthorized: "هذا الحجز غير متاح.",
    statusTitle: "حالة حجز الرحلة",
    pendingNote: "حجز الرحلة غير مؤكد بعد.",
    notConfirmed: "غير مؤكد",
    itineraryHeading: "المسار",
    departure: "المغادرة",
    arrival: "الوصول",
    timezone: "المنطقة الزمنية للمطار",
    monetaryLabel: "المبلغ المسجل",
    baggageHeading: "الأمتعة",
    fareRulesHeading: "قواعد الأجرة والإلغاء",
    refundable: "قابل للاسترداد",
    changeable: "قابل للتغيير",
    offerExpiry: "انتهاء العرض",
    ticketingDeadline: "موعد إصدار التذكرة",
    reservationLabel: "حالة الحجز لدى المورد",
    ticketsHeading: "التذاكر",
    ticketPending: "قيد المعالجة",
    ticketIssued: "صادرة",
    ticketNumber: "رقم التذكرة الإلكترونية",
    payTitle: "دفع حجز الرحلة",
    payNote: "المبلغ مأخوذ من العرض المسجل. لا تجمع TravelCore بيانات البطاقة.",
    payAction: "المتابعة إلى مزود الدفع",
    payUnavailable: "الدفع عبر الإنترنت غير متاح حالياً.",
    payWaiting: "جارٍ التحقق من حالة الدفع.",
    payReceivedPendingConfirm: "تم استلام الدفع؛ إصدار التذاكر قيد المعالجة.",
    payCompensation: "جاري إرجاع المبلغ.",
    payReturned: "عودة من المزود. هذه الصفحة لا تعتبر الدفع ناجحاً.",
    payRetry: "إعادة محاولة الدفع",
    cancelAction: "طلب الإلغاء",
    cancelPending: "الإلغاء قيد المعالجة. الحجز لم يُلغ بعد.",
    cancelBlocked: "يتطلب هذا الإلغاء استرداداً جزئياً وهو غير قابل للتنفيذ حالياً.",
    refundPending: "الاسترداد قيد الانتظار.",
    refundSucceeded: "تم إرجاع المبلغ.",
    acceptOffer: "قبول العرض",
    requestReservation: "طلب الحجز",
  },
};

export function getPublicFlightBookingCopy(locale: AppLocale): PublicFlightBookingCopy {
  return COPY[locale];
}
