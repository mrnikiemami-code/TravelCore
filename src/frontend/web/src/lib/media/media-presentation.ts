import { getApiBaseUrl } from "@/lib/api/config";
import type { MediaImagePresentation } from "@/types/media-image";

/** Public app-proxy profiles (P06-R4). Matches backend lowercase URL segments. */
export type MediaVariantProfileName = "large" | "medium" | "thumbnail";

/**
 * Stable Media identity → app-proxy path (never StorageKey / object-storage host).
 * TC-P06-T009 / P06-R4 APP PROXY.
 */
export function mediaOriginalContentPath(mediaAssetId: string): string {
  return `/api/media/assets/${mediaAssetId}/content`;
}

export function mediaVariantContentPath(
  mediaAssetId: string,
  profile: MediaVariantProfileName,
): string {
  return `/api/media/assets/${mediaAssetId}/variants/${profile}/content`;
}

/**
 * Resolve a Media app-proxy path to a URL suitable for `MediaImagePresentation.src`.
 *
 * - Prefer relative same-origin paths when API base is unset (no remotePatterns needed).
 * - When `TRAVELCORE_API_BASE_URL` / `API_BASE_URL` is set, prepend that public API origin
 *   so next/image can fetch across origins (narrow allowlist only — see next.config.ts).
 */
export function resolveMediaAppProxySrc(appProxyPath: string): string {
  if (!appProxyPath.startsWith("/")) {
    throw new Error("Media app-proxy path must be root-relative (start with /).");
  }
  const base = getApiBaseUrl();
  if (!base) {
    return appProxyPath;
  }
  return `${base}${appProxyPath}`;
}

export type BuildMediaImagePresentationInput = {
  mediaAssetId: string;
  /** Prefer a Ready variant when available; use `original` only when explicitly requested. */
  representation: MediaVariantProfileName | "original";
  alt: string;
  width?: number;
  height?: number;
  aspectRatio?: MediaImagePresentation["aspectRatio"];
  sizes?: string;
  priority?: boolean;
};

/**
 * Map Media-owned identity + metadata → P02 `MediaImagePresentation`.
 * Does not invent alt text; caller supplies exact-locale alt (or `""` decorative).
 */
export function buildMediaImagePresentation(
  input: BuildMediaImagePresentationInput,
): MediaImagePresentation {
  const path =
    input.representation === "original"
      ? mediaOriginalContentPath(input.mediaAssetId)
      : mediaVariantContentPath(input.mediaAssetId, input.representation);

  return {
    src: resolveMediaAppProxySrc(path),
    alt: input.alt,
    width: input.width,
    height: input.height,
    aspectRatio: input.aspectRatio,
    sizes: input.sizes,
    priority: input.priority,
  };
}
