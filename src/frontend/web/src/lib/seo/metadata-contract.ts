/**
 * SEO-composed page metadata contract (TC-P05-T007).
 * Mirrors TravelCore.Modules.Seo.Contracts.SeoComposedMetadataResponse.
 * Frontend maps this to Next Metadata — does not invent SEO truth.
 */
import type { SeoHreflangAlternate } from "./hreflang-contract";
import { robotsFromIndexability } from "./indexability-contract";

export type SeoComposedMetadata = {
  locale: string;
  path: string;
  title: string;
  description: string | null;
  usedTitleOverride: boolean;
  usedDescriptionOverride: boolean;
  effectiveIndex: "NoIndex" | "Index";
  effectiveFollow: "Follow" | "NoFollow";
  robotsDirective: string;
  isIndexable: boolean;
  indexabilityReasons: string[];
  canonicalHref: string | null;
  hreflangAlternates: SeoHreflangAlternate[];
};

export function languagesFromComposed(
  composed: SeoComposedMetadata,
): Record<string, string> {
  const languages: Record<string, string> = {};
  for (const alt of composed.hreflangAlternates ?? []) {
    if (!alt.locale?.trim() || !alt.href?.trim()) continue;
    languages[alt.locale] = alt.href;
  }
  return languages;
}

export function robotsFromComposed(composed: SeoComposedMetadata): {
  index: boolean;
  follow: boolean;
} {
  return robotsFromIndexability({
    locale: composed.locale,
    path: composed.path,
    effectiveIndex: composed.effectiveIndex,
    effectiveFollow: composed.effectiveFollow,
    robotsDirective: composed.robotsDirective,
    configuredIndex: null,
    configuredFollow: null,
    isIndexable: composed.isIndexable,
    reasons: composed.indexabilityReasons,
  });
}
