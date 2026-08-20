import Link from "next/link";
import type { AppLocale } from "@/lib/i18n";

const links = (locale: AppLocale) => [
  { href: `/${locale}/admin/operations`, labelFa: "عملیات", labelEn: "Operations", labelAr: "العمليات" },
  { href: `/${locale}/admin/catalog`, labelFa: "کاتالوگ", labelEn: "Catalog", labelAr: "الكتالوج" },
  { href: `/${locale}/admin/catalog/places`, labelFa: "مکان‌ها", labelEn: "Places", labelAr: "الأماكن" },
  { href: `/${locale}/admin/catalog/tours`, labelFa: "تورها", labelEn: "Tours", labelAr: "الجولات" },
  { href: `/${locale}/admin/catalog/departures`, labelFa: "حرکت‌ها", labelEn: "Departures", labelAr: "المغادرات" },
  { href: `/${locale}/admin/media`, labelFa: "رسانه", labelEn: "Media", labelAr: "الوسائط" },
  { href: `/${locale}/admin/accounts`, labelFa: "حساب‌ها", labelEn: "Accounts", labelAr: "الحسابات" },
  { href: `/${locale}/admin/ugc/moderation`, labelFa: "نظارت UGC", labelEn: "UGC moderation", labelAr: "مراجعة UGC" },
];

/**
 * Shared Admin navigation — T008 operational console.
 */
export function AdminNav({
  locale,
  currentPath,
}: {
  locale: AppLocale;
  currentPath?: string;
}) {
  const items = links(locale);
  const aria =
    locale === "fa" ? "ناوبری ادمین" : locale === "ar" ? "تنقل الإدارة" : "Admin navigation";

  return (
    <nav aria-label={aria}>
      <ul className="flex flex-col gap-0.5 text-sm">
        {items.map((item) => {
          const label =
            locale === "fa"
              ? item.labelFa
              : locale === "ar"
                ? item.labelAr
                : item.labelEn;
          const active = currentPath != null && currentPath.startsWith(item.href);
          return (
            <li key={item.href}>
              <Link
                href={item.href}
                className={
                  active
                    ? "flex min-h-touch items-center rounded-md bg-primary/10 px-3 font-medium text-primary"
                    : "flex min-h-touch items-center rounded-md px-3 text-foreground hover:bg-surface-muted"
                }
                aria-current={active ? "page" : undefined}
              >
                {label}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}
