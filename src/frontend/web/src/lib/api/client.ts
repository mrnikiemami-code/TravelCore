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

export type ServerMutateOptions = ServerFetchOptions & {
  method?: "POST" | "PUT" | "DELETE";
  body?: unknown;
};

function joinUrl(base: string, path: string): string {
  if (path.startsWith("http://") || path.startsWith("https://")) return path;
  const normalized = path.startsWith("/") ? path : `/${path}`;
  return `${base}${normalized}`;
}

async function executeJsonRequest<T>(
  path: string,
  method: string,
  options: ServerMutateOptions,
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
  if (options.body !== undefined && !headers.has("content-type")) {
    headers.set("content-type", "application/json");
  }

  let response: Response;
  try {
    response = await fetch(joinUrl(base, path), {
      method,
      headers,
      body:
        options.body === undefined ? undefined : JSON.stringify(options.body),
      cache: options.cache ?? "no-store",
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

  if (response.status === 204) {
    return apiOk(undefined as T, response.status, correlationId ?? undefined);
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

/**
 * Server-side JSON GET — primary API consumption path for Server Components.
 * Does not encode domain/pricing/booking authority.
 */
export async function apiGetJson<T>(
  path: string,
  options: ServerFetchOptions = {},
): Promise<ApiResult<T>> {
  return executeJsonRequest<T>(path, "GET", options);
}

/**
 * Server-side JSON mutate helper (POST/PUT/DELETE) for Admin workflow writes.
 * Callers must forward auth cookies when required; Access remains server-side.
 */
export async function apiSendJson<T>(
  path: string,
  options: ServerMutateOptions = {},
): Promise<ApiResult<T>> {
  return executeJsonRequest<T>(path, options.method ?? "POST", options);
}

export type ServerFormOptions = ServerFetchOptions & {
  method?: "POST" | "PUT";
  /** multipart/form-data body — do not set content-type (boundary is browser/runtime owned). */
  formData: FormData;
};

/**
 * Server-side multipart mutate helper (e.g. Media upload).
 * Callers must forward auth cookies when required; Access remains server-side.
 */
export async function apiSendFormData<T>(
  path: string,
  options: ServerFormOptions,
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
  // Let fetch set multipart boundary — never force application/json here.

  let response: Response;
  try {
    response = await fetch(joinUrl(base, path), {
      method: options.method ?? "POST",
      headers,
      body: options.formData,
      cache: options.cache ?? "no-store",
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

  if (response.status === 204) {
    return apiOk(undefined as T, response.status, correlationId ?? undefined);
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
