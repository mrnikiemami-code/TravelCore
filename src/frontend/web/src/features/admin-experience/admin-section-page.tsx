import { OpsConsoleShell } from "@/components/shell";
import {
  AdminSectionView,
  type AdminSectionId,
} from "@/features/admin-experience/admin-section-view";
import type { AppLocale } from "@/lib/i18n";

export function AdminSectionPage({
  locale,
  section,
}: {
  locale: AppLocale;
  section: AdminSectionId;
}) {
  return (
    <OpsConsoleShell
      locale={locale}
      title={section}
      breadcrumb={<span>Admin / {section}</span>}
      currentPath={`/${locale}/admin/${section}`}
    >
      <AdminSectionView locale={locale} section={section} />
    </OpsConsoleShell>
  );
}
