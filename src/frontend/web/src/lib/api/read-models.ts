import type { PageViewModelBrand, WorkflowViewModelBrand } from "@/types/api";

/**
 * Conventions for page / workflow read models.
 *
 * - PageViewModel: purpose-specific presentation contract for one screen/section
 * - WorkflowViewModel: may compose multiple module-originated read models
 *   without merging backend ownership (Identity ≠ Party ≠ Access, etc.)
 *
 * Never adopt EF entities / DbContext shapes as these contracts.
 */

export type PageViewModel<TFields extends object> = TFields & PageViewModelBrand;

export type WorkflowViewModel<TFields extends object> = TFields &
  WorkflowViewModelBrand;

export function asPageViewModel<TFields extends object>(
  fields: TFields,
): PageViewModel<TFields> {
  return { ...fields, __viewModel: "page" };
}

export function asWorkflowViewModel<TFields extends object>(
  fields: TFields,
): WorkflowViewModel<TFields> {
  return { ...fields, __viewModel: "workflow" };
}
