import type { AppLocale } from "@/lib/i18n";

export type ReferenceDataAdminCopy = {
  navLabel: string;
  pageTitle: string;
  pageIntro: string;
  hubLink: string;
  destinationsLink: string;
  accountsLink: string;
  countriesHeading: string;
  currenciesHeading: string;
  localesHeading: string;
  filterLabel: string;
  empty: string;
  apiMissing: string;
  errorGeneric: string;
  readOnlyNote: string;
  codeLabel: string;
  nameLabel: string;
};

const COPY: Record<"fa" | "en", ReferenceDataAdminCopy> = {
  fa: {
    navLabel: "مراجع پایدار",
    pageTitle: "مراجع پایدار (ReferenceData)",
    pageIntro:
      "خواندن کاتالوگ‌های پایدار برای پشتیبانی گردش کار مقصد — نه CMS و نه سلسله‌مراتب Destination.",
    hubLink: "مرکز کاتالوگ",
    destinationsLink: "سلسله‌مراتب مقصد",
    accountsLink: "حساب‌ها و افراد",
    countriesHeading: "کشورهای ISO",
    currenciesHeading: "ارزها",
    localesHeading: "Localeها",
    filterLabel: "فیلتر نام یا کد",
    empty: "موردی نیست.",
    apiMissing: "آدرس API پیکربندی نشده است (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "بارگذاری انجام نشد.",
    readOnlyNote: "فقط خواندن — بدون CRUD ماژول‌محور.",
    codeLabel: "کد",
    nameLabel: "نام",
  },
  en: {
    navLabel: "Stable references",
    pageTitle: "Stable references (ReferenceData)",
    pageIntro:
      "Read-only catalogs that support Destination workflows — not a CMS and not Destination hierarchy.",
    hubLink: "Catalog hub",
    destinationsLink: "Destination hierarchy",
    accountsLink: "Accounts & people",
    countriesHeading: "ISO countries",
    currenciesHeading: "Currencies",
    localesHeading: "Locales",
    filterLabel: "Filter by name or code",
    empty: "Nothing listed.",
    apiMissing: "API base URL is not configured (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "Load failed.",
    readOnlyNote: "Read-only — no module-silo CRUD.",
    codeLabel: "Code",
    nameLabel: "Name",
  },
};

export function getReferenceDataAdminCopy(locale: AppLocale): ReferenceDataAdminCopy {
  return locale === "fa" ? COPY.fa : COPY.en;
}
