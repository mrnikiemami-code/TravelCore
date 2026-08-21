import Link from "next/link";
import { Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

/**
 * Customer Dashboard overview foundation (TC-P37-T002).
 * Consumer product · honest empty states · no fake trips/bookings/payments.
 */
export function CustomerDashboardFoundation({
  locale,
}: {
  locale: AppLocale;
}) {
  const base = `/${locale}/me`;
  const copy =
    locale === "fa"
      ? {
          greeting: "سلام — اینجا فضای سفر شماست",
          intro:
            "رزروها، پرداخت‌ها و مدارک سفر از حقیقت سرور خوانده می‌شوند. داده جعلی نشان نمی‌دهیم.",
          trips: "سفرهای من",
          tripsEmpty: "هنوز سفری ندارید. از بازار تورها شروع کنید.",
          browseTours: "مشاهده تورها",
          bookings: "رزروها",
          bookingsEmpty:
            "رزرو Pending/Confirmed وقتی با دسترسی معتبر موجود باشد اینجا دیده می‌شود.",
          payments: "پرداخت‌ها",
          paymentsEmpty:
            "وضعیت پرداخت فقط از قرارداد Payment خوانده می‌شود — نه از بازگشت مرورگر.",
          documents: "مدارک",
          documentsEmpty: "مدرکی برای نمایش منتشر نشده است.",
          passengers: "مسافران ذخیره‌شده",
          passengersEmpty: "هنوز مسافر ذخیره‌شده‌ای ندارید.",
          notifications: "اعلان‌ها",
          notificationsEmpty: "اعلانی برای نمایش نیست.",
          profile: "پروفایل",
          profileHint: "تنظیمات تماس و ترجیحات سفر",
          open: "باز کردن",
          honest: "خالی · صادقانه",
          next: "قدم بعدی",
          nextBody:
            "اگر همین الان رزرو موقت ساخته‌اید، لینک وضعیت از صفحه رزرو در دسترس است. این داشبورد فهرست سراسری جعلی نمی‌سازد.",
        }
      : locale === "ar"
        ? {
            greeting: "مرحباً — هذه مساحة سفرك",
            intro:
              "تُقرأ الحجوزات والمدفوعات والمستندات من حقيقة الخادم. لا نعرض بيانات وهمية.",
            trips: "رحلاتي",
            tripsEmpty: "لا رحلات بعد. ابدأ من سوق الجولات.",
            browseTours: "عرض الجولات",
            bookings: "الحجوزات",
            bookingsEmpty:
              "تظهر حجوزات Pending/Confirmed عند توفر وصول صالح.",
            payments: "المدفوعات",
            paymentsEmpty:
              "تُقرأ حالة الدفع من عقد Payment فقط — وليس من عودة المتصفح.",
            documents: "المستندات",
            documentsEmpty: "لا مستندات منشورة للعرض.",
            passengers: "المسافرون المحفوظون",
            passengersEmpty: "لا مسافرين محفوظين بعد.",
            notifications: "الإشعارات",
            notificationsEmpty: "لا إشعارات للعرض.",
            profile: "الملف",
            profileHint: "إعدادات التواصل وتفضيلات السفر",
            open: "فتح",
            honest: "فارغ · بصدق",
            next: "الخطوة التالية",
            nextBody:
              "إذا أنشأت حجزاً معلقاً للتو، رابط الحالة متاح من صفحة الحجز. لا ننشئ قائمة وهمية هنا.",
          }
        : {
            greeting: "Welcome — this is your travel space",
            intro:
              "Bookings, payments, and travel documents come from server truth. We do not invent customer rows.",
            trips: "My trips",
            tripsEmpty: "No trips yet. Start from the tour marketplace.",
            browseTours: "Browse tours",
            bookings: "Bookings",
            bookingsEmpty:
              "Pending/Confirmed bookings appear here when you have a valid access path.",
            payments: "Payments",
            paymentsEmpty:
              "Payment status comes from the Payment contract only — not from browser return.",
            documents: "Documents",
            documentsEmpty: "No published documents to show yet.",
            passengers: "Saved passengers",
            passengersEmpty: "No saved passengers yet.",
            notifications: "Notifications",
            notificationsEmpty: "No notifications to show.",
            profile: "Profile",
            profileHint: "Contact settings and travel preferences",
            open: "Open",
            honest: "Empty · honest",
            next: "Next step",
            nextBody:
              "If you just prepared a Pending booking, use the status link from that booking page. This dashboard does not invent a global booking list.",
          };

  const cards = [
    {
      id: "trips",
      title: copy.trips,
      body: copy.tripsEmpty,
      href: `${base}/bookings`,
      cta: copy.browseTours,
      ctaHref: `/${locale}/tours`,
    },
    {
      id: "bookings",
      title: copy.bookings,
      body: copy.bookingsEmpty,
      href: `${base}/bookings`,
      cta: copy.open,
      ctaHref: `${base}/bookings`,
    },
    {
      id: "payments",
      title: copy.payments,
      body: copy.paymentsEmpty,
      href: `${base}/payments`,
      cta: copy.open,
      ctaHref: `${base}/payments`,
    },
    {
      id: "documents",
      title: copy.documents,
      body: copy.documentsEmpty,
      href: `${base}/documents`,
      cta: copy.open,
      ctaHref: `${base}/documents`,
    },
    {
      id: "passengers",
      title: copy.passengers,
      body: copy.passengersEmpty,
      href: `${base}/passengers`,
      cta: copy.open,
      ctaHref: `${base}/passengers`,
    },
    {
      id: "notifications",
      title: copy.notifications,
      body: copy.notificationsEmpty,
      href: `${base}/notifications`,
      cta: copy.open,
      ctaHref: `${base}/notifications`,
    },
  ];

  return (
    <div className="flex flex-col gap-6">
      <header className="rounded-2xl border border-border bg-surface p-5 shadow-sm sm:p-6">
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#1D4ED8]">
          {copy.honest}
        </p>
        <h1 className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
          {copy.greeting}
        </h1>
        <p className="mt-2 max-w-2xl text-sm text-muted-foreground">{copy.intro}</p>
        <div className="mt-4 flex flex-wrap gap-2">
          <Link
            href={`/${locale}/tours`}
            className="min-h-touch inline-flex items-center rounded-lg bg-[#1D4ED8] px-4 text-sm font-semibold text-white hover:bg-[#1E40AF]"
          >
            {copy.browseTours}
          </Link>
          <Link
            href={`${base}/profile`}
            className="min-h-touch inline-flex items-center rounded-lg border border-border bg-background px-4 text-sm font-medium hover:border-[#1D4ED8]/40"
          >
            {copy.profile}
          </Link>
        </div>
      </header>

      <section aria-labelledby="customer-sections-title" className="space-y-3">
        <h2
          id="customer-sections-title"
          className="text-sm font-semibold text-foreground"
        >
          {copy.next}
        </h2>
        <p className="text-sm text-muted-foreground">{copy.nextBody}</p>
        <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {cards.map((card) => (
            <li key={card.id}>
              <Surface className="flex h-full flex-col rounded-2xl p-5">
                <div className="flex items-start justify-between gap-2">
                  <h3 className="text-base font-semibold text-foreground">
                    {card.title}
                  </h3>
                  <span className="rounded-full bg-muted px-2 py-0.5 text-[10px] font-medium text-muted-foreground">
                    {copy.honest}
                  </span>
                </div>
                <Text role="muted" className="mt-2 flex-1 text-sm">
                  {card.body}
                </Text>
                <div className="mt-4 flex flex-wrap gap-2">
                  <Link
                    href={card.ctaHref}
                    className="min-h-touch inline-flex items-center rounded-lg bg-[#F59E0B] px-3 text-sm font-semibold text-[#0E172A] hover:opacity-95"
                  >
                    {card.cta}
                  </Link>
                </div>
              </Surface>
            </li>
          ))}
        </ul>
      </section>

      <Surface className="rounded-2xl border-[#1D4ED8]/15 bg-[#1D4ED8]/[0.04] p-5">
        <h2 className="text-sm font-semibold text-[#1D4ED8]">{copy.profile}</h2>
        <p className="mt-2 text-sm text-muted-foreground">{copy.profileHint}</p>
        <Link
          href={`${base}/profile`}
          className="mt-3 inline-flex min-h-touch items-center text-sm font-medium text-[#1D4ED8] underline-offset-2 hover:underline"
        >
          {copy.open}
        </Link>
      </Surface>
    </div>
  );
}
