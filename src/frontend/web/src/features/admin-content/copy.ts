import type { AppLocale } from "@/lib/i18n";

export type AdminContentWorkflowCopy = {
  pageTitle: string;
  pageIntro: string;
  navLabel: string;
  backToHub: string;
  placesLink: string;
  mediaLink: string;
  accountsJob: string;
  hubCta: string;
  stepCreate: string;
  stepBrowse: string;
  stepOpenByCode: string;
  stepInspect: string;
  stepTranslate: string;
  stepTaxonomy: string;
  stepBlocks: string;
  stepDestination: string;
  kindLabel: string;
  codeLabel: string;
  englishNameLabel: string;
  createAction: string;
  kindFilterLabel: string;
  kindAll: string;
  refreshList: string;
  takeLabel: string;
  noItems: string;
  selectItem: string;
  openByCodeAction: string;
  openByCodeHint: string;
  selectedTitle: string;
  metadataHeading: string;
  translationLocale: string;
  titleLabel: string;
  bodyLabel: string;
  excerptLabel: string;
  saveTranslation: string;
  slugLabel: string;
  slugHint: string;
  saveSlug: string;
  publishSeoRoute: string;
  publishSeoHint: string;
  categoryCodeLabel: string;
  tagCodeLabel: string;
  createCategory: string;
  createTag: string;
  assignCategory: string;
  removeCategory: string;
  assignTag: string;
  removeTag: string;
  categoriesHeading: string;
  tagsHeading: string;
  blocksHeading: string;
  headingTextLabel: string;
  headingLevelLabel: string;
  paragraphTextLabel: string;
  addHeading: string;
  addParagraph: string;
  addImageBlock: string;
  removeBlock: string;
  reorderUp: string;
  reorderDown: string;
  mediaPickerHeading: string;
  mediaPickerHint: string;
  refreshReadyMedia: string;
  noReadyMedia: string;
  destinationSlugLocale: string;
  destinationSlugLabel: string;
  resolveDestination: string;
  saveDestinationLink: string;
  removeDestination: string;
  destinationResolved: string;
  pending: string;
  unauthorizedBody: string;
  errorGeneric: string;
  apiMissing: string;
};

const fa: AdminContentWorkflowCopy = {
  pageTitle: "مدیریت محتوا",
  pageIntro:
    "ایجاد و ویرایش Article / LandingPage / Guide با ترجمه، اسلاگ محلی، رده/برچسب، بلوک‌های پایه و لینک مقصد. حذف/آرشیو و قابلیت‌های بازِ R6/R7/R8 هنوز قفل‌نشده‌اند.",
  navLabel: "محتوا",
  backToHub: "بازگشت به کاتالوگ",
  placesLink: "مکان‌ها",
  mediaLink: "رسانه",
  accountsJob: "حساب‌ها",
  hubCta: "باز کردن مدیریت محتوا",
  stepCreate: "ایجاد آیتم",
  stepBrowse: "فهرست",
  stepOpenByCode: "باز کردن با کد",
  stepInspect: "جزئیات",
  stepTranslate: "ترجمه",
  stepTaxonomy: "رده و برچسب",
  stepBlocks: "بلوک‌ها",
  stepDestination: "مقصد",
  kindLabel: "نوع",
  codeLabel: "کد",
  englishNameLabel: "نام انگلیسی",
  createAction: "ایجاد",
  kindFilterLabel: "فیلتر نوع",
  kindAll: "همه",
  refreshList: "بروزرسانی فهرست",
  takeLabel: "تعداد",
  noItems: "آیتمی نیست.",
  selectItem: "انتخاب",
  openByCodeAction: "باز کردن",
  openByCodeHint: "کد ContentItem را وارد کنید.",
  selectedTitle: "آیتم انتخاب‌شده",
  metadataHeading: "فراداده",
  translationLocale: "زبان ترجمه",
  titleLabel: "عنوان",
  bodyLabel: "بدنه",
  excerptLabel: "خلاصه",
  saveTranslation: "ذخیره ترجمه",
  slugLabel: "Slug محلی",
  slugHint: "مالکیت فعلی با Content است؛ تاریخچه/ریدایرکت با SEO.",
  saveSlug: "ذخیرهٔ slug",
  publishSeoRoute: "ثبت مسیر SEO",
  publishSeoHint:
    "برای Article مسیر articles/{slug} و برای LandingPage مسیر landing-pages/{slug} را ثبت می‌کند. IndexPolicy پیش‌فرض noindex,follow می‌ماند مگر صریحاً تنظیم شود.",
  categoryCodeLabel: "کد رده",
  tagCodeLabel: "کد برچسب",
  createCategory: "ایجاد رده",
  createTag: "ایجاد برچسب",
  assignCategory: "افزودن رده",
  removeCategory: "حذف رده",
  assignTag: "افزودن برچسب",
  removeTag: "حذف برچسب",
  categoriesHeading: "رده‌ها",
  tagsHeading: "برچسب‌ها",
  blocksHeading: "بلوک‌های محتوا",
  headingTextLabel: "متن عنوان",
  headingLevelLabel: "سطح عنوان",
  paragraphTextLabel: "متن پاراگراف",
  addHeading: "افزودن Heading",
  addParagraph: "افزودن Paragraph",
  addImageBlock: "افزودن Image از Ready",
  removeBlock: "حذف بلوک",
  reorderUp: "بالا",
  reorderDown: "پایین",
  mediaPickerHeading: "رسانه Ready",
  mediaPickerHint: "انتخاب تصویر Ready برای بلوک Image — بدون paste شناسه خام.",
  refreshReadyMedia: "بروزرسانی Ready",
  noReadyMedia: "رسانه Ready نیست.",
  destinationSlugLocale: "زبان اسلاگ مقصد",
  destinationSlugLabel: "اسلاگ مقصد",
  resolveDestination: "یافتن مقصد",
  saveDestinationLink: "لینک مقصد",
  removeDestination: "برداشتن لینک",
  destinationResolved: "مقصد پیدا شد",
  pending: "در حال انجام…",
  unauthorizedBody: "دسترسی ندارید (نیاز به content.items.write).",
  errorGeneric: "خطا در عملیات محتوا.",
  apiMissing: "API پایه پیکربندی نشده است.",
};

