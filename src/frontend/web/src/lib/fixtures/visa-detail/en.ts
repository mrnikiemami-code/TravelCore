import { asPageViewModel } from "@/lib/api/read-models";
import type { VisaDetailPageViewModel } from "@/types/pages/visa-detail";

export const visaDetailEnFixture: VisaDetailPageViewModel = asPageViewModel({
  locale: "en",
  visaDefinitionId: "visa-def-tr-tourist",
  code: "TR-TOURIST",
  name: "Turkey Tourist Visa — Sample",
  summary: "UIVAL VisaDetail fixture — illustrative requirements and documents.",
  publicPath: "visas/TR-TOURIST",
  requirementSets: [
    {
      requirementSetId: "rs-default",
      applicability: {
        destinationGeographicId: "geo-tr",
        applicantNationalityCode: "IR",
        residenceCountryCode: "IR",
        applicantCategory: "Individual",
      },
      requiredDocuments: [
        {
          requiredDocumentId: "doc-passport",
          code: "Passport",
          requirementLevel: "Required",
          sortOrder: 1,
          name: "Passport",
          notes: "Minimum 6 months validity",
        },
      ],
      eligibilityRequirements: [],
      processingTime: { minValue: 3, maxValue: 7, unit: "BusinessDays" },
      validity: { value: 180, unit: "Days" },
      allowedStay: { value: 30, unit: "Days" },
      entryPolicy: { kind: "Multiple" },
      officialFees: [
        {
          officialFeeId: "fee-1",
          kind: "Consular",
          money: { amount: "35", currencyCode: "USD" },
          sortOrder: 1,
          source: "Embassy published",
        },
      ],
      effectiveFrom: "2026-01-01",
      effectiveTo: null,
    },
  ],
});
