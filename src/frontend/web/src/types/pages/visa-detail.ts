import type { AppLocale } from "@/lib/i18n";
import type { PageViewModel } from "@/lib/api/read-models";
import type { MoneyView } from "@/types/money";

export type VisaApplicabilityView = {
  destinationGeographicId: string;
  applicantNationalityCode: string | null;
  residenceCountryCode: string | null;
  applicantCategory: string | null;
};

export type VisaRequiredDocumentView = {
  requiredDocumentId: string;
  code: string;
  requirementLevel: string;
  sortOrder: number;
  name: string | null;
  notes: string | null;
};

export type VisaEligibilityRequirementView = {
  eligibilityRequirementId: string;
  code: string;
  requirementLevel: string;
  kind: string | null;
  value: string | null;
  unit: string | null;
  sortOrder: number;
  name: string | null;
  notes: string | null;
};

export type VisaProcessingTimeView = {
  minValue: number;
  maxValue: number | null;
  unit: string;
};

export type VisaValidityView = {
  value: number;
  unit: string;
};

export type VisaAllowedStayView = {
  value: number;
  unit: string;
};

export type VisaEntryPolicyView = {
  kind: string;
};

export type VisaOfficialFeeView = {
  officialFeeId: string;
  kind: string;
  money: MoneyView;
  sortOrder: number;
  source: string | null;
};

export type VisaRequirementSetView = {
  requirementSetId: string;
  applicability: VisaApplicabilityView;
  requiredDocuments: VisaRequiredDocumentView[];
  eligibilityRequirements: VisaEligibilityRequirementView[];
  processingTime: VisaProcessingTimeView | null;
  validity: VisaValidityView | null;
  allowedStay: VisaAllowedStayView | null;
  entryPolicy: VisaEntryPolicyView | null;
  officialFees: VisaOfficialFeeView[];
  effectiveFrom: string | null;
  effectiveTo: string | null;
};

export type VisaDetailPageFields = {
  locale: AppLocale;
  visaDefinitionId: string;
  code: string;
  name: string;
  summary: string | null;
  publicPath: string;
  requirementSets: VisaRequirementSetView[];
};

export type VisaDetailPageViewModel = PageViewModel<VisaDetailPageFields>;
