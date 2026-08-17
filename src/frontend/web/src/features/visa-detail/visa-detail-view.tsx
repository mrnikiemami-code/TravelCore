import {
  Container,
  LtrValue,
  MoneyText,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import { RelatedContentList } from "@/features/public-experience/related-content-list";
import type { RelatedContentView } from "@/features/public-experience/load-related-content";
import type { AppLocale } from "@/lib/i18n";
import type {
  VisaAllowedStayView,
  VisaDetailPageViewModel,
  VisaOfficialFeeView,
  VisaProcessingTimeView,
  VisaRequirementSetView,
  VisaValidityView,
} from "@/types/pages/visa-detail";

function label(locale: string, fa: string, en: string): string {
  return locale === "fa" ? fa : en;
}

function levelLabel(locale: string, level: string): string {
  if (locale === "fa") {
    switch (level) {
      case "Required":
        return "الزامی";
      case "Conditional":
        return "مشروط";
      case "Optional":
        return "اختیاری";
      default:
        return level;
    }
  }
  return level;
}

function timeUnitLabel(locale: string, unit: string): string {
  if (locale === "fa") {
    switch (unit) {
      case "Days":
        return "روز";
      case "BusinessDays":
        return "روز کاری";
      case "Months":
        return "ماه";
      case "Years":
        return "سال";
      default:
        return unit;
    }
  }
  return unit;
}

function formatQuantity(
  locale: string,
  value: number,
  unit: string,
): string {
  return `${value} ${timeUnitLabel(locale, unit)}`;
}

function formatProcessing(
  locale: string,
  processing: VisaProcessingTimeView,
): string {
  const unit = timeUnitLabel(locale, processing.unit);
  if (processing.maxValue == null || processing.maxValue === processing.minValue) {
    return `${processing.minValue} ${unit}`;
  }
  return `${processing.minValue}–${processing.maxValue} ${unit}`;
}

function formatValidity(locale: string, validity: VisaValidityView): string {
  return formatQuantity(locale, validity.value, validity.unit);
}

function formatStay(locale: string, stay: VisaAllowedStayView): string {
  return formatQuantity(locale, stay.value, stay.unit);
}

function entryLabel(locale: string, kind: string): string {
  if (locale === "fa") {
    switch (kind) {
      case "Single":
        return "یک‌بار ورود";
      case "Double":
        return "دو بار ورود";
      case "Multiple":
        return "چندبار ورود";
      default:
        return kind;
    }
  }
  return kind;
}

function feeKindLabel(locale: string, kind: string): string {
  if (locale === "fa") {
    switch (kind) {
      case "Application":
        return "هزینه درخواست رسمی";
      case "Issuance":
        return "هزینه صدور رسمی";
      case "Embassy":
        return "هزینه سفارت";
      case "ServiceCenter":
        return "هزینه مرکز خدمات";
      default:
        return kind;
    }
  }
  return kind;
}

function formatInstant(value: string | null, locale: string): string | null {
  if (!value) {
    return null;
  }
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }
  return new Intl.DateTimeFormat(locale === "fa" ? "fa-IR" : "en-GB", {
    year: "numeric",
    month: "short",
    day: "numeric",
  }).format(date);
}

