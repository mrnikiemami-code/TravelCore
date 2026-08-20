import Link from "next/link";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";

export type PublicFooterProps = {
  locale: AppLocale;
  className?: string;
};

function copy(locale: AppLocale) {
  if (locale === "fa") {
    return {
      brand: "TravelCore",
      blurb: "پلتفرم گردشگری حرفه‌ای — کشف، اعتماد، رزرو.",
      tours: "تور",
      hotels: "هتل",
      destinations: "مقاصد",
      stories: "سفرنامه",
      legal: "استفاده منصفانه · بدون داده جعلی تجارت",
    };
  }
  if (locale === "ar") {
    return {
      brand: "TravelCore",
      blurb: "منصة سفر احترافية — اكتشف، ثق، احجز.",
      tours: "جولات",
      hotels: "فنادق",
      destinations: "وجهات",
      stories: "رحلات",
      legal: "استخدام عادل · بدون بيانات تجارة وهمية",
    };
  }
  return {
    brand: "TravelCore",
    blurb: "Professional travel commerce — Discover, Trust, Book.",
    tours: "Tours",
    hotels: "Hotels",
    destinations: "Destinations",
    stories: "Stories",
    legal: "Honest surfaces · no fake commerce facts",
  };
}

/**
 * Public marketplace footer chrome — P30 T004.
 */
export function PublicFooter({ locale, className }: PublicFooterProps) {
  const c = copy(locale);
  const base = `/${locale}`;

  return (
    <div
      className={cn(
        "grid gap-6 sm:grid-cols-2 lg:grid-cols-[1.4fr_1fr_1fr]",
        className,
      )}
    >
      <div className="space-y-2">
        <p className="text-base font-semibold text-primary">{c.brand}</p>
        <p className="text-sm text-muted-foreground">{c.blurb}</p>
      </div>

      <div>
        <ul className="flex flex-col gap-2 text-sm">
          <li>
            <Link className="min-h-touch inline-flex items-center hover:text-primary" href={`${base}/tours`}>
              {c.tours}
            </Link>
          </li>
          <li>
            <Link className="min-h-touch inline-flex items-center hover:text-primary" href={`${base}/hotels`}>
              {c.hotels}
            </Link>
          </li>
          <li>
            <Link
              className="min-h-touch inline-flex items-center hover:text-primary"
              href={`${base}/destinations`}
            >
              {c.destinations}
            </Link>
          </li>
          <li>
            <Link
              className="min-h-touch inline-flex items-center hover:text-primary"
              href={`${base}/travelogues`}
            >
              {c.stories}
            </Link>
          </li>
        </ul>
      </div>

      <div className="text-xs text-muted-foreground sm:text-end">{c.legal}</div>
    </div>
  );
}
