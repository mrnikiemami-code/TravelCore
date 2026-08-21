import Link from "next/link";
import type { ReactNode } from "react";
import type { AppLocale } from "@/lib/i18n";
import { cn } from "@/lib/ui/cn";
import { AdminShell, type AdminShellProps } from "./admin-shell";

export type CustomerShellProps = {
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
      brand: "TravelCore Traveler",
      tagline: "سفرهای من",
      nav: "ناوبری مسافر",
      overview: "نمای کلی",
      trips: "سفرها",
      bookings: "رزروها",
      payments: "پرداخت‌ها",
      documents: "مدارک",
      passengers: "مسافران",
      notifications: "اعلان‌ها",
      profile: "پروفایل",
      marketplace: "بازار عمومی",
    };
  }
  if (locale === "ar") {
    return {
      brand: "TravelCore Traveler",
      tagline: "رحلاتي",
      nav: "تنقل المسافر",
      overview: "نظرة عامة",
      trips: "الرحلات",
      bookings: "الحجوزات",
      payments: "المدفوعات",
      documents: "المستندات",
      passengers: "المسافرون",
      notifications: "الإشعارات",
      profile: "الملف",
      marketplace: "السوق العام",
    };
  }
  return {
    brand: "TravelCore Traveler",
    tagline: "My trips",
    nav: "Traveler navigation",
    overview: "Overview",
    trips: "My trips",
    bookings: "Bookings",
    payments: "Payments",
    documents: "Documents",
    passengers: "Passengers",
    notifications: "Notifications",
    profile: "Profile",
    marketplace: "Public marketplace",
  };
}

/**
 * Customer Dashboard shell (TC-P37-T002).
 * Consumer product chrome — distinct from Agency sales and Admin ops.
 */
export function CustomerShell({
  locale,
  title,
  context,
  breadcrumb,
  currentPath,
  children,
  actions,
  embedded,
  className,
}: CustomerShellProps) {
  const c = copy(locale);
  const base = `/${locale}/me`;
  const active = currentPath ?? base;

  const items = [
    { href: base, label: c.overview, key: "overview" },
    { href: `${base}/bookings`, label: c.bookings, key: "bookings" },
    { href: `${base}/payments`, label: c.payments, key: "payments" },
    { href: `${base}/documents`, label: c.documents, key: "documents" },
    { href: `${base}/passengers`, label: c.passengers, key: "passengers" },
    { href: `${base}/notifications`, label: c.notifications, key: "notifications" },
    { href: `${base}/profile`, label: c.profile, key: "profile" },
  ];

  return (
    <AdminShell
      embedded={embedded}
      className={cn(
        "bg-[linear-gradient(180deg,color-mix(in_oklab,#1D4ED8_7%,transparent),transparent_200px)] bg-surface-muted",
        className,
      )}
      header={
        <div className="flex min-w-0 flex-col gap-0.5">
          <span className="text-[11px] font-semibold uppercase tracking-[0.14em] text-[#1D4ED8]">
            {c.brand}
          </span>
          <div className="truncate text-base font-semibold text-foreground">
            {title ?? c.trips}
          </div>
        </div>
      }
      context={context ?? c.tagline}
      breadcrumb={breadcrumb}
      actions={
        actions ?? (
          <Link
            href={`/${locale}/tours`}
            className="min-h-touch inline-flex items-center rounded-lg bg-[#F59E0B] px-3 text-xs font-semibold text-[#0E172A] hover:opacity-95"
          >
            {c.marketplace}
          </Link>
        )
      }
      navigation={
        <nav aria-label={c.nav}>
          <ul className="flex flex-col gap-0.5 text-sm">
            {items.map((item) => {
              const isActive =
                item.key === "overview"
                  ? active === base || active.endsWith("/me")
                  : active === item.href || active.startsWith(`${item.href}/`);
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    className={cn(
                      "min-h-touch flex items-center rounded-lg px-3 py-2 font-medium",
                      isActive
                        ? "bg-[#1D4ED8]/10 text-[#1D4ED8]"
                        : "text-foreground hover:bg-surface",
                    )}
                    aria-current={isActive ? "page" : undefined}
                  >
                    {item.label}
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>
      }
    >
      {children}
    </AdminShell>
  );
}
