import type { AppLocale } from "@/lib/i18n";

export type AdminMediaWorkflowCopy = {
  pageTitle: string;
  pageIntro: string;
  navLabel: string;
  backToCatalog: string;
  accountsJob: string;
  stepUpload: string;
  stepBrowse: string;
  stepInspect: string;
  stepTranslate: string;
  stepFocal: string;
  stepVariants: string;
  fileLabel: string;
  uploadAction: string;
  uploadHint: string;
  statusFilterLabel: string;
  statusAll: string;
  statusReady: string;
  statusPending: string;
  statusFailed: string;
  refreshList: string;
  takeLabel: string;
  noAssets: string;
  selectAsset: string;
  selectedTitle: string;
  metadataHeading: string;
  contentTypeLabel: string;
  dimensionsLabel: string;
  byteSizeLabel: string;
  statusLabel: string;
  createdLabel: string;
  previewLabel: string;
  previewUnavailable: string;
  variantsHeading: string;
  generateVariants: string;
  noVariants: string;
  variantProfile: string;
  variantStatus: string;
  variantFailure: string;
  translationLocale: string;
  altLabel: string;
  captionLabel: string;
  publicationLabel: string;
  publicationDraft: string;
  publicationReady: string;
  publicationPublished: string;
  publicationArchived: string;
  saveTranslation: string;
  focalHint: string;
  focalXLabel: string;
  focalYLabel: string;
  saveFocal: string;
  clearFocal: string;
  pickFocalOnPreview: string;
  unauthorizedBody: string;
  apiMissing: string;
  errorGeneric: string;
  svgDeniedHint: string;
};

