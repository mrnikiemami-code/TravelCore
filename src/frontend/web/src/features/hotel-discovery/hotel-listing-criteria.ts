import type { AppLocale } from "@/lib/i18n";
import type { HotelBrowseItemView } from "@/features/hotel-discovery/load-hotel-discovery-list";

export type HotelListingSort = "name-asc" | "name-desc" | "stars-desc" | "stars-asc";

export type HotelListingCriteria = {
  q: string;
  sort: HotelListingSort;
};

export function parseHotelListingCriteria(
  searchParams: Record<string, string | string[] | undefined> | undefined,
): HotelListingCriteria {
  const rawQ = searchParams?.q;
  const rawSort = searchParams?.sort;
  const q = (Array.isArray(rawQ) ? rawQ[0] : rawQ)?.trim() ?? "";
  const sortRaw = (Array.isArray(rawSort) ? rawSort[0] : rawSort) ?? "name-asc";
  const sort: HotelListingSort =
    sortRaw === "name-desc" ||
    sortRaw === "stars-desc" ||
    sortRaw === "stars-asc" ||
    sortRaw === "name-asc"
      ? sortRaw
      : "name-asc";
  return { q, sort };
}

export function applyHotelListingCriteria(
  hotels: HotelBrowseItemView[],
  criteria: HotelListingCriteria,
): HotelBrowseItemView[] {
  const q = criteria.q.toLowerCase();
  let list = hotels;
  if (q) {
    list = list.filter(
      (h) =>
        h.name.toLowerCase().includes(q) ||
        (h.description?.toLowerCase().includes(q) ?? false) ||
        h.slug.toLowerCase().includes(q),
    );
  }

  const sorted = [...list];
  sorted.sort((a, b) => {
    switch (criteria.sort) {
      case "name-desc":
        return b.name.localeCompare(a.name);
      case "stars-desc":
        return (b.starRating ?? -1) - (a.starRating ?? -1);
      case "stars-asc":
        return (a.starRating ?? 999) - (b.starRating ?? 999);
      case "name-asc":
      default:
        return a.name.localeCompare(b.name);
    }
  });
  return sorted;
}

export function hotelListingCopy(locale: AppLocale) {
  if (locale === "fa") {
    return {
      filterLabel: "فیلتر نام",
      filterPlaceholder: "جستجو در نام یا توضیح…",
      sortLabel: "مرتب‌سازی",
      apply: "اعمال",
      sortNameAsc: "نام (الف→ی)",
      sortNameDesc: "نام (ی→الف)",
      sortStarsDesc: "ستاره (بیشتر)",
      sortStarsAsc: "ستاره (کمتر)",
      patternNote: "الگوی فیلتر/مرتب‌سازی کاتالوگ · نه موتور جستجوی availability",
    };
  }
  if (locale === "ar") {
    return {
      filterLabel: "تصفية الاسم",
      filterPlaceholder: "ابحث في الاسم أو الوصف…",
      sortLabel: "الترتيب",
      apply: "تطبيق",
      sortNameAsc: "الاسم (أ→ي)",
      sortNameDesc: "الاسم (ي→أ)",
      sortStarsDesc: "النجوم (الأعلى)",
      sortStarsAsc: "النجوم (الأقل)",
      patternNote: "نمط التصفية/الترتيب للكتالوج · ليس محرك توفر",
    };
  }
  return {
    filterLabel: "Name filter",
    filterPlaceholder: "Search name or description…",
    sortLabel: "Sort",
    apply: "Apply",
    sortNameAsc: "Name (A→Z)",
    sortNameDesc: "Name (Z→A)",
    sortStarsDesc: "Stars (high)",
    sortStarsAsc: "Stars (low)",
    patternNote: "Catalog filter/sort pattern · not an availability search engine",
  };
}
