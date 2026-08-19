import Link from "next/link";
import { Container, Stack, Surface, Text } from "@/components/ui";
import type { AppLocale } from "@/lib/i18n";

type AdminSurfaceLink = {
  href: string;
  label: string;
};

/**
 * UIVAL-T014 Admin surfaces validation — job-based workflow routes (no domain silo CRUD).
 */
export function AdminSurfacesShowcase({ locale }: { locale: AppLocale }) {
  const title = locale === "fa" ? "سطوح Admin" : "Admin surfaces";
  const links: AdminSurfaceLink[] = [
    { href: `/${locale}/admin/accounts`, label: locale === "fa" ? "حساب‌ها" : "Accounts" },
    { href: `/${locale}/admin/accounts/onboard`, label: locale === "fa" ? "راه‌اندازی Identity↔Party" : "Identity↔Party onboard" },
    { href: `/${locale}/admin/catalog`, label: locale === "fa" ? "کاتالوگ" : "Catalog hub" },
    { href: `/${locale}/admin/catalog/destinations`, label: locale === "fa" ? "مقصدها" : "Destinations" },
    { href: `/${locale}/admin/catalog/places`, label: locale === "fa" ? "مکان‌ها" : "Places" },
    { href: `/${locale}/admin/catalog/tours`, label: locale === "fa" ? "تورها" : "Tours" },
    { href: `/${locale}/admin/catalog/departures`, label: locale === "fa" ? "اجرها" : "Departures" },
    { href: `/${locale}/admin/catalog/content`, label: locale === "fa" ? "محتوا" : "Content" },
    { href: `/${locale}/admin/catalog/reference`, label: locale === "fa" ? "مرجع" : "Reference data" },
    { href: `/${locale}/admin/media`, label: locale === "fa" ? "رسانه" : "Media" },
  ];

  return (
    <div className="py-8">
      <Container width="content">
        <Stack gap="lg">
          <Surface>
            <Stack gap="sm">
              <Text as="h1" role="heading">{title}</Text>
              <Text role="muted">
                {locale === "fa"
                  ? "مسیرهای workflow-oriented · noindex · Client islands فقط برای islands"
                  : "Workflow-oriented routes · noindex · Client islands only where needed"}
              </Text>
            </Stack>
          </Surface>
          <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2">
            {links.map((item) => (
              <li key={item.href}>
                <Link
                  href={item.href}
                  className="min-h-touch flex rounded-md border border-border px-4 py-3 text-sm underline-offset-2 hover:underline"
                >
                  {item.label}
                </Link>
              </li>
            ))}
          </ul>
        </Stack>
      </Container>
    </div>
  );
}
