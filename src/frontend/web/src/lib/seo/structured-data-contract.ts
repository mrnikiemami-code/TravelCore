/**
 * SEO breadcrumb JSON-LD contract (TC-P05-T008).
 * Projection only — never invent ratings/prices/Tour schema.
 */
export type SeoBreadcrumbNodeInput = {
  name: string;
  publicPath?: string | null;
};

export type SeoBreadcrumbListItem = {
  "@type": "ListItem";
  position: number;
  name: string;
  item?: string | null;
};

export type SeoBreadcrumbListJsonLd = {
  "@context": "https://schema.org";
  "@type": "BreadcrumbList";
  itemListElement: SeoBreadcrumbListItem[];
};

/** Serialize for <script type="application/ld+json"> — omit null item fields. */
export function serializeBreadcrumbJsonLd(
  doc: SeoBreadcrumbListJsonLd | null | undefined,
): string | null {
  if (!doc?.itemListElement?.length) return null;
  const cleaned = {
    "@context": doc["@context"],
    "@type": doc["@type"],
    itemListElement: doc.itemListElement.map((el) => {
      const base: Record<string, unknown> = {
        "@type": el["@type"],
        position: el.position,
        name: el.name,
      };
      if (el.item) base.item = el.item;
      return base;
    }),
  };
  return JSON.stringify(cleaned);
}
