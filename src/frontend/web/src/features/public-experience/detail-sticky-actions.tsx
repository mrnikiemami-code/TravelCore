import { Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

/**
 * P36-T005: Sticky commerce actions on public Tour Detail.
 * Allowed: View Departure · View Price · Contact · optional Prepare booking.
 * Forbidden: Payment · fake Confirmed · inventing money.
 */
export function PublicDetailStickyActions({
  locale,
  bookHref,
}: {
  locale: AppLocale;
  bookHref?: string;
}) {
  const copy =
    locale === "fa"
      ? {
          viewOptions: "تاریخ‌های حرکت",
          viewPrice: "خلاصه قیمت",
          contact: "درخواست اطلاعات",
          prepare: "شروع رزرو موقت",
          note: bookHref
            ? "حرکت + قیمت · سپس رزرو موقت · بدون پرداخت و بدون تأیید قطعی"
            : "نمایش حرکت و قیمت · بدون ایجاد رزرو و بدون پرداخت",
        }
      : locale === "ar"
        ? {
            viewOptions: "تواريخ المغادرة",
            viewPrice: "ملخص السعر",
            contact: "طلب معلومات",
            prepare: "بدء حجز مؤقت",
            note: bookHref
              ? "المغادرة + السعر · ثم حجز مؤقت · بلا دفع وبلا تأكيد"
              : "عرض المغادرة والسعر · بلا إنشاء حجز وبلا دفع",
          }
        : {
            viewOptions: "Departures",
            viewPrice: "Price summary",
            contact: "Ask a question",
            prepare: "Start pending booking",
            note: bookHref
              ? "Departure + price · then Pending booking · no payment · not Confirmed"
              : "Browse departure + price · no booking create · no payment",
          };

  return (
    <div className="pointer-events-none fixed inset-x-0 bottom-0 z-40 p-3 lg:pointer-events-auto lg:inset-x-auto lg:bottom-8 lg:end-8 lg:w-80">
      <nav
        aria-label={locale === "fa" ? "اقدام‌های صفحه تور" : "Tour page actions"}
        className="pointer-events-auto rounded-2xl border border-border bg-background/95 p-3.5 shadow-xl backdrop-blur"
      >
        <Text role="caption" className="text-[11px] leading-snug">
          {copy.note}
        </Text>
        <ul className="mt-2.5 grid grid-cols-2 gap-2 lg:grid-cols-1">
          <li>
            <a
              className="min-h-touch inline-flex w-full items-center justify-center rounded-lg border border-border px-3 py-2 text-sm font-medium hover:border-[#1D4ED8]/40"
              href="#published-departures"
            >
              {copy.viewOptions}
            </a>
          </li>
          <li>
            <a
              className="min-h-touch inline-flex w-full items-center justify-center rounded-lg border border-border px-3 py-2 text-sm font-medium hover:border-[#1D4ED8]/40"
              href="#price-from"
            >
              {copy.viewPrice}
            </a>
          </li>
          <li>
            <a
              className="min-h-touch inline-flex w-full items-center justify-center rounded-lg border border-border px-3 py-2 text-sm font-medium hover:border-[#1D4ED8]/40"
              href="#request-information"
            >
              {copy.contact}
            </a>
          </li>
          {bookHref ? (
            <li>
              <a
                className="min-h-touch inline-flex w-full items-center justify-center rounded-lg bg-[#F59E0B] px-3 py-2 text-sm font-semibold text-[#0E172A] hover:opacity-95"
                href={bookHref}
              >
                {copy.prepare}
              </a>
            </li>
          ) : null}
        </ul>
      </nav>
    </div>
  );
}