function RequirementSetSection({
  locale,
  set,
  index,
}: {
  locale: AppLocale;
  set: VisaRequirementSetView;
  index: number;
}) {
  const from = formatInstant(set.effectiveFrom, locale);
  const to = formatInstant(set.effectiveTo, locale);

  return (
    <Surface className="scroll-mt-4">
      <Stack gap="md">
        <Stack gap="sm">
          <Text as="h2" role="heading" className="scroll-mt-4">
            {label(locale, `زمینه ${index + 1}`, `Context ${index + 1}`)}
          </Text>
          <Text role="caption">
            {label(locale, "این الزام‌ها برای چه کسانی است", "Who this applies to")}
          </Text>
          <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2">
            <li>
              <Text role="caption">
                {label(locale, "مقصد / حوزه", "Destination / jurisdiction")}
              </Text>
              <LtrValue>{set.applicability.destinationGeographicId}</LtrValue>
            </li>
            {set.applicability.applicantNationalityCode ? (
              <li>
                <Text role="caption">{label(locale, "ملیت", "Nationality")}</Text>
                <LtrValue>{set.applicability.applicantNationalityCode}</LtrValue>
              </li>
            ) : null}
            {set.applicability.residenceCountryCode ? (
              <li>
                <Text role="caption">{label(locale, "اقامت", "Residence")}</Text>
                <LtrValue>{set.applicability.residenceCountryCode}</LtrValue>
              </li>
            ) : null}
            {set.applicability.applicantCategory ? (
              <li>
                <Text role="caption">{label(locale, "دسته متقاضی", "Applicant category")}</Text>
                <Text>{set.applicability.applicantCategory}</Text>
              </li>
            ) : null}
          </ul>
          {from || to ? (
            <Text role="caption">
              {label(locale, "بازه اعتبار مجموعه الزام", "Requirement-set effective period")}
              {": "}
              <LtrValue>
                {from ?? "—"} – {to ?? "—"}
              </LtrValue>
            </Text>
          ) : null}
        </Stack>

        <Stack gap="sm">
          <Text as="h3" role="title">
            {label(locale, "مدارک لازم", "Required documents")}
          </Text>
          {set.requiredDocuments.length === 0 ? (
            <Text role="muted">
              {label(locale, "مدرک ساخت‌یافته‌ای ثبت نشده.", "No structured documents.")}
            </Text>
          ) : (
            <ul className="grid grid-cols-1 gap-2">
              {set.requiredDocuments.map((item) => (
                <li key={item.requiredDocumentId}>
                  <Surface tone="muted" className="p-3 sm:p-4">
                    <Stack gap="sm">
                      <Text>
                        {item.name ?? item.code}{" "}
                        <Text as="span" role="caption">
                          ({levelLabel(locale, item.requirementLevel)})
                        </Text>
                      </Text>
                      <Text role="caption">
                        <LtrValue>{item.code}</LtrValue>
                      </Text>
                      {item.notes ? <Text role="muted">{item.notes}</Text> : null}
                    </Stack>
                  </Surface>
                </li>
              ))}
            </ul>
          )}
        </Stack>

        <Stack gap="sm">
          <Text as="h3" role="title">
            {label(locale, "شرایط احراز", "Eligibility")}
          </Text>
          {set.eligibilityRequirements.length === 0 ? (
            <Text role="muted">
              {label(locale, "شرط ساخت‌یافته‌ای ثبت نشده.", "No structured eligibility facts.")}
            </Text>
          ) : (
            <ul className="grid grid-cols-1 gap-2">
              {set.eligibilityRequirements.map((item) => (
                <li key={item.eligibilityRequirementId}>
                  <Surface tone="muted" className="p-3 sm:p-4">
                    <Stack gap="sm">
                      <Text>
                        {item.name ?? item.code}{" "}
                        <Text as="span" role="caption">
                          ({levelLabel(locale, item.requirementLevel)})
                        </Text>
                      </Text>
                      <Text role="caption">
                        <LtrValue>{item.code}</LtrValue>
                        {item.kind ? (
                          <>
                            {" · "}
                            <LtrValue>{item.kind}</LtrValue>
                          </>
                        ) : null}
                        {item.value ? (
                          <>
                            {" · "}
                            <LtrValue>
                              {item.value}
                              {item.unit ? ` ${item.unit}` : ""}
                            </LtrValue>
                          </>
                        ) : null}
                      </Text>
                      {item.notes ? <Text role="muted">{item.notes}</Text> : null}
                    </Stack>
                  </Surface>
                </li>
              ))}
            </ul>
          )}
        </Stack>

        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <Surface tone="muted">
            <Stack gap="sm">
              <Text as="h3" role="title">
                {label(locale, "زمان رسیدگی", "Processing time")}
              </Text>
              <Text>
                {set.processingTime
                  ? formatProcessing(locale, set.processingTime)
                  : label(locale, "ثبت نشده", "Not recorded")}
              </Text>
            </Stack>
          </Surface>
          <Surface tone="muted">
            <Stack gap="sm">
              <Text as="h3" role="title">
                {label(locale, "اعتبار ویزا", "Visa validity")}
              </Text>
              <Text>
                {set.validity
                  ? formatValidity(locale, set.validity)
                  : label(locale, "ثبت نشده", "Not recorded")}
              </Text>
            </Stack>
          </Surface>
          <Surface tone="muted">
            <Stack gap="sm">
              <Text as="h3" role="title">
                {label(locale, "مدت اقامت مجاز", "Allowed stay")}
              </Text>
              <Text>
                {set.allowedStay
                  ? formatStay(locale, set.allowedStay)
                  : label(locale, "ثبت نشده", "Not recorded")}
              </Text>
            </Stack>
          </Surface>
          <Surface tone="muted">
            <Stack gap="sm">
              <Text as="h3" role="title">
                {label(locale, "سیاست ورود", "Entry policy")}
              </Text>
              <Text>
                {set.entryPolicy
                  ? entryLabel(locale, set.entryPolicy.kind)
                  : label(locale, "ثبت نشده", "Not recorded")}
              </Text>
            </Stack>
          </Surface>
        </div>

        <Stack gap="sm">
          <Text as="h3" role="title">
            {label(locale, "هزینه‌های رسمی", "Official fees")}
          </Text>
          <Text role="caption">
            {label(
              locale,
              "مبالغ رسمی/تنظیمی برای اطلاع — قیمت فروش یا پیش‌فاکتور نیست.",
              "Regulatory/informational amounts — not a selling price or quote.",
            )}
          </Text>
          {set.officialFees.length === 0 ? (
            <Text role="muted">
              {label(locale, "هزینه رسمی ثبت نشده.", "No official fee recorded.")}
            </Text>
          ) : (
            <ul className="grid grid-cols-1 gap-2">
              {set.officialFees.map((fee: VisaOfficialFeeView) => (
                <li key={fee.officialFeeId}>
                  <Surface tone="muted" className="p-3 sm:p-4">
                    <Stack gap="sm">
                      <Text>{feeKindLabel(locale, fee.kind)}</Text>
                      <MoneyText money={fee.money} locale={locale} />
                      {fee.source ? (
                        <Text role="caption">
                          {label(locale, "منبع", "Source")} ·{" "}
                          <LtrValue>{fee.source}</LtrValue>
                        </Text>
                      ) : null}
                    </Stack>
                  </Surface>
                </li>
              ))}
            </ul>
          )}
        </Stack>
      </Stack>
    </Surface>
  );
}

