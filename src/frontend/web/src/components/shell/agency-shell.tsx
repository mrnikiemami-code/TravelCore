import Link from "next/link";
import type { ReactNode } from "react";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";
import { AdminShell, type AdminShellProps } from "./admin-shell";

export type AgencyShellProps = {
  locale: AppLocale;
  title?: ReactNode;
  context?: ReactNode;
  breadcrumb?: ReactNode;
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
      catalog: "کاتالوگ فروش",
      bookings: "رزروهای آژانس",
      customers: "مشتریان",
      commission: "کمیسیون",
      settlement: "تسویه",
      users: "کاربران آژانس",
      profile: "پروفایل تجاری",
      public: "بازار عمومی",
      traveler: "فضای مسافر",
    };
  }
  if (locale === "ar") {
    return {
      brand: "TravelCore Agency",
      tagline: "مساحة مبيعات B2B",
      nav: "تنقل الوكالة",
      dashboard: "لوحة المبيعات",
      catalog: "كتالوج البيع",
      bookings: "حجوزات الوكالة",
      customers: "العملاء",
      commission: "العمولة",
      settlement: "التسوية",
      users: "مستخدمو الوكالة",
      profile: "الملف التجاري",
      public: "السوق العام",
      traveler: "مساحة المسافر",
    };
  }
  return {
    brand: "TravelCore Agency",
    tagline: "B2B sales workspace",
    nav: "Agency navigation",
    dashboard: "Sales dashboard",
    catalog: "Sellable catalog",
    bookings: "Agency bookings",
    customers: "Customers",
    commission: "Commission",
    settlement: "Settlement",
    users: "Agency users",
    profile: "Commercial profile",
    public: "Public marketplace",
    traveler: "Traveler space",
  };
}

/**
 * Agency portal shell (TC-P37-T003).
 * B2B sales chrome — distinct from Customer Dashboard and Admin ops.
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
    { href: base, label: c.dashboard, key: "dashboard" },
    { href: `${base}/catalog`, label: c.catalog, key: "catalog" },
    { href: `${base}/bookings`, label: c.bookings, key: "bookings" },
    { href: `${base}/customers`, label: c.customers, key: "customers" },
    { href: `${base}/commission`, label: c.commission, key: "commission" },
    { href: `${base}/settlement`, label: c.settlement, key: "settlement" },
    { href: `${base}/users`, label: c.users, key: "users" },
    { href: `${base}/profile`, label: c.profile, key: "profile" },
  ];

  return (
    <AdminShell
      embedded={embedded}
      className={cn(
        "bg-[linear-gradient(180deg,color-mix(in_oklab,var(--tc-color-accent)_10%,transparent),transparent_180px)] bg-surface-muted",
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
      actions={
        actions ?? (
          <Link
            href={`/${locale}/tours`}
            className="min-h-touch inline-flex items-center rounded-lg bg-accent px-3 text-xs font-semibold text-accent-foreground hover:opacity-95"
          >
            {c.catalog}
          </Link>
        )
      }
      navigation={
        <nav aria-label={c.nav}>
          <ul className="flex flex-col gap-0.5 text-sm">
            {items.map((item) => {
              const isActive =
                item.key === "dashboard"
                  ? active === base || active.endsWith("/agency")
                  : active === item.href || active.startsWith(`${item.href}/`);
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    className={cn(
                      "flex min-h-touch items-center rounded-md px-3 font-medium",
                      isActive
                        ? "bg-accent/20 text-foreground"
                        : "text-foreground hover:bg-surface",
                    )}
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
            <li>
              <Link
                href={`/${locale}/me`}
                className="flex min-h-touch items-center rounded-md px-3 text-muted-foreground hover:bg-surface hover:text-foreground"
              >
                {c.traveler}
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
