import { Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

/**
 * P14-R2 / P33-T006 / P33-T007: Sticky actions on public Tour Detail.
 * Allowed: View Departure · View Price · Contact · optional Prepare booking (I3).
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
          viewOptions: "مشاهده اجراها",
          viewPrice: "مشاهده قیمت",
          contact: "درخواست اطلاعات",
          prepare: "شروع رزرو موقت",
          note: bookHref
            ? "انتخاب حرکت + قیمت · سپس رزرو Pending · نه پرداخت · نه Confirmed"
            : "اقدام نمایشی · انتخاب حرکت + خلاصه قیمت · نه ایجاد رزرو · نه پرداخت",
        }
      : locale === "ar"
        ? {
            viewOptions: "عرض الرحلات",
            viewPrice: "عرض السعر",
            contact: "طلب معلومات",
            prepare: "إعداد حجز مؤقت",
            note: bookHref
              ? "اختيار المغادرة + السعر · ثم حجز Pending · بلا دفع · بلا تأكيد"
              : "إجراءات العرض · اختيار المغادرة + ملخص السعر · بلا إنشاء حجز · بلا دفع",
          }
        : {
            viewOptions: "View departures",
            viewPrice: "View price",
            contact: "Request information",
            prepare: "Prepare booking",
            note: bookHref
              ? "Departure + price · then Pending booking · no payment · not Confirmed"
              : "Presentation · departure + price summary · no booking create · no payment",
          };

  return (
    <div className="pointer-events-none fixed inset-x-0 bottom-0 z-40 p-3 lg:pointer-events-auto lg:inset-x-auto lg:bottom-8 lg:end-8 lg:w-72">
      <nav
        aria-label={locale === "fa" ? "اقدام‌های صفحه تور" : "Tour page actions"}
        className="pointer-events-auto rounded-lg border border-border bg-background/95 p-3 shadow-lg backdrop-blur"
      >
        <Text role="caption">{copy.note}</Text>
        <ul className="mt-2 flex flex-row gap-2 lg:flex-col">
          <li className="flex-1">
            <a
              className="min-h-touch inline-flex w-full items-center justify-center rounded-md border border-border px-3 py-2 text-sm underline-offset-2 hover:underline"
              href="#published-departures"
            >
              {copy.viewOptions}
            </a>
          </li>
          <li className="flex-1">
            <a
              className="min-h-touch inline-flex w-full items-center justify-center rounded-md border border-border px-3 py-2 text-sm underline-offset-2 hover:underline"
              href="#price-from"
            >
              {copy.viewPrice}
            </a>
          </li>
          <li className="flex-1">
            <a
              className="min-h-touch inline-flex w-full items-center justify-center rounded-md border border-border px-3 py-2 text-sm underline-offset-2 hover:underline"
              href="#request-information"
            >
              {copy.contact}
            </a>
          </li>
          {bookHref ? (
            <li className="flex-1">
              <a
                className="min-h-touch inline-flex w-full items-center justify-center rounded-md bg-accent px-3 py-2 text-sm font-semibold text-accent-foreground hover:opacity-95"
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
