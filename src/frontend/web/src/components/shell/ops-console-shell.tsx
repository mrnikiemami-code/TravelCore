import Link from "next/link";
import type { ReactNode } from "react";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";
import { AdminShell, type AdminShellProps } from "./admin-shell";

export type OpsConsoleShellProps = {
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
      brand: "TravelCore Ops",
      tagline: "کنسول عملیاتی",
      nav: "ناوبری ادمین",
      dashboard: "داشبورد عملیات",
      catalog: "عملیات کاتالوگ",
      content: "محتوا و رسانه",
      agencies: "مدیریت آژانس",
      access: "کاربر و دسترسی",
      reporting: "گزارش‌ها",
      audit: "ممیزی و گردش‌کار",
      profile: "پروفایل اپراتور",
      public: "بازار عمومی",
      agency: "پورتال آژانس",
      traveler: "فضای مسافر",
    };
  }
  if (locale === "ar") {
    return {
      brand: "TravelCore Ops",
      tagline: "وحدة تشغيلية",
      nav: "تنقل الإدارة",
      dashboard: "لوحة العمليات",
      catalog: "عمليات الكتالوج",
      content: "المحتوى والوسائط",
      agencies: "إدارة الوكالات",
      access: "المستخدم والوصول",
      reporting: "التقارير",
      audit: "التدقيق وسير العمل",
      profile: "ملف المشغّل",
      public: "السوق العام",
      agency: "بوابة الوكالة",
      traveler: "مساحة المسافر",
    };
  }
  return {
    brand: "TravelCore Ops",
    tagline: "Operational console",
    nav: "Admin navigation",
    dashboard: "Operations dashboard",
    catalog: "Catalog operations",
    content: "Content & media",
    agencies: "Agency management",
    access: "Users & access",
    reporting: "Reporting",
    audit: "Audit & workflow",
    profile: "Operator profile",
    public: "Public marketplace",
    agency: "Agency portal",
    traveler: "Traveler space",
  };
}

/**
 * Admin Console shell (TC-P37-T004).
 * Operational chrome — distinct from Customer Dashboard and Agency Portal.
 */
export function OpsConsoleShell({
  locale,
  title,
  context,
  breadcrumb,
  currentPath,
  children,
  actions,
  embedded,
  className,
}: OpsConsoleShellProps) {
  const c = copy(locale);
  const base = `/${locale}/admin`;
  const active = currentPath ?? base;

  const items = [
    { href: base, label: c.dashboard, key: "dashboard" },
    { href: `${base}/catalog-ops`, label: c.catalog, key: "catalog" },
    { href: `${base}/content`, label: c.content, key: "content" },
    { href: `${base}/agencies`, label: c.agencies, key: "agencies" },
    { href: `${base}/access`, label: c.access, key: "access" },
    { href: `${base}/reporting`, label: c.reporting, key: "reporting" },
    { href: `${base}/audit`, label: c.audit, key: "audit" },
    { href: `${base}/profile`, label: c.profile, key: "profile" },
  ];

  return (
    <AdminShell
      embedded={embedded}
      className={cn(
        "bg-[linear-gradient(180deg,color-mix(in_oklab,var(--tc-color-primary)_8%,transparent),transparent_160px)] bg-surface-muted",
        className,
      )}
      header={
        <div className="flex min-w-0 flex-col gap-0.5">
          <span className="text-[11px] font-semibold uppercase tracking-[0.14em] text-primary">
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
            href={`${base}/catalog-ops`}
            className="min-h-touch inline-flex items-center rounded-lg bg-primary px-3 text-xs font-semibold text-primary-foreground hover:opacity-95"
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
                  ? active === base ||
                    active.endsWith("/admin") ||
                    active.includes("/admin/operations")
                  : active === item.href || active.startsWith(`${item.href}/`);
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    className={cn(
                      "flex min-h-touch items-center rounded-md px-3 font-medium",
                      isActive
                        ? "bg-primary/15 text-primary"
                        : "text-foreground hover:bg-surface-muted",
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
                className="flex min-h-touch items-center rounded-md px-3 text-muted-foreground hover:bg-surface-muted hover:text-foreground"
              >
                {c.public}
              </Link>
            </li>
            <li>
              <Link
                href={`/${locale}/agency`}
                className="flex min-h-touch items-center rounded-md px-3 text-muted-foreground hover:bg-surface-muted hover:text-foreground"
              >
                {c.agency}
              </Link>
            </li>
            <li>
              <Link
                href={`/${locale}/me`}
                className="flex min-h-touch items-center rounded-md px-3 text-muted-foreground hover:bg-surface-muted hover:text-foreground"
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
