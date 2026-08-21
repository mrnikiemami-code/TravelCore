import { CustomerShell } from "@/components/shell";
import {
  CustomerSectionView,
  type CustomerSectionId,
} from "@/features/customer-experience/customer-section-view";
import type { AppLocale } from "@/lib/i18n";

function titles(locale: AppLocale, section: CustomerSectionId): string {
  if (locale === "fa") {
    switch (section) {
      case "bookings":
        return "رزروها";
      case "payments":
        return "پرداخت‌ها";
      case "documents":
        return "مدارک";
      case "passengers":
        return "مسافران";
      case "notifications":
        return "اعلان‌ها";
      case "profile":
        return "پروفایل";
    }
  }
  if (locale === "ar") {
    switch (section) {
      case "bookings":
        return "الحجوزات";
      case "payments":
        return "المدفوعات";
      case "documents":
        return "المستندات";
      case "passengers":
        return "المسافرون";
      case "notifications":
        return "الإشعارات";
      case "profile":
        return "الملف";
    }
  }
  switch (section) {
    case "bookings":
      return "Bookings";
    case "payments":
      return "Payments";
    case "documents":
      return "Documents";
    case "passengers":
      return "Passengers";
    case "notifications":
      return "Notifications";
    case "profile":
      return "Profile";
  }
}

export function CustomerSectionPage({
  locale,
  section,
}: {
  locale: AppLocale;
  section: CustomerSectionId;
}) {
  const title = titles(locale, section);
  const breadcrumb =
    locale === "fa"
      ? `مسافر / ${title}`
      : locale === "ar"
        ? `مسافر / ${title}`
        : `Traveler / ${title}`;

  return (
    <CustomerShell
      locale={locale}
      title={title}
      breadcrumb={<span>{breadcrumb}</span>}
      currentPath={`/${locale}/me/${section}`}
    >
      <CustomerSectionView locale={locale} section={section} />
    </CustomerShell>
  );
}
