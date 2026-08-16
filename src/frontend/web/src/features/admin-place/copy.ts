import type { AppLocale } from "@/lib/i18n";

export type AdminPlaceWorkflowCopy = {
  pageTitle: string;
  pageIntro: string;
  navLabel: string;
  backToHub: string;
  destinationsLink: string;
  mediaLink: string;
  accountsJob: string;
  hubCta: string;
  stepCreate: string;
  stepBrowse: string;
  stepOpenByCode: string;
  stepInspect: string;
  stepTranslate: string;
  stepDestination: string;
  stepGeo: string;
  stepAddress: string;
  stepCatalog: string;
  stepMedia: string;
  kindLabel: string;
  codeLabel: string;
  englishNameLabel: string;
  starRatingLabel: string;
  cuisineTypeLabel: string;
  categoryCodeLabel: string;
  createAction: string;
  kindFilterLabel: string;
  kindAll: string;
  refreshList: string;
  takeLabel: string;
  noPlaces: string;
  selectPlace: string;
  openByCodeAction: string;
  openByCodeHint: string;
  selectedTitle: string;
  metadataHeading: string;
  statusLabel: string;
  classificationLabel: string;
  facilitiesLabel: string;
  facilitiesHint: string;
  saveCatalog: string;
  translationLocale: string;
  nameLabel: string;
  descriptionLabel: string;
  saveTranslation: string;
  destinationSlugLocale: string;
  destinationSlugLabel: string;
  resolveDestination: string;
  clearDestination: string;
  destinationResolved: string;
  saveDestinationLink: string;
  latitudeLabel: string;
  longitudeLabel: string;
  saveGeo: string;
  clearGeo: string;
  line1Label: string;
  line2Label: string;
  localityLabel: string;
  adminAreaLabel: string;
  postalCodeLabel: string;
  countryCodeLabel: string;
  saveAddress: string;
  clearAddress: string;
  coverMediaLabel: string;
  setCover: string;
  removeCover: string;
  galleryMediaLabel: string;
  addGallery: string;
  removeGalleryItem: string;
  mediaLinksHeading: string;
  noMediaLinks: string;
  catalogDraft: string;
  catalogActive: string;
  catalogInactive: string;
  unauthorizedBody: string;
  apiMissing: string;
  errorGeneric: string;
  noDeleteHint: string;
};

