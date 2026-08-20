import type { AppLocale } from "@/lib/i18n";
import type { RelatedTourView } from "@/features/public-experience/load-related-tours";

export type TourListingSort = "name-asc" | "name-desc" | "kind-asc";

export type TourListingCriteria = {
  destination: string;
  q: string;
  sort: TourListingSort;
};

export function parseTourListingCriteria(
  searchParams: Record<string, string | string[] | undefined> | undefined,
): TourListingCriteria {
  const rawDest = searchParams?.destination;
  const rawQ = searchParams?.q;
  const rawSort = searchParams?.sort;
  const destination = (Array.isArray(rawDest) ? rawDest[0] : rawDest)?.trim() ?? "";
  const q = (Array.isArray(rawQ) ? rawQ[0] : rawQ)?.trim() ?? "";
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

export function tourListingCopy(locale: AppLocale) {
  if (locale === "fa") {
    return {
      destinationLabel: "مقصد",
      destinationPlaceholder: "مثلاً istanbul",
      filterLabel: "جستجوی نام",
      filterPlaceholder: "نام تور یا کد…",
      sortLabel: "مرتب‌سازی",
      apply: "اعمال",
      sortNameAsc: "نام (الف→ی)",
      sortNameDesc: "نام (ی→الف)",
      sortKindAsc: "نوع تور",
      patternNote: "فیلتر فهرست تورها · بر اساس مقصد منتشرشده",
    };
  }
  if (locale === "ar") {
    return {
      destinationLabel: "الوجهة",
      destinationPlaceholder: "مثلاً istanbul",
      filterLabel: "بحث بالاسم",
      filterPlaceholder: "اسم الجولة أو الرمز…",
      sortLabel: "الترتيب",
      apply: "تطبيق",
      sortNameAsc: "الاسم (أ→ي)",
      sortNameDesc: "الاسم (ي→أ)",
      sortKindAsc: "نوع الجولة",
      patternNote: "تصفية قائمة الجولات · حسب الوجهة المنشورة",
    };
  }
  return {
    destinationLabel: "Destination",
    destinationPlaceholder: "e.g. istanbul",
    filterLabel: "Search by name",
    filterPlaceholder: "Tour name or code…",
    sortLabel: "Sort",
    apply: "Apply",
    sortNameAsc: "Name (A→Z)",
    sortNameDesc: "Name (Z→A)",
    sortKindAsc: "Tour kind",
    patternNote: "Filter tour list · published destination",
  };
}
