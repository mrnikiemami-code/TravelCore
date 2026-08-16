import { apiSendJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type { SeoComposedMetadata } from "@/lib/seo/metadata-contract";

export type ComposeSeoMetadataInput = {
  locale: string;
  path: string;
  localizedTitle: string;
  localizedDescription?: string | null;
};

/**
 * Server-only: compose Destination (or peer) content through SEO metadata rules.
 * Missing API → null (caller may fall back conservatively without inventing SEO).
 */
export async function loadComposedSeoMetadata(
  input: ComposeSeoMetadataInput,
): Promise<SeoComposedMetadata | null> {
  const result = await apiSendJson<SeoComposedMetadata>("/api/seo/metadata/compose", {
    method: "POST",
    body: {
      locale: input.locale,
      path: input.path,
      localizedTitle: input.localizedTitle,
      localizedDescription: input.localizedDescription ?? null,
    },
    cache: "no-store",
  });

  if (!isApiOk(result)) {
    return null;
  }

  return result.data;
}
