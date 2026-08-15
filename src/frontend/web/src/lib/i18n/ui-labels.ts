import { DEFAULT_LOCALE, isAppLocale, type AppLocale } from "@/lib/i18n";

type ErrorCopy = {
  title: string;
  body: string;
  retry: string;
  home: string;
};

type LoadingCopy = {
  loadingLabel: string;
};

type NotFoundCopy = {
  title: string;
  body: string;
  home: string;
};

const ERROR_COPY: Record<AppLocale, ErrorCopy> = {
  fa: {
    title: "مشکلی پیش آمد",
    body: "لطفاً دوباره تلاش کنید. جزئیات فنی نمایش داده نمی‌شود.",
    retry: "تلاش دوباره",
    home: "بازگشت به صفحهٔ اصلی",
  },
  en: {
    title: "Something went wrong",
    body: "Please try again. Technical details are not shown.",
    retry: "Try again",
    home: "Back to home",
  },
  ar: {
    title: "حدث خطأ ما",
    body: "يرجى المحاولة مرة أخرى. لن تُعرض التفاصيل التقنية.",
    retry: "إعادة المحاولة",
    home: "العودة إلى الصفحة الرئيسية",
  },
};

const LOADING_COPY: Record<AppLocale, LoadingCopy> = {
  fa: { loadingLabel: "در حال بارگذاری" },
  en: { loadingLabel: "Loading" },
  ar: { loadingLabel: "جارٍ التحميل" },
};

const NOT_FOUND_COPY: Record<AppLocale, Omit<NotFoundCopy, "home">> = {
  fa: {
    title: "صفحه پیدا نشد",
    body: "این مسیر وجود ندارد یا در دسترس نیست.",
  },
  en: {
    title: "Page not found",
    body: "This path does not exist or is unavailable.",
  },
  ar: {
    title: "الصفحة غير موجودة",
    body: "هذا المسار غير موجود أو غير متاح.",
  },
};

export function getErrorCopy(locale: AppLocale): ErrorCopy {
  return ERROR_COPY[locale];
}

export function getLoadingCopy(locale: AppLocale): LoadingCopy {
  return LOADING_COPY[locale];
}

export function getNotFoundCopy(locale: AppLocale): NotFoundCopy {
  const base = NOT_FOUND_COPY[locale];
  return { ...base, home: ERROR_COPY[locale].home };
}

export function normalizeUiLocale(value: string | null | undefined): AppLocale {
  if (value && isAppLocale(value)) return value;
  return DEFAULT_LOCALE;
}

/** Best-effort locale from request headers (for loading/not-found without params). */
export function localeFromPathname(pathname: string | null | undefined): AppLocale {
  if (!pathname) return DEFAULT_LOCALE;
  const match = pathname.match(/\/(fa|en|ar)(?=\/|$)/);
  return normalizeUiLocale(match?.[1]);
}
