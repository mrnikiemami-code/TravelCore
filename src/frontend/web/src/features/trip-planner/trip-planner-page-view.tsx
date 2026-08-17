import type { AppLocale } from "@/lib/i18n";
import { Stack, Text } from "@/components/ui";
import { getTripPlannerWorkflowCopy } from "@/features/trip-planner/copy";
import { TripPlannerWorkflowIsland } from "@/features/trip-planner/trip-planner-workflow-island";

export type TripPlannerPageViewProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

/**
 * Public Trip Planner composition (TC-P18-T008 / P18-R8).
 * PublicExperience composes; TripPlanner owns TripIntent/Lead facts.
 */
export function TripPlannerPageView({ locale, apiConfigured }: TripPlannerPageViewProps) {
  const copy = getTripPlannerWorkflowCopy(locale);

  return (
    <Stack gap="md">
      <Text as="h1" role="heading">
        {copy.pageTitle}
      </Text>
      <TripPlannerWorkflowIsland locale={locale} apiConfigured={apiConfigured} />
    </Stack>
  );
}
