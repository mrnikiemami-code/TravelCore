/** Presentation views for the guided Destination Admin workflow (not domain SoT). */

export type DestinationKindView = "Country" | "Region" | "City" | "Area";

export type DestinationSummaryView = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  parentId: string | null;
  isoCountryCode: string | null;
  latitude: number | null;
  longitude: number | null;
  localizedName: string | null;
  localizedDescription: string | null;
  locale: string | null;
};

export type DestinationPathNodeView = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  parentId: string | null;
  depthFromRoot: number;
};

export type DestinationPathView = {
  destinationId: string;
  ancestorsRootFirst: DestinationPathNodeView[];
  self: DestinationPathNodeView;
  breadcrumbRootFirst: DestinationPathNodeView[];
};

export type DestinationTranslationView = {
  destinationId: string;
  localeCode: string;
  name: string;
  description: string | null;
  slug: string | null;
};

export type DestinationSlugHitView = {
  destinationId: string;
  localeCode: string;
  slug: string;
  kind: string;
  code: string;
  englishName: string;
};