/**
 * Public VisaDetailPage composition (TC-P17-T007 / P17-R7).
 * Server Component. Visa facts stay in Visa; Content is enrichment; SEO owns IndexPolicy.
 * Informational only — no application/transaction workflow.
 */
export function VisaDetailView({
  vm,
  relatedContent,
}: {
  vm: VisaDetailPageViewModel;
  relatedContent: RelatedContentView[];
}) {
  const locale = vm.locale;

  return (
    <div className="py-6 sm:py-8">
      <Container width="content">
        <Stack gap="lg">
          <Stack gap="sm">
            <Text as="h1" role="heading">
              {vm.name}
            </Text>
            <Text role="muted">
              {label(locale, "نوع ویزا", "Visa type")} · <LtrValue>{vm.code}</LtrValue>
            </Text>
            {vm.summary ? <Text as="p">{vm.summary}</Text> : null}
          </Stack>

          <div role="status">
            <Surface tone="muted">
              <Text>
                {label(
                  locale,
                  "الزام‌های ویزا ممکن است تغییر کنند. این صفحه راهنمای حقوقی یا موتور مشاوره نیست.",
                  "Visa requirements can change. This page is not legal advice and not an advice engine.",
                )}
              </Text>
            </Surface>
          </div>

          {vm.requirementSets.length === 0 ? (
            <Text role="muted">
              {label(
                locale,
                "مجموعه الزام ساخت‌یافته‌ای برای این نوع ویزا ثبت نشده.",
                "No structured requirement set is recorded for this visa type.",
              )}
            </Text>
          ) : (
            vm.requirementSets.map((set, index) => (
              <RequirementSetSection
                key={set.requirementSetId}
                locale={locale}
                set={set}
                index={index}
              />
            ))
          )}

          <RelatedContentList locale={locale} items={relatedContent} />
        </Stack>
      </Container>
    </div>
  );
}
