import type { AppLocale } from "@/lib/i18n";

export type DestinationHierarchyWorkflowCopy = {
  hubTitle: string;
  hubBody: string;
  startJourney: string;
  navLabel: string;
  accountsJob: string;
  pageTitle: string;
  pageIntro: string;
  stepOpen: string;
  stepBrowse: string;
  stepCreate: string;
  stepTranslate: string;
  stepGeo: string;
  openBySlugLabel: string;
  slugLocaleLabel: string;
  slugValueLabel: string;
  openBySlug: string;
  createRootCountry: string;
  codeLabel: string;
  englishNameLabel: string;
  isoCountryLabel: string;
  isoCountryFilter: string;
  createDestination: string;
  kindLabel: string;
  parentContext: string;
  noFocus: string;
  breadcrumbLabel: string;
  childrenLabel: string;
  descendantsLabel: string;
  focusChild: string;
  focusAncestor: string;
  noChildren: string;
  translationLocale: string;
  translationName: string;
  translationDescription: string;
  translationSlug: string;
  saveTranslation: string;
  saveSlug: string;
  latitudeLabel: string;
  longitudeLabel: string;
  saveGeo: string;
  clearGeo: string;
  focusedTitle: string;
  kindCountry: string;
  kindRegion: string;
  kindCity: string;
  kindArea: string;
  slugNotSeo: string;
  unauthorizedBody: string;
  apiMissing: string;
  errorGeneric: string;
  notFound: string;
  backToHub: string;
  noCountries: string;
};

const COPY: Record<"fa" | "en", DestinationHierarchyWorkflowCopy> = {
  fa: {
    hubTitle: "کاتالوگ و مقاصد",
    hubBody:
      "گردش کاری سلسله‌مراتب مقصد — بدون منوی CRUD جدا برای Destination یا ReferenceData.",
    startJourney: "مدیریت سلسله‌مراتب مقصد",
    navLabel: "کاتالوگ و مقاصد",
    accountsJob: "حساب‌ها و افراد",
    pageTitle: "گردش کار مدیریت مقصد",
    pageIntro:
      "با اسلاگ محلی باز کنید، زیر گرهٔ انتخاب‌شده ایجاد کنید، ترجمه و مختصات را ویرایش کنید. شناسهٔ خام مسیر اصلی نیست.",
    stepOpen: "۱. باز کردن یا ایجاد ریشه",
    stepBrowse: "۲. مسیر و فرزندان",
    stepCreate: "۳. ایجاد زیر والد فعلی",
    stepTranslate: "۴. ترجمه و اسلاگ محلی",
    stepGeo: "۵. مختصات جغرافیایی",
    openBySlugLabel: "باز کردن با اسلاگ محلی",
    slugLocaleLabel: "زبان اسلاگ",
    slugValueLabel: "اسلاگ",
    openBySlug: "باز کردن",
    createRootCountry: "ایجاد کشور ریشه (Country)",
    codeLabel: "کد مقصد",
    englishNameLabel: "نام انگلیسی",
    isoCountryLabel: "کشور ISO (ReferenceData)",
    isoCountryFilter: "فیلتر نام یا کد ISO",
    createDestination: "ایجاد مقصد",
    kindLabel: "نوع گره",
    parentContext: "والد فعلی",
    noFocus: "هنوز گره‌ای انتخاب نشده — با اسلاگ باز کنید یا کشور ریشه بسازید.",
    breadcrumbLabel: "مسیر",
    childrenLabel: "فرزندان مستقیم",
    descendantsLabel: "نوادگان (عمق ۱)",
    focusChild: "انتخاب",
    focusAncestor: "رفتن به",
    noChildren: "فرزندی نیست.",
    translationLocale: "زبان ترجمه",
    translationName: "نام محلی",
    translationDescription: "توضیح",
    translationSlug: "اسلاگ محلی",
    saveTranslation: "ذخیره ترجمه",
    saveSlug: "تنظیم اسلاگ",
    latitudeLabel: "عرض جغرافیایی",
    longitudeLabel: "طول جغرافیایی",
    saveGeo: "ذخیره مختصات",
    clearGeo: "پاک کردن مختصات",
    focusedTitle: "گرهٔ فعال",
    kindCountry: "کشور",
    kindRegion: "منطقه / استان",
    kindCity: "شهر",
    kindArea: "ناحیه",
    slugNotSeo: "اسلاگ هوک موجودیت است — موتور SEO/انتشار عمومی اینجا نیست.",
    unauthorizedBody:
      "برای ایجاد/ویرایش باید با Cookie امن احراز هویت شده باشید و مجوز نوشتن داشته باشید.",
    apiMissing: "آدرس API پیکربندی نشده است (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "عملیات انجام نشد. جزئیات فنی نمایش داده نمی‌شود.",
    notFound: "مقصدی با این اسلاگ پیدا نشد.",
    backToHub: "بازگشت به مرکز کاتالوگ",
    noCountries: "کاتالوگ کشور خالی است.",
  },
  en: {
    hubTitle: "Catalog & destinations",
    hubBody:
      "Job-based Destination hierarchy workflow — not separate Destination/ReferenceData CRUD menus.",
    startJourney: "Manage destination hierarchy",
    navLabel: "Catalog & destinations",
    accountsJob: "Accounts & people",
    pageTitle: "Destination management workflow",
    pageIntro:
      "Open by localized slug, create under the focused parent, edit translations and geo. Raw IDs are not the primary path.",
    stepOpen: "1. Open or create a root",
    stepBrowse: "2. Path and children",
    stepCreate: "3. Create under current parent",
    stepTranslate: "4. Translation and localized slug",
    stepGeo: "5. Geographic coordinates",
    openBySlugLabel: "Open by localized slug",
    slugLocaleLabel: "Slug locale",
    slugValueLabel: "Slug",
    openBySlug: "Open",
    createRootCountry: "Create root Country",
    codeLabel: "Destination code",
    englishNameLabel: "English name",
    isoCountryLabel: "ISO country (ReferenceData)",
    isoCountryFilter: "Filter by name or ISO code",
    createDestination: "Create destination",
    kindLabel: "Node kind",
    parentContext: "Current parent",
    noFocus: "No node focused yet — open by slug or create a root Country.",
    breadcrumbLabel: "Path",
    childrenLabel: "Direct children",
    descendantsLabel: "Descendants (depth 1)",
    focusChild: "Select",
    focusAncestor: "Go to",
    noChildren: "No children.",
    translationLocale: "Translation locale",
    translationName: "Localized name",
    translationDescription: "Description",
    translationSlug: "Localized slug",
    saveTranslation: "Save translation",
    saveSlug: "Set slug",
    latitudeLabel: "Latitude",
    longitudeLabel: "Longitude",
    saveGeo: "Save coordinates",
    clearGeo: "Clear coordinates",
    focusedTitle: "Focused node",
    kindCountry: "Country",
    kindRegion: "Region",
    kindCity: "City",
    kindArea: "Area",
    slugNotSeo: "Slug is an entity hook — SEO publish engine is not here.",
    unauthorizedBody:
      "Create/edit requires a secure authenticated session with write permission.",
    apiMissing: "API base URL is not configured (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "The operation failed. Technical details are not shown.",
    notFound: "No destination matched that slug.",
    backToHub: "Back to catalog hub",
    noCountries: "Country catalog is empty.",
  },
};

export function getDestinationHierarchyWorkflowCopy(
  locale: AppLocale,
): DestinationHierarchyWorkflowCopy {
  return locale === "fa" ? COPY.fa : COPY.en;
}
