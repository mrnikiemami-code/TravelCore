import type { AppLocale } from "@/lib/i18n";

export type IdentityPartyWorkflowCopy = {
  hubTitle: string;
  hubBody: string;
  startJourney: string;
  navLabel: string;
  pageTitle: string;
  pageIntro: string;
  stepAccount: string;
  stepParty: string;
  stepLink: string;
  emailLabel: string;
  passwordLabel: string;
  createAccount: string;
  accountCreated: string;
  partySearchLabel: string;
  partySearch: string;
  partyCreateKind: string;
  partyDisplayName: string;
  createParty: string;
  selectParty: string;
  linkParty: string;
  replaceParty: string;
  unlinkParty: string;
  inspectTitle: string;
  noResults: string;
  unauthorizedTitle: string;
  unauthorizedBody: string;
  apiMissing: string;
  errorGeneric: string;
  backToHub: string;
};

const COPY: Record<"fa" | "en", IdentityPartyWorkflowCopy> = {
  fa: {
    hubTitle: "حساب‌ها و افراد",
    hubBody:
      "گردش کاری انتخاب/ایجاد هویت و پیوند به Party — بدون منوی جدا برای هر ماژول.",
    startJourney: "شروع پیوند Identity ↔ Party",
    navLabel: "حساب‌ها و افراد",
    pageTitle: "گردش کار Identity ↔ Party",
    pageIntro:
      "هویت را بسازید، Party را جستجو یا ایجاد کنید، سپس پیوند دهید. شناسه خام مسیر اصلی نیست.",
    stepAccount: "۱. ایجاد حساب",
    stepParty: "۲. انتخاب یا ایجاد Party",
    stepLink: "۳. پیوند / جایگزینی / قطع پیوند",
    emailLabel: "ایمیل",
    passwordLabel: "رمز عبور",
    createAccount: "ایجاد حساب",
    accountCreated: "حساب ایجاد شد",
    partySearchLabel: "جستجوی Party",
    partySearch: "جستجو",
    partyCreateKind: "نوع Party",
    partyDisplayName: "نام نمایشی",
    createParty: "ایجاد Party",
    selectParty: "انتخاب",
    linkParty: "پیوند",
    replaceParty: "جایگزینی",
    unlinkParty: "قطع پیوند",
    inspectTitle: "وضعیت فعلی",
    noResults: "نتیجه‌ای نیست — Party جدید بسازید.",
    unauthorizedTitle: "ورود لازم است",
    unauthorizedBody:
      "برای این گردش کار باید با Cookie امن احراز هویت شده باشید (سرور تصمیم می‌گیرد).",
    apiMissing: "آدرس API پیکربندی نشده است (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "عملیات انجام نشد. جزئیات فنی نمایش داده نمی‌شود.",
    backToHub: "بازگشت به مرکز گردش کار",
  },
  en: {
    hubTitle: "Accounts & people",
    hubBody:
      "Job-based Identity create/select and Party link — not three module CRUD menus.",
    startJourney: "Start Identity ↔ Party linking",
    navLabel: "Accounts & people",
    pageTitle: "Identity ↔ Party workflow",
    pageIntro:
      "Create an account, search or create a Party, then link. Raw IDs are not the primary path.",
    stepAccount: "1. Create account",
    stepParty: "2. Select or create Party",
    stepLink: "3. Link / replace / unlink",
    emailLabel: "Email",
    passwordLabel: "Password",
    createAccount: "Create account",
    accountCreated: "Account created",
    partySearchLabel: "Search parties",
    partySearch: "Search",
    partyCreateKind: "Party kind",
    partyDisplayName: "Display name",
    createParty: "Create Party",
    selectParty: "Select",
    linkParty: "Link",
    replaceParty: "Replace",
    unlinkParty: "Unlink",
    inspectTitle: "Current state",
    noResults: "No matches — create a Party.",
    unauthorizedTitle: "Sign-in required",
    unauthorizedBody:
      "This workflow requires a secure authenticated session (server decides).",
    apiMissing: "API base URL is not configured (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "The operation failed. Technical details are not shown.",
    backToHub: "Back to workflow hub",
  },
};

export function getIdentityPartyWorkflowCopy(
  locale: AppLocale,
): IdentityPartyWorkflowCopy {
  return locale === "fa" ? COPY.fa : COPY.en;
}
