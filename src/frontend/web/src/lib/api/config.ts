/**
 * Server-side API base URL — never hardcode hosts/ports.
 * Set via environment (e.g. TRAVELCORE_API_BASE_URL). Optional for fixture-only paths.
 */
export function getApiBaseUrl(): string | undefined {
  const raw =
    process.env.TRAVELCORE_API_BASE_URL ?? process.env.API_BASE_URL ?? undefined;
  if (!raw) return undefined;
  return raw.replace(/\/$/, "");
}

export const CORRELATION_HEADER = "X-Correlation-ID" as const;
