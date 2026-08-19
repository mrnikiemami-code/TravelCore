import { asPageViewModel } from "@/lib/api/read-models";
import type { VisaDetailPageViewModel } from "@/types/pages/visa-detail";

export const visaDetailFaFixture: VisaDetailPageViewModel = asPageViewModel({
  locale: "fa",
  visaDefinitionId: "visa-def-tr-tourist",
  code: "TR-TOURIST",
  name: "ویزای توریستی ترکیه — نمونه",
  summary: "صفحهٔ VisaDetail نمونه UIVAL — الزامات و مدارک نمایشی.",
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
          name: "گذرنامه",
          notes: "حداقل ۶ ماه اعتبار",
        },
      ],
      eligibilityRequirements: [
        {
          eligibilityRequirementId: "elig-min-funds",
          code: "MinFunds",
          requirementLevel: "Conditional",
          kind: "Money",
          value: "1500",
          unit: "USD",
          sortOrder: 1,
          name: "حداقل موجودی",
          notes: null,
        },
      ],
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
