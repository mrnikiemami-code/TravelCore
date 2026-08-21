import { AgencyShell } from "@/components/shell";
import {
  AgencySectionView,
  type AgencySectionId,
} from "@/features/agency-experience/agency-section-view";
import type { AppLocale } from "@/lib/i18n";

export function AgencySectionPage({
  locale,
  section,
}: {
  locale: AppLocale;
  section: AgencySectionId;
}) {
  return (
    <AgencyShell
      locale={locale}
      title={section}
      breadcrumb={
        <span>
          Agency / {section}
        </span>
      }
      currentPath={`/${locale}/agency/${section}`}
    >
      <AgencySectionView locale={locale} section={section} />
    </AgencyShell>
  );
}
