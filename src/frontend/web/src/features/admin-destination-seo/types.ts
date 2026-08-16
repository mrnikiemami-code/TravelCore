export type SeoRouteView = {
  id: string;
  resourceType: string;
  resourceId: string;
  locale: string;
  path: string;
};

export type SeoIndexPolicyView = {
  id: string;
  resourceType: string;
  resourceId: string;
  locale: string;
  indexDirective: string;
  followDirective: string;
  updatedAt: string;
};

export type SeoIndexabilityView = {
  locale: string;
  path: string;
  effectiveIndex: string;
  effectiveFollow: string;
  robotsDirective: string;
  configuredIndex?: string | null;
  configuredFollow?: string | null;
  isIndexable: boolean;
  reasons: string[];
};

export type SeoDestinationPostureView = {
  destinationId: string;
  locale: string;
  routes: SeoRouteView[];
  configuredPolicy: SeoIndexPolicyView | null;
  effectiveIndexability: SeoIndexabilityView | null;
  notes: string;
};
