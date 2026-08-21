import Link from "next/link";
import type { AppLocale } from "@/lib/i18n";

const links = (locale: AppLocale) => [
  {
    href: `/${locale}/admin`,
    labelFa: "داشبورد عملیات",
    labelEn: "Operations dashboard",
    labelAr: "لوحة العمليات",
  },
  {
    href: `/${locale}/admin/catalog-ops`,
    labelFa: "عملیات کاتالوگ",
    labelEn: "Catalog operations",
    labelAr: "عمليات الكتالوج",
  },
  {
    href: `/${locale}/admin/content`,
    labelFa: "محتوا و رسانه",
    labelEn: "Content & media",
    labelAr: "المحتوى والوسائط",
  },
  {
    href: `/${locale}/admin/agencies`,
    labelFa: "مدیریت آژانس",
    labelEn: "Agency management",
    labelAr: "إدارة الوكالات",
  },
  {
    href: `/${locale}/admin/access`,
    labelFa: "کاربر و دسترسی",
    labelEn: "Users & access",
    labelAr: "المستخدم والوصول",
  },
  {
    href: `/${locale}/admin/reporting`,
    labelFa: "گزارش‌ها",
    labelEn: "Reporting",
    labelAr: "التقارير",
  },
  {
    href: `/${locale}/admin/audit`,
    labelFa: "ممیزی و گردش‌کار",
    labelEn: "Audit & workflow",
    labelAr: "التدقيق وسير العمل",
  },
];

/**
 * Shared Admin navigation — P37-T004 operational IA (workflow-oriented).
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
  const base = `/${locale}/admin`;

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
          const active =
            item.href === base
              ? currentPath === base ||
                currentPath?.endsWith("/admin") ||
                currentPath?.includes("/admin/operations")
              : currentPath != null &&
                (currentPath === item.href || currentPath.startsWith(`${item.href}/`));
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
