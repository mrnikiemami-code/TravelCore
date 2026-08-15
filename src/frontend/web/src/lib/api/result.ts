import type { ApiFailure, ApiResult, ApiSuccess, ProblemDetails } from "@/types/api";

export function apiOk<T>(
  data: T,
  status = 200,
  correlationId?: string,
): ApiSuccess<T> {
  return { ok: true, data, status, correlationId };
}

export function apiFail(
  input: Omit<ApiFailure, "ok">,
): ApiFailure {
  return { ok: false, ...input };
}

export function isApiOk<T>(result: ApiResult<T>): result is ApiSuccess<T> {
  return result.ok;
}

export async function tryParseProblem(
  response: Response,
): Promise<ProblemDetails | undefined> {
  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("json")) return undefined;
  try {
    const body = (await response.json()) as ProblemDetails;
    if (body && typeof body === "object") return body;
  } catch {
    return undefined;
  }
  return undefined;
}
