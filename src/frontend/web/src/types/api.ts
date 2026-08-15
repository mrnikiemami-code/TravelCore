/**
 * Frontend transport / problem contracts.
 * Aligned with P01 Problem Details (`application/problem+json`) — presentation only.
 */

export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  extensions?: Record<string, unknown>;
  [key: string]: unknown;
};

export type ApiSuccess<T> = {
  ok: true;
  data: T;
  status: number;
  correlationId?: string;
};

export type ApiFailure = {
  ok: false;
  status: number;
  message: string;
  problem?: ProblemDetails;
  correlationId?: string;
  /** Transport/parse failures (not HTTP business Problem Details). */
  kind: "http" | "network" | "parse" | "config";
};

export type ApiResult<T> = ApiSuccess<T> | ApiFailure;

/**
 * Marker for page-facing contracts.
 * Domain Model ≠ Persistence Model ≠ API Contract ≠ Page View Model
 */
export type PageViewModelBrand = { readonly __viewModel: "page" };

/**
 * Marker for workflow-oriented contracts that may compose multiple modules'
 * read models without merging backend ownership (enables T010 later).
 */
export type WorkflowViewModelBrand = { readonly __viewModel: "workflow" };
