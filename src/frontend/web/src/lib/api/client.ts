import { CORRELATION_HEADER, getApiBaseUrl } from "@/lib/api/config";
import { apiFail, apiOk, tryParseProblem } from "@/lib/api/result";
import type { ApiResult } from "@/types/api";

export type ServerFetchOptions = {
  /** Explicit Next.js fetch cache control — caller decides; no hidden global cache. */
  cache?: RequestCache;
  next?: { revalidate?: number | false; tags?: string[] };
  correlationId?: string;
  headers?: HeadersInit;
  signal?: AbortSignal;
};

function joinUrl(base: string, path: string): string {
  if (path.startsWith("http://") || path.startsWith("https://")) return path;
  const normalized = path.startsWith("/") ? path : `/${path}`;
  return `${base}${normalized}`;
}

/**
 * Server-side JSON GET — primary API consumption path for Server Components.
 * Does not encode domain/pricing/booking authority.
 */
export async function apiGetJson<T>(
  path: string,
  options: ServerFetchOptions = {},
): Promise<ApiResult<T>> {
  const base = getApiBaseUrl();
  if (!base) {
    return apiFail({
      kind: "config",
      status: 0,
      message:
        "API base URL is not configured (set TRAVELCORE_API_BASE_URL or API_BASE_URL).",
      correlationId: options.correlationId,
    });
  }

  const headers = new Headers(options.headers);
  if (options.correlationId) {
    headers.set(CORRELATION_HEADER, options.correlationId);
  }
  if (!headers.has("accept")) {
    headers.set("accept", "application/json, application/problem+json");
  }

  let response: Response;
  try {
    response = await fetch(joinUrl(base, path), {
      method: "GET",
      headers,
      cache: options.cache,
      next: options.next,
      signal: options.signal,
    });
  } catch {
    return apiFail({
      kind: "network",
      status: 0,
      message: "Network request failed.",
      correlationId: options.correlationId,
    });
  }

  const correlationId =
    response.headers.get(CORRELATION_HEADER) ?? options.correlationId;

  if (!response.ok) {
    const problem = await tryParseProblem(response);
    return apiFail({
      kind: "http",
      status: response.status,
      message:
        problem?.title ??
        problem?.detail ??
        `Request failed with status ${response.status}.`,
      problem,
      correlationId,
    });
  }

  try {
    const data = (await response.json()) as T;
    return apiOk(data, response.status, correlationId ?? undefined);
  } catch {
    return apiFail({
      kind: "parse",
      status: response.status,
      message: "Response was not valid JSON.",
      correlationId,
    });
  }
}
