import type { AppLocale } from "@/lib/i18n";

export type AdminTourWorkflowCopy = {
  pageTitle: string;
  pageIntro: string;
  navLabel: string;
  backToHub: string;
  placesLink: string;
  contentLink: string;
  mediaLink: string;
  accountsJob: string;
  hubCta: string;
  stepCreate: string;
  stepBrowse: string;
  stepOpenByCode: string;
  stepInspect: string;
  stepTranslate: string;
  stepCatalog: string;
  stepMedia: string;
  kindLabel: string;
  codeLabel: string;
  englishNameLabel: string;
  createAction: string;
  kindFilterLabel: string;
  kindAll: string;
  refreshList: string;
  takeLabel: string;
  noTours: string;
  selectTour: string;
  openByCodeAction: string;
  openByCodeHint: string;
  selectedTitle: string;
  statusLabel: string;
  classificationLabel: string;
  saveClassification: string;
  saveStatus: string;
  translationLocale: string;
  titleLabel: string;
  descriptionLabel: string;
  slugLabel: string;
  slugHint: string;
  saveTranslation: string;
  saveSlug: string;
  publishSeoRoute: string;
  publishSeoHint: string;
  mediaHeading: string;
  mediaReadyHint: string;
  refreshReadyMedia: string;
  setCover: string;
  addGallery: string;
  removeCover: string;
  apiMissing: string;
  busy: string;
  errorPrefix: string;
};

const fa: AdminTourWorkflowCopy = {
  pageTitle: "مدیریت کاتالوگ تور",
  pageIntro:
    "ایجاد و ویرایش TourProduct · وضعیت انتشار · ترجمه/اسلاگ · رسانه Ready. بدون Departure/Booking/Pricing.",
  navLabel: "تور",
  backToHub: "بازگشت به کاتالوگ",
  placesLink: "مکان‌ها",
  contentLink: "محتوا",
  mediaLink: "رسانه",
  accountsJob: "حساب‌ها",
  hubCta: "مدیریت تور",
  stepCreate: "۱) ایجاد تور",
  stepBrowse: "۲) فهرست",
  stepOpenByCode: "۳) باز کردن با کد",
  stepInspect: "۴) جزئیات",
  stepTranslate: "۵) ترجمه و اسلاگ",
  stepCatalog: "۶) وضعیت و طبقه‌بندی",
  stepMedia: "۷) رسانه (Ready)",
  kindLabel: "نوع",
  codeLabel: "کد",
  englishNameLabel: "نام انگلیسی",
  createAction: "ایجاد",
  kindFilterLabel: "فیلتر نوع",
  kindAll: "همه",
  refreshList: "بروزرسانی فهرست",
  takeLabel: "تعداد",
  noTours: "توری نیست.",
  selectTour: "انتخاب",
  openByCodeAction: "باز کردن",
  openByCodeHint: "کد دقیق TourProduct",
  selectedTitle: "تور انتخاب‌شده",
  statusLabel: "وضعیت کاتالوگ",
  classificationLabel: "کد طبقه‌بندی",
  saveClassification: "ذخیره طبقه‌بندی",
  saveStatus: "ذخیره وضعیت",
  translationLocale: "زبان ترجمه",
  titleLabel: "عنوان",
  descriptionLabel: "توضیح",
  slugLabel: "اسلاگ",
  slugHint: "مالک فعلی اسلاگ: Tour · تاریخچه/IndexPolicy: SEO",
  saveTranslation: "ذخیره ترجمه",
  saveSlug: "ذخیره اسلاگ",
  publishSeoRoute: "انتشار مسیر SEO",
  publishSeoHint: "مسیر tours/{slug} · IndexPolicy پیش‌فرض noindex,follow",
  mediaHeading: "Cover / Gallery",
  mediaReadyHint: "فقط MediaAsset با وضعیت Ready — بدون ورود دستی شناسه",
  refreshReadyMedia: "بارگذاری Ready",
  setCover: "تنظیم Cover",
  addGallery: "افزودن به Gallery",
  removeCover: "حذف Cover",
  apiMissing: "API پیکربندی نشده است.",
  busy: "در حال اجرا…",
  errorPrefix: "خطا",
};

const en: AdminTourWorkflowCopy = {
  pageTitle: "Tour catalog admin",
  pageIntro:
    "Create/edit TourProduct · catalog status · translation/slug · Ready media. No Departure/Booking/Pricing.",
  navLabel: "Tours",
  backToHub: "Back to catalog hub",
  placesLink: "Places",
  contentLink: "Content",
  mediaLink: "Media",
  accountsJob: "Accounts",
  hubCta: "Manage tours",
  stepCreate: "1) Create tour",
  stepBrowse: "2) Browse",
  stepOpenByCode: "3) Open by code",
  stepInspect: "4) Inspect",
  stepTranslate: "5) Translation + slug",
  stepCatalog: "6) Status + classification",
  stepMedia: "7) Media (Ready)",
  kindLabel: "Kind",
  codeLabel: "Code",
  englishNameLabel: "English name",
  createAction: "Create",
  kindFilterLabel: "Kind filter",
  kindAll: "All",
  refreshList: "Refresh list",
  takeLabel: "Take",
  noTours: "No tours yet.",
  selectTour: "Select",
  openByCodeAction: "Open",
  openByCodeHint: "Exact TourProduct code",
  selectedTitle: "Selected tour",
  statusLabel: "Catalog status",
  classificationLabel: "Classification code",
  saveClassification: "Save classification",
  saveStatus: "Save status",
  translationLocale: "Translation locale",
  titleLabel: "Title",
  descriptionLabel: "Description",
  slugLabel: "Slug",
  slugHint: "Current slug owned by Tour · history/IndexPolicy owned by SEO",
  saveTranslation: "Save translation",
  saveSlug: "Save slug",
  publishSeoRoute: "Publish SEO route",
  publishSeoHint: "Path tours/{slug} · default IndexPolicy noindex,follow",
  mediaHeading: "Cover / Gallery",
  mediaReadyHint: "Ready MediaAssets only — no raw ID paste",
  refreshReadyMedia: "Load Ready media",
  setCover: "Set cover",
  addGallery: "Add to gallery",
  removeCover: "Remove cover",
  apiMissing: "API is not configured.",
  busy: "Working…",
  errorPrefix: "Error",
};

export function getAdminTourWorkflowCopy(locale: AppLocale): AdminTourWorkflowCopy {
  return locale === "fa" ? fa : en;
}