const en: AdminContentWorkflowCopy = {
  pageTitle: "Content admin",
  pageIntro:
    "Create and edit Article / LandingPage / Guide with translations, locale slug, category/tag, baseline blocks, and destination links. Delete/archive and open R6/R7/R8 capabilities remain locked out of this baseline.",
  navLabel: "Content",
  backToHub: "Back to catalog",
  placesLink: "Places",
  mediaLink: "Media",
  accountsJob: "Accounts",
  hubCta: "Open content admin",
  stepCreate: "Create item",
  stepBrowse: "Browse",
  stepOpenByCode: "Open by code",
  stepInspect: "Inspect",
  stepTranslate: "Translate",
  stepTaxonomy: "Category & tag",
  stepBlocks: "Blocks",
  stepDestination: "Destination",
  kindLabel: "Kind",
  codeLabel: "Code",
  englishNameLabel: "English name",
  createAction: "Create",
  kindFilterLabel: "Kind filter",
  kindAll: "All",
  refreshList: "Refresh list",
  takeLabel: "Take",
  noItems: "No content items.",
  selectItem: "Select",
  openByCodeAction: "Open",
  openByCodeHint: "Enter a ContentItem code.",
  selectedTitle: "Selected item",
  metadataHeading: "Metadata",
  translationLocale: "Translation locale",
  titleLabel: "Title",
  bodyLabel: "Body",
  excerptLabel: "Excerpt",
  saveTranslation: "Save translation",
  slugLabel: "Localized slug",
  slugHint: "Content owns current slug; SEO owns history/redirects.",
  saveSlug: "Save slug",
  publishSeoRoute: "Publish SEO route",
  publishSeoHint:
    "Registers articles/{slug} or landing-pages/{slug} in the SEO namespace. Default IndexPolicy stays noindex,follow until set explicitly.",
  categoryCodeLabel: "Category code",
  tagCodeLabel: "Tag code",
  createCategory: "Create category",
  createTag: "Create tag",
  assignCategory: "Assign category",
  removeCategory: "Remove category",
  assignTag: "Assign tag",
  removeTag: "Remove tag",
  categoriesHeading: "Categories",
  tagsHeading: "Tags",
  blocksHeading: "Content blocks",
  headingTextLabel: "Heading text",
  headingLevelLabel: "Heading level",
  paragraphTextLabel: "Paragraph text",
  addHeading: "Add heading",
  addParagraph: "Add paragraph",
  addImageBlock: "Add image from Ready",
  removeBlock: "Remove block",
  reorderUp: "Up",
  reorderDown: "Down",
  mediaPickerHeading: "Ready media",
  mediaPickerHint: "Pick a Ready asset for an Image block — no raw ID paste.",
  refreshReadyMedia: "Refresh Ready",
  noReadyMedia: "No Ready media.",
  destinationSlugLocale: "Destination slug locale",
  destinationSlugLabel: "Destination slug",
  resolveDestination: "Resolve destination",
  saveDestinationLink: "Link destination",
  removeDestination: "Unlink destination",
  destinationResolved: "Destination resolved",
  pending: "Working…",
  unauthorizedBody: "Unauthorized (requires content.items.write).",
  errorGeneric: "Content operation failed.",
  apiMissing: "API base URL is not configured.",
};

export function getAdminContentWorkflowCopy(
  locale: AppLocale,
): AdminContentWorkflowCopy {
  return locale === "en" ? en : fa;
}