const COPY: Record<"fa" | "en", AdminPlaceWorkflowCopy> = {
  fa: {
    pageTitle: "مدیریت مکان‌ها",
    pageIntro:
      "ایجاد و ویرایش کاتالوگ مکان (هتل/رستوران/جاذبه)، ترجمه، مقصد اختیاری، مختصات/آدرس، وضعیت کاتالوگ، طبقه‌بندی، امکانات و رسانهٔ Cover/Gallery — بدون Booking و بدون حذف/بایگانی. مسیر اصلی با Code است نه شناسهٔ خام.",
    navLabel: "مکان‌ها",
    backToHub: "بازگشت به کاتالوگ",
    destinationsLink: "مقاصد",
    mediaLink: "رسانه",
    accountsJob: "حساب‌ها و افراد",
    hubCta: "مدیریت مکان‌ها",
    stepCreate: "۱. ایجاد مکان",
    stepBrowse: "۲. فهرست مکان‌ها",
    stepOpenByCode: "باز کردن با Code",
    stepInspect: "۳. بازرسی",
    stepTranslate: "۴. محتوای محلی",
    stepDestination: "۵. پیوند مقصد (اختیاری)",
    stepGeo: "۶. مختصات",
    stepAddress: "۷. آدرس",
    stepCatalog: "۸. وضعیت · طبقه‌بندی · امکانات",
    stepMedia: "۹. Cover و Gallery",
    kindLabel: "نوع مکان",
    codeLabel: "کد مکان",
    englishNameLabel: "نام انگلیسی",
    starRatingLabel: "ستاره (هتل)",
    cuisineTypeLabel: "نوع غذا (رستوران)",
    categoryCodeLabel: "کد دسته (جاذبه)",
    createAction: "ایجاد",
    kindFilterLabel: "فیلتر نوع",
    kindAll: "همه",
    refreshList: "تازه‌سازی فهرست",
    takeLabel: "حداکثر تعداد",
    noPlaces: "مکانی نیست — ابتدا ایجاد کنید.",
    selectPlace: "باز کردن",
    openByCodeAction: "باز کردن",
    openByCodeHint: "Code یکتای کاتالوگ؛ جایگزین slug عمومی (هنوز تصمیم‌گیری نشده).",
    selectedTitle: "مکان انتخاب‌شده",
    metadataHeading: "متادیتا",
    statusLabel: "وضعیت کاتالوگ",
    classificationLabel: "کد طبقه‌بندی",
    facilitiesLabel: "امکانات",
    facilitiesHint: "کدها را با ویرگول جدا کنید (مثلاً wifi,parking).",
    saveCatalog: "ذخیرهٔ کاتالوگ",
    translationLocale: "زبان ترجمه",
    nameLabel: "نام محلی",
    descriptionLabel: "توضیح",
    saveTranslation: "ذخیرهٔ ترجمه",
    destinationSlugLocale: "زبان slug مقصد",
    destinationSlugLabel: "slug مقصد",
    resolveDestination: "یافتن مقصد",
    clearDestination: "پاک کردن پیوند",
    destinationResolved: "مقصد یافت‌شده",
    saveDestinationLink: "ذخیرهٔ پیوند مقصد",
    latitudeLabel: "عرض جغرافیایی",
    longitudeLabel: "طول جغرافیایی",
    saveGeo: "ذخیرهٔ مختصات",
    clearGeo: "پاک کردن مختصات",
    line1Label: "خط ۱",
    line2Label: "خط ۲",
    localityLabel: "محله/شهر",
    adminAreaLabel: "استان/ناحیه",
    postalCodeLabel: "کد پستی",
    countryCodeLabel: "کد کشور (ISO alpha-2)",
    saveAddress: "ذخیرهٔ آدرس",
    clearAddress: "پاک کردن آدرس",
    coverMediaLabel: "MediaAsset برای Cover",
    setCover: "تنظیم Cover",
    removeCover: "برداشتن Cover",
    galleryMediaLabel: "MediaAsset برای Gallery",
    addGallery: "افزودن به Gallery",
    removeGalleryItem: "حذف از Gallery",
    mediaLinksHeading: "پیوندهای رسانه",
    noMediaLinks: "پیوند رسانه‌ای ثبت نشده.",
    catalogDraft: "Draft",
    catalogActive: "Active",
    catalogInactive: "Inactive",
    unauthorizedBody:
      "برای این گردش کار باید با Cookie امن احراز هویت شده باشید و مجوز Place.Places.Write داشته باشید.",
    apiMissing: "آدرس API پیکربندی نشده است (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "عملیات انجام نشد. جزئیات فنی نمایش داده نمی‌شود.",
    noDeleteHint:
      "حذف/بایگانی مکان در این فاز نیست (P07-R3 باز). Inactive فقط وضعیت عملیاتی کاتالوگ است.",
  },
  en: {
    pageTitle: "Place catalog",
    pageIntro:
      "Create and edit Place catalog (Hotel/Restaurant/Attraction), translations, optional Destination, geo/address, catalog status, classification, facilities, and Cover/Gallery media — no Booking and no delete/archive. Primary path is Code, not raw IDs.",
    navLabel: "Places",
    backToHub: "Back to catalog",
    destinationsLink: "Destinations",
    mediaLink: "Media",
    accountsJob: "Accounts & people",
    hubCta: "Manage places",
    stepCreate: "1. Create place",
    stepBrowse: "2. Browse places",
    stepOpenByCode: "Open by Code",
    stepInspect: "3. Inspect",
    stepTranslate: "4. Localized content",
    stepDestination: "5. Destination link (optional)",
    stepGeo: "6. Coordinates",
    stepAddress: "7. Address",
    stepCatalog: "8. Status · classification · facilities",
    stepMedia: "9. Cover & Gallery",
    kindLabel: "Place kind",
    codeLabel: "Place code",
    englishNameLabel: "English name",
    starRatingLabel: "Star rating (Hotel)",
    cuisineTypeLabel: "Cuisine type (Restaurant)",
    categoryCodeLabel: "Category code (Attraction)",
    createAction: "Create",
    kindFilterLabel: "Kind filter",
    kindAll: "All",
    refreshList: "Refresh list",
    takeLabel: "Max items",
    noPlaces: "No places yet — create first.",
    selectPlace: "Open",
    openByCodeAction: "Open",
    openByCodeHint: "Unique catalog Code; not a public slug (P07-R4 still open).",
    selectedTitle: "Selected place",
    metadataHeading: "Metadata",
    statusLabel: "Catalog status",
    classificationLabel: "Classification code",
    facilitiesLabel: "Facilities",
    facilitiesHint: "Comma-separated codes (e.g. wifi,parking).",
    saveCatalog: "Save catalog fields",
    translationLocale: "Translation locale",
    nameLabel: "Localized name",
    descriptionLabel: "Description",
    saveTranslation: "Save translation",
    destinationSlugLocale: "Destination slug locale",
    destinationSlugLabel: "Destination slug",
    resolveDestination: "Resolve destination",
    clearDestination: "Clear link",
    destinationResolved: "Resolved destination",
    saveDestinationLink: "Save destination link",
    latitudeLabel: "Latitude",
    longitudeLabel: "Longitude",
    saveGeo: "Save coordinates",
    clearGeo: "Clear coordinates",
    line1Label: "Line 1",
    line2Label: "Line 2",
    localityLabel: "Locality",
    adminAreaLabel: "Administrative area",
    postalCodeLabel: "Postal code",
    countryCodeLabel: "Country code (ISO alpha-2)",
    saveAddress: "Save address",
    clearAddress: "Clear address",
    coverMediaLabel: "MediaAsset for Cover",
    setCover: "Set Cover",
    removeCover: "Remove Cover",
    galleryMediaLabel: "MediaAsset for Gallery",
    addGallery: "Add to Gallery",
    removeGalleryItem: "Remove from Gallery",
    mediaLinksHeading: "Media links",
    noMediaLinks: "No media links yet.",
    catalogDraft: "Draft",
    catalogActive: "Active",
    catalogInactive: "Inactive",
    unauthorizedBody:
      "Sign in with the secure Identity cookie and hold Place.Places.Write to use this workflow.",
    apiMissing: "API base URL is not configured (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "The operation failed. Technical details are not shown.",
    noDeleteHint:
      "Place delete/archive is out of scope (P07-R3 open). Inactive is catalog ops status only.",
  },
};

export function getAdminPlaceWorkflowCopy(locale: AppLocale): AdminPlaceWorkflowCopy {
  return COPY[locale === "en" ? "en" : "fa"];
}