const COPY: Record<"fa" | "en", AdminMediaWorkflowCopy> = {
  fa: {
    pageTitle: "مدیریت رسانه",
    pageIntro:
      "بارگذاری، بازرسی متادیتا/نسخه‌ها، ویرایش alt/caption و نقطهٔ کانونی — بدون وابستگی به Place یا Tour CMS. شناسهٔ خام مسیر اصلی نیست.",
    navLabel: "رسانه",
    backToCatalog: "بازگشت به کاتالوگ",
    accountsJob: "حساب‌ها و افراد",
    stepUpload: "۱. بارگذاری دارایی",
    stepBrowse: "۲. فهرست دارایی‌ها",
    stepInspect: "۳. بازرسی و پیش‌نمایش",
    stepTranslate: "۴. alt و caption محلی",
    stepFocal: "۵. نقطهٔ کانونی",
    stepVariants: "نسخه‌های مشتق",
    fileLabel: "فایل تصویر",
    uploadAction: "بارگذاری",
    uploadHint: "فقط قالب‌های مجاز رستر (مثلاً PNG/JPEG/WebP). SVG رد می‌شود.",
    statusFilterLabel: "وضعیت پردازش",
    statusAll: "همه",
    statusReady: "Ready",
    statusPending: "PendingStorage",
    statusFailed: "Failed",
    refreshList: "تازه‌سازی فهرست",
    takeLabel: "حداکثر تعداد",
    noAssets: "دارایی‌ای نیست — ابتدا بارگذاری کنید.",
    selectAsset: "باز کردن",
    selectedTitle: "دارایی انتخاب‌شده",
    metadataHeading: "متادیتا",
    contentTypeLabel: "نوع محتوا",
    dimensionsLabel: "ابعاد",
    byteSizeLabel: "حجم",
    statusLabel: "وضعیت",
    createdLabel: "ایجاد",
    previewLabel: "پیش‌نمایش (app-proxy)",
    previewUnavailable: "پیش‌نمایش فقط برای دارایی Ready در دسترس است.",
    variantsHeading: "نسخه‌ها (large / medium / thumbnail)",
    generateVariants: "تولید هم‌زمان نسخه‌ها",
    noVariants: "نسخه‌ای ثبت نشده — تولید را اجرا کنید.",
    variantProfile: "پروفایل",
    variantStatus: "وضعیت",
    variantFailure: "دلیل شکست",
    translationLocale: "زبان ترجمه",
    altLabel: "متن جایگزین (alt)",
    captionLabel: "عنوان/توضیح (caption)",
    publicationLabel: "وضعیت انتشار",
    publicationDraft: "Draft",
    publicationReady: "Ready",
    publicationPublished: "Published",
    publicationArchived: "Archived",
    saveTranslation: "ذخیرهٔ ترجمه",
    focalHint:
      "مختصات نسبی [۰٫۰–۱٫۰] با مبدأ گوشهٔ بالا-چپ. روی پیش‌نمایش کلیک کنید یا مقادیر را وارد کنید.",
    focalXLabel: "Focal X",
    focalYLabel: "Focal Y",
    saveFocal: "ذخیرهٔ نقطهٔ کانونی",
    clearFocal: "پاک کردن نقطهٔ کانونی",
    pickFocalOnPreview: "کلیک برای انتخاب نقطهٔ کانونی روی پیش‌نمایش",
    unauthorizedBody:
      "برای این گردش کار باید با Cookie امن احراز هویت شده باشید و مجوز Media.Assets.Write داشته باشید.",
    apiMissing: "آدرس API پیکربندی نشده است (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "عملیات انجام نشد. جزئیات فنی نمایش داده نمی‌شود.",
    svgDeniedHint: "فایل SVG پذیرفته نمی‌شود.",
  },
  en: {
    pageTitle: "Media assets",
    pageIntro:
      "Upload, inspect metadata/variants, edit alt/caption, and set focal point — without Place or Tour CMS. Raw IDs are not the primary path.",
    navLabel: "Media",
    backToCatalog: "Back to catalog",
    accountsJob: "Accounts & people",
    stepUpload: "1. Upload asset",
    stepBrowse: "2. Browse assets",
    stepInspect: "3. Inspect & preview",
    stepTranslate: "4. Localized alt & caption",
    stepFocal: "5. Focal point",
    stepVariants: "Derived variants",
    fileLabel: "Image file",
    uploadAction: "Upload",
    uploadHint: "Allowlisted raster formats only (e.g. PNG/JPEG/WebP). SVG is denied.",
    statusFilterLabel: "Processing status",
    statusAll: "All",
    statusReady: "Ready",
    statusPending: "PendingStorage",
    statusFailed: "Failed",
    refreshList: "Refresh list",
    takeLabel: "Max items",
    noAssets: "No assets yet — upload first.",
    selectAsset: "Open",
    selectedTitle: "Selected asset",
    metadataHeading: "Metadata",
    contentTypeLabel: "Content type",
    dimensionsLabel: "Dimensions",
    byteSizeLabel: "Size",
    statusLabel: "Status",
    createdLabel: "Created",
    previewLabel: "Preview (app-proxy)",
    previewUnavailable: "Preview is available only for Ready assets.",
    variantsHeading: "Variants (large / medium / thumbnail)",
    generateVariants: "Generate variants (sync)",
    noVariants: "No variants yet — run generation.",
    variantProfile: "Profile",
    variantStatus: "Status",
    variantFailure: "Failure reason",
    translationLocale: "Translation locale",
    altLabel: "Alt text",
    captionLabel: "Caption",
    publicationLabel: "Publication status",
    publicationDraft: "Draft",
    publicationReady: "Ready",
    publicationPublished: "Published",
    publicationArchived: "Archived",
    saveTranslation: "Save translation",
    focalHint:
      "Normalized [0.0–1.0] coordinates, origin top-left. Click the preview or enter values.",
    focalXLabel: "Focal X",
    focalYLabel: "Focal Y",
    saveFocal: "Save focal point",
    clearFocal: "Clear focal point",
    pickFocalOnPreview: "Click preview to pick focal point",
    unauthorizedBody:
      "Sign in with the secure Identity cookie and hold Media.Assets.Write to use this workflow.",
    apiMissing: "API base URL is not configured (TRAVELCORE_API_BASE_URL).",
    errorGeneric: "The operation failed. Technical details are not shown.",
    svgDeniedHint: "SVG files are not accepted.",
  },
};

export function getAdminMediaWorkflowCopy(locale: AppLocale): AdminMediaWorkflowCopy {
  return COPY[locale === "en" ? "en" : "fa"];
}
