import Link from "next/link";
import { Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

/**
 * Agency Portal dashboard foundation (TC-P30-T009).
 * Sales-tool feeling · honest empty states · no invented metrics.
 */
export function AgencyDashboardFoundation({ locale }: { locale: AppLocale }) {
  const copy =
    locale === "fa"
      ? {
          feeling: "این ابزار فروش است.",
          intro:
            "فضای کاری آژانس برای پیگیری فروش، رزرو، مشتری و درخواست‌ها — بدون عدد جعلی.",
          sales: "نمای فروش",
          salesEmpty: "هنوز خلاصه فروش متصل به داده زنده نیست.",
          bookings: "نمای رزرو",
          bookingsEmpty: "رزروهای آژانس وقتی قرارداد آماده باشد اینجا نمایش داده می‌شود.",
          customers: "نمای مشتری",
          customersEmpty: "فهرست مشتری زنده در این لایه موجود نیست.",
          requests: "اقدام لازم",
          requestsEmpty: "درخواست معلقی برای نمایش نیست.",
          offers: "آگهی‌های فروش",
          offersHint: "مدیریت Offer از مسیر Marketplace موجود",
          openOffers: "رفتن به کاتالوگ عمومی هتل/تور",
          shortcuts: "میان‌برهای عملیاتی",
          shortcutProfile: "پروفایل تجاری",
          shortcutPublish: "بازبینی انتشار",
          shortcutPublic: "بازار عمومی",
          status: "وضعیت عملیاتی",
          statusBody:
            "وضعیت‌های تجاری فقط وقتی دادهٔ معتبر ماژول موجود باشد نشان داده می‌شوند. کمیسیون/درآمد جعلی نمایش داده نمی‌شود.",
          emptyBadge: "خالی · صادقانه",
        }
      : locale === "ar"
        ? {
            feeling: "هذه أداة مبيعات.",
            intro:
              "مساحة عمل الوكالة لمتابعة المبيعات والحجوزات والعملاء والطلبات — دون أرقام وهمية.",
            sales: "نظرة المبيعات",
            salesEmpty: "ملخص المبيعات غير متصل ببيانات حية بعد.",
            bookings: "نظرة الحجوزات",
            bookingsEmpty: "ستظهر حجوزات الوكالة عند جاهزية العقد.",
            customers: "نظرة العملاء",
            customersEmpty: "لا قائمة عملاء حية في هذه الطبقة.",
            requests: "إجراء مطلوب",
            requestsEmpty: "لا طلبات معلّقة للعرض.",
            offers: "عروض المبيعات",
            offersHint: "إدارة العروض عبر مسار Marketplace الحالي",
            openOffers: "إلى السوق العام للفنادق/الجولات",
            shortcuts: "اختصارات تشغيلية",
            shortcutProfile: "الملف التجاري",
            shortcutPublish: "مراجعة النشر",
            shortcutPublic: "السوق العام",
            status: "الحالة التشغيلية",
            statusBody:
              "تُعرض الحالات التجارية فقط عند توفر بيانات وحدة موثوقة. لا عمولات/إيرادات وهمية.",
            emptyBadge: "فارغ · بصدق",
          }
        : {
            feeling: "This is a sales tool.",
            intro:
              "Agency workspace for sales, bookings, customers, and requests — no invented numbers.",
            sales: "Sales overview",
            salesEmpty: "Live sales summary is not wired yet.",
            bookings: "Booking overview",
            bookingsEmpty: "Agency bookings appear here when the contract is ready.",
            customers: "Customer overview",
            customersEmpty: "No live customer list on this layer yet.",
            requests: "Action required",
            requestsEmpty: "No pending requests to show.",
            offers: "Sales offers",
            offersHint: "Offer management via existing Marketplace paths",
            openOffers: "Open public hotel/tour marketplace",
            shortcuts: "Action shortcuts",
            shortcutProfile: "Commercial profile",
            shortcutPublish: "Publication review",
            shortcutPublic: "Public marketplace",
            status: "Operational status",
            statusBody:
              "Commercial statuses appear only when authoritative module data exists. No fake commissions or revenue.",
            emptyBadge: "Empty · honest",
          };

  const overview = [
    { id: "sales", title: copy.sales, empty: copy.salesEmpty },
    { id: "bookings", title: copy.bookings, empty: copy.bookingsEmpty },
    { id: "customers", title: copy.customers, empty: copy.customersEmpty },
    { id: "requests", title: copy.requests, empty: copy.requestsEmpty },
  ] as const;

  return (
    <div className="flex flex-col gap-4 p-3 sm:gap-5 sm:p-4">
      <Surface className="border-accent/30 bg-gradient-to-br from-accent/15 via-surface to-surface p-4 sm:p-5">
        <p className="text-lg font-semibold tracking-tight text-foreground sm:text-xl">
          {copy.feeling}
        </p>
        <Text role="muted" className="mt-1 text-sm">
          {copy.intro}
        </Text>
      </Surface>

      <ul className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {overview.map((item) => (
          <li key={item.id} id={item.id}>
            <Surface className="flex h-full flex-col gap-2 p-4">
              <div className="flex items-center justify-between gap-2">
                <Text as="h2" role="label">
                  {item.title}
                </Text>
                <span className="rounded-full bg-surface-muted px-2 py-0.5 text-[10px] text-muted-foreground">
                  {copy.emptyBadge}
                </span>
              </div>
              <Text role="muted" className="text-sm">
                {item.empty}
              </Text>
              <div
                className="mt-auto h-16 rounded-md border border-dashed border-border bg-surface-muted/40"
                aria-hidden
              />
            </Surface>
          </li>
        ))}
      </ul>

      <section id="offers" className="grid grid-cols-1 gap-3 lg:grid-cols-3">
        <Surface className="p-4 lg:col-span-2">
          <Text as="h2" role="heading">
            {copy.offers}
          </Text>
          <Text role="muted" className="mt-1 text-sm">
            {copy.offersHint}
          </Text>
          <div className="mt-4 flex flex-wrap gap-2">
            <Link
              href={`/${locale}/tours`}
              className="min-h-touch inline-flex items-center rounded-md bg-accent px-3 text-sm font-semibold text-accent-foreground hover:opacity-95"
            >
              {copy.openOffers}
            </Link>
            <Link
              href={`/${locale}/hotels`}
              className="min-h-touch inline-flex items-center rounded-md border border-border bg-surface px-3 text-sm hover:bg-surface-muted"
            >
              Hotels
            </Link>
          </div>
        </Surface>

        <Surface className="p-4">
          <Text as="h2" role="heading">
            {copy.shortcuts}
          </Text>
          <ul className="mt-3 flex flex-col gap-2 text-sm">
            <li>
              <span className="font-medium">{copy.shortcutProfile}</span>
              <Text role="caption" className="block text-muted-foreground">
                Agency Marketplace profile · when authorized
              </Text>
            </li>
            <li>
              <span className="font-medium">{copy.shortcutPublish}</span>
              <Text role="caption" className="block text-muted-foreground">
                Offer publish cycle · existing module ownership
              </Text>
            </li>
            <li>
              <Link
                href={`/${locale}`}
                className="font-medium text-primary underline-offset-2 hover:underline"
              >
                {copy.shortcutPublic}
              </Link>
            </li>
          </ul>
        </Surface>
      </section>

      <Surface className="p-4">
        <Text as="h2" role="heading">
          {copy.status}
        </Text>
        <Text role="muted" className="mt-2 text-sm">
          {copy.statusBody}
        </Text>
        <ul className="mt-3 grid grid-cols-1 gap-2 sm:grid-cols-3">
          {["Queue", "Follow-up", "Blocked"].map((label) => (
            <li
              key={label}
              className="rounded-md border border-dashed border-border bg-surface-muted/50 px-3 py-3 text-center text-xs text-muted-foreground"
            >
              {label} · {copy.emptyBadge}
            </li>
          ))}
        </ul>
      </Surface>
    </div>
  );
}
