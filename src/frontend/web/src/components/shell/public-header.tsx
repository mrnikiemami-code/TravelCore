import Link from "next/link";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";

export type PublicHeaderProps = {
  locale: AppLocale;
  className?: string;
};

type NavItem = { href: string; label: string };

function navItems(locale: AppLocale): NavItem[] {
  const base = `/${locale}`;
  if (locale === "fa") {
    return [
      { href: base, label: "خانه" },
      { href: `${base}/tours`, label: "تور" },
      { href: `${base}/hotels`, label: "هتل" },
      { href: `${base}/destinations`, label: "مقاصد" },
      { href: `${base}/travelogues`, label: "سفرنامه" },
    ];
  }
  if (locale === "ar") {
    return [
      { href: base, label: "الرئيسية" },
      { href: `${base}/tours`, label: "جولات" },
      { href: `${base}/hotels`, label: "فنادق" },
      { href: `${base}/destinations`, label: "وجهات" },
      { href: `${base}/travelogues`, label: "رحلات" },
    ];
  }
  return [
    { href: base, label: "Home" },
    { href: `${base}/tours`, label: "Tours" },
    { href: `${base}/hotels`, label: "Hotels" },
    { href: `${base}/destinations`, label: "Destinations" },
    { href: `${base}/travelogues`, label: "Stories" },
  ];
}

function copy(locale: AppLocale) {
  if (locale === "fa") {
    return {
      brand: "TravelCore",
      tagline: "کشف · اعتماد · رزرو",
      search: "جستجو",
      account: "میز کار",
      myTrips: "سفرهای من",
      menu: "منو",
      navLabel: "ناوبری اصلی",
    };
  }
  if (locale === "ar") {
    return {
      brand: "TravelCore",
      tagline: "اكتشف · ثق · احجز",
      search: "بحث",
      account: "مساحة العمل",
      myTrips: "رحلاتي",
      menu: "القائمة",
      navLabel: "التنقل الرئيسي",
    };
  }
  return {
    brand: "TravelCore",
    tagline: "Discover · Trust · Book",
    search: "Search",
    account: "Workspace",
    myTrips: "My trips",
    menu: "Menu",
    navLabel: "Primary",
  };
}

/**
 * Public marketplace header chrome — P30 T004.
 * Server Component · direction-neutral · mobile-first (details/summary menu).
 */
export function PublicHeader({ locale, className }: PublicHeaderProps) {
  const c = copy(locale);
  const items = navItems(locale);

  return (
    <div className={cn("flex flex-col gap-3", className)}>
      <div className="flex items-center justify-between gap-3">
        <div className="min-w-0">
          <Link
            href={`/${locale}`}
            className="inline-flex items-baseline gap-2 rounded-md focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
          >
            <span className="text-lg font-semibold tracking-tight text-primary">
              {c.brand}
            </span>
            <span className="hidden text-xs text-muted-foreground sm:inline">
              {c.tagline}
            </span>
          </Link>
        </div>

        <div className="flex items-center gap-2">
          <Link
            href={`/${locale}/plan`}
            className="hidden min-h-touch items-center rounded-md border border-border bg-surface px-3 text-sm text-foreground hover:bg-surface-muted sm:inline-flex"
          >
            {c.search}
          </Link>
          <Link
            href={`/${locale}/me`}
            className="min-h-touch inline-flex items-center rounded-md border border-border bg-surface px-3 text-sm font-medium text-foreground hover:bg-surface-muted"
          >
            {c.myTrips}
          </Link>
          <Link
            href={`/${locale}/admin/catalog`}
            className="min-h-touch inline-flex items-center rounded-md bg-accent px-3 text-sm font-medium text-accent-foreground hover:opacity-90"
          >
            {c.account}
          </Link>

          <details className="relative md:hidden">
            <summary className="min-h-touch list-none inline-flex cursor-pointer items-center rounded-md border border-border bg-surface px-3 text-sm marker:content-none [&::-webkit-details-marker]:hidden">
              {c.menu}
            </summary>
            <nav
              aria-label={c.navLabel}
              className="absolute end-0 z-40 mt-2 w-56 rounded-lg border border-border bg-surface p-2 shadow-md"
            >
              <ul className="flex flex-col gap-1">
                {items.map((item) => (
                  <li key={item.href}>
                    <Link
                      href={item.href}
                      className="min-h-touch flex items-center rounded-md px-3 text-sm hover:bg-surface-muted"
                    >
                      {item.label}
                    </Link>
                  </li>
                ))}
              </ul>
            </nav>
          </details>
        </div>
      </div>

      <nav aria-label={c.navLabel} className="hidden md:block">
        <ul className="flex flex-wrap items-center gap-1">
          {items.map((item) => (
            <li key={item.href}>
              <Link
                href={item.href}
                className="min-h-touch inline-flex items-center rounded-md px-3 text-sm font-medium text-foreground hover:bg-surface-muted hover:text-primary"
              >
                {item.label}
              </Link>
            </li>
          ))}
        </ul>
      </nav>
    </div>
  );
}
