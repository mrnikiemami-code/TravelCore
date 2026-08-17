import { apiGetJson } from "@/lib/api/client";
import { asPageViewModel } from "@/lib/api/read-models";
import { isApiOk } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";
import type { AppLocale } from "@/lib/i18n";
import type {
  VisaDetailPageViewModel,
  VisaRequirementSetView,
} from "@/types/pages/visa-detail";

type ApiMoney = {
  amount: string;
  currencyCode: string;
};

type ApiRequirementSet = {
  requirementSetId: string;
  applicability: {
    destinationGeographicId: string;
    applicantNationalityCode?: string | null;
    residenceCountryCode?: string | null;
    applicantCategory?: string | null;
  };
  requiredDocuments?: Array<{
    requiredDocumentId: string;
    code: string;
    requirementLevel: string;
    sortOrder: number;
    name?: string | null;
    notes?: string | null;
  }> | null;
  eligibilityRequirements?: Array<{
    eligibilityRequirementId: string;
    code: string;
    requirementLevel: string;
    kind?: string | null;
    value?: string | null;
    unit?: string | null;
    sortOrder: number;
    name?: string | null;
    notes?: string | null;
  }> | null;
  processingTime?: {
    minValue: number;
    maxValue?: number | null;
    unit: string;
  } | null;
  validity?: { value: number; unit: string } | null;
  allowedStay?: { value: number; unit: string } | null;
  entryPolicy?: { kind: string } | null;
  officialFees?: Array<{
    officialFeeId: string;
    kind: string;
    money: ApiMoney;
    sortOrder: number;
    source?: string | null;
  }> | null;
  effectiveFrom?: string | null;
  effectiveTo?: string | null;
};

type ApiPublicVisa = {
  visaDefinitionId: string;
  code: string;
  localeCode: string;
  name: string;
  summary?: string | null;
  publicPath: string;
  requirementSets?: ApiRequirementSet[] | null;
};

function mapSet(set: ApiRequirementSet): VisaRequirementSetView {
  return {
    requirementSetId: set.requirementSetId,
    applicability: {
      destinationGeographicId: set.applicability.destinationGeographicId,
      applicantNationalityCode: set.applicability.applicantNationalityCode ?? null,
      residenceCountryCode: set.applicability.residenceCountryCode ?? null,
      applicantCategory: set.applicability.applicantCategory ?? null,
    },
    requiredDocuments: (set.requiredDocuments ?? []).map((item) => ({
      requiredDocumentId: item.requiredDocumentId,
      code: item.code,
      requirementLevel: item.requirementLevel,
      sortOrder: item.sortOrder,
      name: item.name ?? null,
      notes: item.notes ?? null,
    })),
    eligibilityRequirements: (set.eligibilityRequirements ?? []).map((item) => ({
      eligibilityRequirementId: item.eligibilityRequirementId,
      code: item.code,
      requirementLevel: item.requirementLevel,
      kind: item.kind ?? null,
      value: item.value ?? null,
      unit: item.unit ?? null,
      sortOrder: item.sortOrder,
      name: item.name ?? null,
      notes: item.notes ?? null,
    })),
    processingTime: set.processingTime
      ? {
          minValue: set.processingTime.minValue,
          maxValue: set.processingTime.maxValue ?? null,
          unit: set.processingTime.unit,
        }
      : null,
    validity: set.validity
      ? { value: set.validity.value, unit: set.validity.unit }
      : null,
    allowedStay: set.allowedStay
      ? { value: set.allowedStay.value, unit: set.allowedStay.unit }
      : null,
    entryPolicy: set.entryPolicy ? { kind: set.entryPolicy.kind } : null,
    officialFees: (set.officialFees ?? []).map((fee) => ({
      officialFeeId: fee.officialFeeId,
      kind: fee.kind,
      money: {
        amount: fee.money.amount,
        currencyCode: fee.money.currencyCode,
      },
      sortOrder: fee.sortOrder,
      source: fee.source ?? null,
    })),
    effectiveFrom: set.effectiveFrom ?? null,
    effectiveTo: set.effectiveTo ?? null,
  };
}

/**
 * Loads public Visa facts for locale + Visa-owned code.
 * Locale is explicit (URL). Missing localized definition → 404.
 * Does not query Visa persistence from the frontend.
 */
export async function loadVisaDetailPage(
  locale: AppLocale,
  code: string,
): Promise<ApiResult<VisaDetailPageViewModel>> {
  const qs = new URLSearchParams({ localeCode: locale });
  const result = await apiGetJson<ApiPublicVisa>(
    `/api/visa/public/definitions/${encodeURIComponent(code.trim())}?${qs.toString()}`,
    { cache: "no-store" },
  );
  if (!isApiOk(result)) {
    return result;
  }

  const data = result.data;
  return {
    ok: true,
    status: 200,
    data: asPageViewModel({
      locale,
      visaDefinitionId: data.visaDefinitionId,
      code: data.code,
      name: data.name,
      summary: data.summary?.trim() || null,
      publicPath: data.publicPath,
      requirementSets: (data.requirementSets ?? []).map(mapSet),
    }),
  };
}
