import Link from "next/link";
import { Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

export type CustomerSectionId =
  | "bookings"
  | "payments"
  | "documents"
  | "passengers"
  | "notifications"
  | "profile";

/**
 * Customer Dashboard section foundation (TC-P37-T002).
 * Professional empty/honest states — no fake rows.
 */
export function CustomerSectionView({
  locale,
  section,
}: {
  locale: AppLocale;
  section: CustomerSectionId;
}) {
  const content = sectionCopy(locale, section);

  return (
    <div className="flex flex-col gap-5">
      <header>
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#1D4ED8]">
          {content.eyebrow}
        </p>
        <h1 className="mt-2 text-2xl font-semibold tracking-tight text-foreground">
          {content.title}
        </h1>
        <p className="mt-2 max-w-2xl text-sm text-muted-foreground">
          {content.intro}
        </p>
      </header>

      <Surface className="rounded-2xl p-6 sm:p-8">
        <div className="mx-auto flex max-w-lg flex-col items-start gap-3">
          <span className="rounded-full bg-muted px-2.5 py-1 text-[11px] font-medium text-muted-foreground">
            {content.badge}
          </span>
          <Text as="h2" role="heading" className="text-lg font-semibold">
            {content.emptyTitle}
          </Text>
          <Text role="muted">{content.emptyBody}</Text>
          {content.boundary ? (
            <Text role="caption" className="text-muted-foreground">
              {content.boundary}
            </Text>
          ) : null}
          <div className="mt-2 flex flex-wrap gap-2">
            {content.primaryHref ? (
              <Link
                href={content.primaryHref}
                className="min-h-touch inline-flex items-center rounded-lg bg-[#1D4ED8] px-4 text-sm font-semibold text-white hover:bg-[#1E40AF]"
              >
                {content.primaryCta}
              </Link>
            ) : null}
            <Link
              href={`/${locale}/me`}
              className="min-h-touch inline-flex items-center rounded-lg border border-border px-4 text-sm font-medium hover:border-[#1D4ED8]/40"
            >
              {content.back}
            </Link>
          </div>
        </div>
      </Surface>
    </div>
  );
}

function sectionCopy(locale: AppLocale, section: CustomerSectionId) {
  const fa = {
    eyebrow: "فضای مسافر",
    badge: "خالی · صادقانه",
    back: "بازگشت به نمای کلی",
    bookings: {
      title: "رزروها",
      intro: "وضعیت رزرو از Booking خوانده می‌شود — نه از Frontend.",
      emptyTitle: "رزروی برای نمایش نیست",
      emptyBody:
        "وقتی رزرو Pending با دسترسی معتبر داشته باشید، وضعیت دوستانه اینجا دیده می‌شود.",
      boundary: "Booking ≠ Payment · FE ≠ Source of Truth",
      primaryCta: "شروع از تورها",
      primaryHref: `/${locale}/tours`,
    },
    payments: {
      title: "پرداخت‌ها",
      intro: "وضعیت پرداخت فقط از قرارداد Payment می‌آید.",
      emptyTitle: "پرداختی برای نمایش نیست",
      emptyBody:
        "اگر پرداخت سندباکس در دسترس باشد، از صفحه وضعیت رزرو هدایت می‌شوید — اینجا لیست جعلی نمی‌سازیم.",
      boundary: "Payment success ≠ Auto Confirm · browser return ≠ success",
      primaryCta: "مشاهده رزروها",
      primaryHref: `/${locale}/me/bookings`,
    },
    documents: {
      title: "مدارک سفر",
      intro: "مدارک فقط وقتی منتشر/پیوست معتبر باشند نمایش داده می‌شوند.",
      emptyTitle: "مدرکی نیست",
      emptyBody: "هنوز مدرک سفری برای این حساب موجود نیست.",
      boundary: null,
      primaryCta: "بازگشت به بازار",
      primaryHref: `/${locale}`,
    },
    passengers: {
      title: "مسافران",
      intro: "پروفایل مسافران ذخیره‌شده برای رزروهای بعدی.",
      emptyTitle: "مسافر ذخیره‌شده‌ای نیست",
      emptyBody: "پس از اتصال قرارداد ترجیحات/مسافر، اینجا پر می‌شود.",
      boundary: null,
      primaryCta: "نمای کلی",
      primaryHref: `/${locale}/me`,
    },
    notifications: {
      title: "اعلان‌ها",
      intro: "اعلان‌ها از ماژول Notification — بدون هشدار جعلی.",
      emptyTitle: "اعلانی نیست",
      emptyBody: "فعلاً اعلانی برای نمایش وجود ندارد.",
      boundary: null,
      primaryCta: "نمای کلی",
      primaryHref: `/${locale}/me`,
    },
    profile: {
      title: "پروفایل",
      intro: "اطلاعات تماس و ترجیحات سفر.",
      emptyTitle: "پروفایل هنوز متصل نیست",
      emptyBody:
        "بنیاد UI آماده است. داده پروفایل وقتی قرارداد Identity/Preference در دسترس باشد نمایش داده می‌شود.",
      boundary: "Identity ≠ Party ≠ Access",
      primaryCta: "بازار عمومی",
      primaryHref: `/${locale}`,
    },
  };

  const en = {
    eyebrow: "Traveler space",
    badge: "Empty · honest",
    back: "Back to overview",
    bookings: {
      title: "Bookings",
      intro: "Booking status comes from Booking — not from the frontend.",
      emptyTitle: "No bookings to show",
      emptyBody:
        "When you have a Pending booking with valid access, a friendly status view appears here.",
      boundary: "Booking ≠ Payment · FE ≠ Source of Truth",
      primaryCta: "Start from tours",
      primaryHref: `/${locale}/tours`,
    },
    payments: {
      title: "Payments",
      intro: "Payment status comes only from the Payment contract.",
      emptyTitle: "No payments to show",
      emptyBody:
        "If sandbox payment is available, you are guided from the booking status page — we do not invent a payment list here.",
      boundary: "Payment success ≠ Auto Confirm · browser return ≠ success",
      primaryCta: "View bookings",
      primaryHref: `/${locale}/me/bookings`,
    },
    documents: {
      title: "Travel documents",
      intro: "Documents appear only when a valid attachment/publish path exists.",
      emptyTitle: "No documents yet",
      emptyBody: "There are no travel documents for this account yet.",
      boundary: null,
      primaryCta: "Back to marketplace",
      primaryHref: `/${locale}`,
    },
    passengers: {
      title: "Passengers",
      intro: "Saved traveler profiles for future bookings.",
      emptyTitle: "No saved passengers",
      emptyBody: "This fills when preference/passenger contracts are available.",
      boundary: null,
      primaryCta: "Overview",
      primaryHref: `/${locale}/me`,
    },
    notifications: {
      title: "Notifications",
      intro: "Notifications from the Notification module — no fake alerts.",
      emptyTitle: "No notifications",
      emptyBody: "There are no notifications to show right now.",
      boundary: null,
      primaryCta: "Overview",
      primaryHref: `/${locale}/me`,
    },
    profile: {
      title: "Profile",
      intro: "Contact details and travel preferences.",
      emptyTitle: "Profile not wired yet",
      emptyBody:
        "UI foundation is ready. Profile facts appear when Identity/Preference contracts are available.",
      boundary: "Identity ≠ Party ≠ Access",
      primaryCta: "Public marketplace",
      primaryHref: `/${locale}`,
    },
  };

  const ar = {
    eyebrow: "مساحة المسافر",
    badge: "فارغ · بصدق",
    back: "العودة إلى النظرة العامة",
    bookings: {
      title: "الحجوزات",
      intro: "تأتي حالة الحجز من Booking — وليس من الواجهة.",
      emptyTitle: "لا حجوزات للعرض",
      emptyBody:
        "عند وجود حجز Pending بوصول صالح تظهر حالة ودّية هنا.",
      boundary: "Booking ≠ Payment · FE ≠ Source of Truth",
      primaryCta: "ابدأ من الجولات",
      primaryHref: `/${locale}/tours`,
    },
    payments: {
      title: "المدفوعات",
      intro: "تأتي حالة الدفع من عقد Payment فقط.",
      emptyTitle: "لا مدفوعات للعرض",
      emptyBody:
        "إذا كان دفع Sandbox متاحاً يتم التوجيه من صفحة حالة الحجز — بلا قائمة وهمية هنا.",
      boundary: "Payment success ≠ Auto Confirm · browser return ≠ success",
      primaryCta: "عرض الحجوزات",
      primaryHref: `/${locale}/me/bookings`,
    },
    documents: {
      title: "مستندات السفر",
      intro: "تظهر المستندات فقط عند وجود مسار نشر/مرفق صالح.",
      emptyTitle: "لا مستندات بعد",
      emptyBody: "لا مستندات سفر لهذا الحساب حالياً.",
      boundary: null,
      primaryCta: "العودة إلى السوق",
      primaryHref: `/${locale}`,
    },
    passengers: {
      title: "المسافرون",
      intro: "ملفات المسافرين المحفوظة للحجوزات القادمة.",
      emptyTitle: "لا مسافرين محفوظين",
      emptyBody: "تُملأ عند توفر عقود التفضيلات/المسافرين.",
      boundary: null,
      primaryCta: "نظرة عامة",
      primaryHref: `/${locale}/me`,
    },
    notifications: {
      title: "الإشعارات",
      intro: "إشعارات من وحدة Notification — بلا تنبيهات وهمية.",
      emptyTitle: "لا إشعارات",
      emptyBody: "لا إشعارات للعرض حالياً.",
      boundary: null,
      primaryCta: "نظرة عامة",
      primaryHref: `/${locale}/me`,
    },
    profile: {
      title: "الملف",
      intro: "بيانات التواصل وتفضيلات السفر.",
      emptyTitle: "الملف غير مربوط بعد",
      emptyBody:
        "أساس الواجهة جاهز. تظهر بيانات الملف عند توفر عقود Identity/Preference.",
      boundary: "Identity ≠ Party ≠ Access",
      primaryCta: "السوق العام",
      primaryHref: `/${locale}`,
    },
  };

  const pack = locale === "fa" ? fa : locale === "ar" ? ar : en;
  const sectionPack = pack[section];
  return {
    eyebrow: pack.eyebrow,
    badge: pack.badge,
    back: pack.back,
    title: sectionPack.title,
    intro: sectionPack.intro,
    emptyTitle: sectionPack.emptyTitle,
    emptyBody: sectionPack.emptyBody,
    boundary: sectionPack.boundary,
    primaryCta: sectionPack.primaryCta,
    primaryHref: sectionPack.primaryHref,
  };
}
