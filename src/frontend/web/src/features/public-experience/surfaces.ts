export const PUBLIC_EXPERIENCE_SURFACES = ["detail", "listing", "landing"] as const;

export type PublicExperienceSurface = (typeof PUBLIC_EXPERIENCE_SURFACES)[number];

/** P14-R1: Public Experience Layer owns presentation surfaces. Not Search. Not catalog. */
export const PUBLIC_EXPERIENCE_OWNER = "PublicExperience" as const;
export const PUBLIC_EXPERIENCE_CATALOG_OWNER = "Tour" as const;
export const PUBLIC_EXPERIENCE_SEARCH_OWNER = "Search" as const;
