import Link from "next/link";
import type { ReactNode } from "react";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";
import { AdminShell, type AdminShellProps } from "./admin-shell";

export type AgencyShellProps = {
  locale: AppLocale;
  title?: ReactNode;
  children: ReactNode;
  className?: string;
} & Pick<AdminShellProps, "actions" | "embedded">;

function copy(locale: AppLocale) {
  if (locale === "fa") {
    return {
      brand: "TravelCore Agency",
      nav: "ناوبری آژانس",
      dashboard: "داشبورد فروش",
      offers: "آگهی‌ها",
      publish: "انتشار",
      public: "بازار عمومی",
    };
  }
  if (locale === "ar") {
    return {
      brand: "TravelCore Agency",
      nav: "تنقل الوكالة",
      dashboard: "لوحة المبيعات",
      offers: "العروض",
      publish: "النشر",
      public: "السوق العام",
    };
  }
  return {
    brand: "TravelCore Agency",
    nav: "Agency navigation",
    dashboard: "Sales dashboard",
    offers: "Offers",
    publish: "Publish",
    public: "Public marketplace",
  };
}

/**
 * Agency portal shell — sales-oriented chrome (P30 T004).
 * Distinct from Admin ops density; reuses AdminShell layout mechanics.
 */
export function AgencyShell({
  locale,
  title,
  children,
  actions,
  embedded,
  className,
}: AgencyShellProps) {
  const c = copy(locale);
  const base = `/${locale}/agency`;

  return (
    <AdminShell
      embedded={embedded}
      className={cn("bg-surface-muted", className)}
      header={
        <div className="flex min-w-0 flex-col gap-0.5">
          <span className="text-xs font-medium uppercase tracking-wide text-accent">
            {c.brand}
          </span>
          <div className="truncate text-base font-semibold text-foreground">
            {title ?? c.dashboard}
          </div>
        </div>
      }
      actions={actions}
      navigation={
        <nav aria-label={c.nav}>
          <ul className="flex flex-col gap-1 text-sm">
            <li>
              <Link
                href={base}
                className="min-h-touch flex items-center rounded-md px-2 font-medium text-primary hover:bg-surface"
              >
                {c.dashboard}
              </Link>
            </li>
            <li>
              <Link
                href={base}
                className="min-h-touch flex items-center rounded-md px-2 hover:bg-surface"
              >
                {c.offers}
              </Link>
            </li>
            <li>
              <Link
                href={base}
                className="min-h-touch flex items-center rounded-md px-2 hover:bg-surface"
              >
                {c.publish}
              </Link>
            </li>
            <li className="mt-2 border-t border-border pt-2">
              <Link
                href={`/${locale}`}
                className="min-h-touch flex items-center rounded-md px-2 text-muted-foreground hover:bg-surface hover:text-foreground"
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
