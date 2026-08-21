import type { AppLocale } from "@/lib/i18n";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";
import { labelForDestinationSlug } from "@/features/tour-discovery/tour-destination-options";

export type TourListingSort = "name-asc" | "name-desc" | "kind-asc";

export type TourListingCriteria = {
  destination: string;
  q: string;
  sort: TourListingSort;
};

function sanitizeText(value: string | undefined): string {
  const raw = value?.trim() ?? "";
  if (
    raw === "" ||
    raw.toLowerCase() === "undefined" ||
    raw.toLowerCase() === "null"
  ) {
    return "";
  }
  return raw;
}

export function parseTourListingCriteria(
  searchParams: Record<string, string | string[] | undefined> | undefined,
): TourListingCriteria {
  const rawDest = searchParams?.destination;
  const rawQ = searchParams?.q;
  const rawSort = searchParams?.sort;
  const destination = sanitizeText(
    Array.isArray(rawDest) ? rawDest[0] : rawDest,
  );
  const q = sanitizeText(Array.isArray(rawQ) ? rawQ[0] : rawQ);
  const sortRaw = (Array.isArray(rawSort) ? rawSort[0] : rawSort) ?? "name-asc";
  const sort: TourListingSort =
    sortRaw === "name-desc" || sortRaw === "kind-asc" || sortRaw === "name-asc"
      ? sortRaw
      : "name-asc";
  return { destination, q, sort };
}

export function applyTourListingCriteria(
  tours: RelatedTourView[],
  criteria: TourListingCriteria,
): RelatedTourView[] {
  const q = criteria.q.toLowerCase();
  let list = tours;
  if (q) {
    list = list.filter(
      (t) =>
        t.name.toLowerCase().includes(q) ||
        t.code.toLowerCase().includes(q) ||
        t.slug.toLowerCase().includes(q) ||
        t.kind.toLowerCase().includes(q),
    );
  }
  const sorted = [...list];
  sorted.sort((a, b) => {
    switch (criteria.sort) {
      case "name-desc":
        return b.name.localeCompare(a.name);
      case "kind-asc":
        return a.kind.localeCompare(b.kind) || a.name.localeCompare(b.name);
      case "name-asc":
      default:
        return a.name.localeCompare(b.name);
    }
  });
  return sorted;
}

export function humanDestinationLabel(
  locale: AppLocale,
  destinationSlug: string,
): string {
  if (!destinationSlug) return "";
  return labelForDestinationSlug(locale, destinationSlug);
}

export function tourListingCopy(locale: AppLocale) {
  if (locale === "fa") {
    return {
      destinationLabel: "مقصد",
      destinationPlaceholder: "یک مقصد را انتخاب کنید",
      destinationAny: "انتخاب مقصد…",
      filterLabel: "جستجوی نام",
      filterPlaceholder: "نام تور…",
      sortLabel: "مرتب‌سازی",
      apply: "نمایش تورها",
      sortNameAsc: "نام (الف→ی)",
      sortNameDesc: "نام (ی→الف)",
      sortKindAsc: "نوع تور",
      patternNote: "مقصدهای منتشرشده · بدون موتور جستجوی جعلی",
    };
  }
  if (locale === "ar") {
    return {
      destinationLabel: "الوجهة",
      destinationPlaceholder: "اختر وجهة",
      destinationAny: "اختر وجهة…",
      filterLabel: "بحث بالاسم",
      filterPlaceholder: "اسم الجولة…",
      sortLabel: "الترتيب",
      apply: "عرض الجولات",
      sortNameAsc: "الاسم (أ→ي)",
      sortNameDesc: "الاسم (ي→أ)",
      sortKindAsc: "نوع الجولة",
      patternNote: "وجهات منشورة · بلا محرك بحث وهمي",
    };
  }
  return {
    destinationLabel: "Destination",
    destinationPlaceholder: "Choose a destination",
    destinationAny: "Choose a destination…",
    filterLabel: "Search by name",
    filterPlaceholder: "Tour name…",
    sortLabel: "Sort",
    apply: "Show tours",
    sortNameAsc: "Name (A→Z)",
    sortNameDesc: "Name (Z→A)",
    sortKindAsc: "Tour kind",
    patternNote: "Published destinations · no fake search engine",
  };
}
