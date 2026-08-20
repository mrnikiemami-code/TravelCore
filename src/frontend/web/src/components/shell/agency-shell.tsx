import Link from "next/link";
import type { ReactNode } from "react";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";
import { AdminShell, type AdminShellProps } from "./admin-shell";

export type AgencyShellProps = {
  locale: AppLocale;
  title?: ReactNode;
  /** Sales-context subtitle under brand. */
  context?: ReactNode;
  breadcrumb?: ReactNode;
  /** Highlight active nav path (e.g. /fa/agency). */
  currentPath?: string;
  children: ReactNode;
  className?: string;
} & Pick<AdminShellProps, "actions" | "embedded">;

function copy(locale: AppLocale) {
  if (locale === "fa") {
    return {
      brand: "TravelCore Agency",
      tagline: "فضای فروش B2B",
      nav: "ناوبری آژانس",
      dashboard: "داشبورد فروش",
      sales: "فروش",
      bookings: "رزروها",
      customers: "مشتریان",
      requests: "درخواست‌ها",
      offers: "آگهی‌ها",
      public: "بازار عمومی",
    };
  }
  if (locale === "ar") {
    return {
      brand: "TravelCore Agency",
      tagline: "مساحة مبيعات B2B",
      nav: "تنقل الوكالة",
      dashboard: "لوحة المبيعات",
      sales: "المبيعات",
      bookings: "الحجوزات",
      customers: "العملاء",
      requests: "الطلبات",
      offers: "العروض",
      public: "السوق العام",
    };
  }
  return {
    brand: "TravelCore Agency",
    tagline: "B2B sales workspace",
    nav: "Agency navigation",
    dashboard: "Sales dashboard",
    sales: "Sales",
    bookings: "Bookings",
    customers: "Customers",
    requests: "Requests",
    offers: "Offers",
    public: "Public marketplace",
  };
}

/**
 * Agency portal shell — sales workspace chrome (P30 T004 · refined T009).
 * Distinct from Admin ops density and Public marketplace; same design system.
 */
export function AgencyShell({
  locale,
  title,
  context,
  breadcrumb,
  currentPath,
  children,
  actions,
  embedded,
  className,
}: AgencyShellProps) {
  const c = copy(locale);
  const base = `/${locale}/agency`;
  const active = currentPath ?? base;

  const items = [
    { href: base, label: c.dashboard, match: base },
    { href: `${base}#sales`, label: c.sales, match: "#sales" },
    { href: `${base}#bookings`, label: c.bookings, match: "#bookings" },
    { href: `${base}#customers`, label: c.customers, match: "#customers" },
    { href: `${base}#requests`, label: c.requests, match: "#requests" },
    { href: `${base}#offers`, label: c.offers, match: "#offers" },
  ];

  return (
    <AdminShell
      embedded={embedded}
      className={cn(
        "bg-[linear-gradient(180deg,color-mix(in_oklab,var(--tc-color-accent)_8%,transparent),transparent_180px)] bg-surface-muted",
        className,
      )}
      header={
        <div className="flex min-w-0 flex-col gap-0.5">
          <span className="text-[11px] font-semibold uppercase tracking-[0.14em] text-accent">
            {c.brand}
          </span>
          <div className="truncate text-base font-semibold text-foreground">
            {title ?? c.dashboard}
          </div>
        </div>
      }
      context={context ?? c.tagline}
      breadcrumb={breadcrumb}
      actions={actions}
      navigation={
        <nav aria-label={c.nav}>
          <ul className="flex flex-col gap-0.5 text-sm">
            {items.map((item) => {
              const isActive =
                item.match === base
                  ? active === base || active.endsWith("/agency")
                  : false;
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    className={
                      isActive
                        ? "flex min-h-touch items-center rounded-md bg-accent/20 px-3 font-medium text-foreground"
                        : "flex min-h-touch items-center rounded-md px-3 text-foreground hover:bg-surface"
                    }
                    aria-current={isActive ? "page" : undefined}
                  >
                    {item.label}
                  </Link>
                </li>
              );
            })}
            <li className="mt-2 border-t border-border pt-2">
              <Link
                href={`/${locale}`}
                className="flex min-h-touch items-center rounded-md px-3 text-muted-foreground hover:bg-surface hover:text-foreground"
              >
                {c.public}
              </Link>
            </li>
          </ul>
        </nav>
      }
    >
      {children}
    </AdminShell>
  );
}
