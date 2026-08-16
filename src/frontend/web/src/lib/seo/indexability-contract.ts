/**
 * Frontend metadata integration contract for SEO IndexPolicy (TC-P05-T005 / R2).
 *
 * Backend endpoint: GET /api/seo/indexability/{locale}/{*path}
 * Shape mirrors TravelCore.Modules.Seo.Contracts.SeoIndexabilityResponse.
 *
 * Default (missing policy) = noindex, follow.
 * Public Destination pages (P04) remain hardcoded noindex until a later task
 * (e.g. T007 metadata composition) consumes this contract — do not mass-flip.
 */
export type SeoIndexabilityContract = {
  locale: string;
  path: string;
  effectiveIndex: "NoIndex" | "Index";
  effectiveFollow: "Follow" | "NoFollow";
  /** e.g. "noindex, follow" or "index, follow" */
  robotsDirective: string;
  configuredIndex: "NoIndex" | "Index" | null;
  configuredFollow: "Follow" | "NoFollow" | null;
  isIndexable: boolean;
  reasons: string[];
};

export function robotsFromIndexability(evaluation: SeoIndexabilityContract): {
  index: boolean;
  follow: boolean;
} {
  return {
    index: evaluation.effectiveIndex === "Index",
    follow: evaluation.effectiveFollow === "Follow",
  };
}
