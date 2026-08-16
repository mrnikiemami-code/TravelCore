import type { AppLocale } from "@/lib/i18n";

export type DestinationSeoPostureCopy = {
  stepTitle: string;
  intro: string;
  publishNote: string;
  localeLabel: string;
  refresh: string;
  routesLabel: string;
  noRoutes: string;
  configuredLabel: string;
  missingPolicy: string;
  effectiveLabel: string;
  robotsLabel: string;
  indexableYes: string;
  indexableNo: string;
  indexDirectiveLabel: string;
  followDirectiveLabel: string;
  savePolicy: string;
  republish: string;
  slugForPublish: string;
  unauthorizedBody: string;
  errorGeneric: string;
  needFocus: string;
};

const COPY: Record<"fa" | "en", DestinationSeoPostureCopy> = {
  fa: {
    stepTitle: "۶. وضعیت سئوی مقصد",
    intro:
      "بازبینی مسیر منتشرشده و IndexPolicy — منوی CRUD جدا برای جداول سئو نیست. انتشار مسیر ≠ ایندکس.",
    publishNote:
      "اسلاگ همچنان متعلق به Destination است؛ سئو فقط مسیر عمومی و سیاست ایندکس را مدیریت می‌کند.",
    localeLabel: "زبان وضعیت سئو",
    refresh: "بازخوانی وضعیت",
    routesLabel: "مسیرهای منتشرشده",
    noRoutes: "هنوز مسیر عمومی برای این زبان منتشر نشده است.",
    configuredLabel: "IndexPolicy پیکربندی‌شده",
    missingPolicy: "سیاست صریح نیست → پیش‌فرض noindex, follow (R2).",
    effectiveLabel: "وضعیت مؤثر",
    robotsLabel: "robots",
    indexableYes: "قابل ایندکس (پس از eligibility)",
    indexableNo: "غیرقابل ایندکس",
    indexDirectiveLabel: "Index",
    followDirectiveLabel: "Follow",
    savePolicy: "ذخیره IndexPolicy",
    republish: "انتشار/بازنشر مسیر از اسلاگ",
    slugForPublish: "اسلاگ برای انتشار",
    unauthorizedBody:
      "برای مدیریت وضعیت سئو باید احراز هویت شده و مجوز seo.destination-posture.write داشته باشید.",
    errorGeneric: "عملیات سئو انجام نشد. جزئیات فنی نمایش داده نمی‌شود.",
    needFocus: "ابتدا یک مقصد را با اسلاگ باز کنید.",
  },
  en: {
    stepTitle: "6. Destination SEO posture",
    intro:
      "Inspect published route and IndexPolicy — not silo CRUD for SEO tables. Publish ≠ Index.",
    publishNote:
      "Slug remains Destination-owned; SEO owns public path namespace and index posture.",
    localeLabel: "SEO posture locale",
    refresh: "Refresh posture",
    routesLabel: "Published routes",
    noRoutes: "No public route published for this locale yet.",
    configuredLabel: "Configured IndexPolicy",
    missingPolicy: "No explicit policy → default noindex, follow (R2).",
    effectiveLabel: "Effective posture",
    robotsLabel: "robots",
    indexableYes: "Indexable (when eligible)",
    indexableNo: "Not indexable",
    indexDirectiveLabel: "Index",
    followDirectiveLabel: "Follow",
    savePolicy: "Save IndexPolicy",
    republish: "Publish/republish path from slug",
    slugForPublish: "Slug to publish",
    unauthorizedBody:
      "SEO posture requires an authenticated session with seo.destination-posture.write.",
    errorGeneric: "SEO operation failed. Technical details are not shown.",
    needFocus: "Open a destination by slug first.",
  },
};

export function getDestinationSeoPostureCopy(
  locale: AppLocale,
): DestinationSeoPostureCopy {
  return locale === "fa" ? COPY.fa : COPY.en;
}
