import { apiOk } from "@/lib/api/result";
import { asPageViewModel } from "@/lib/api/read-models";
import type { ApiResult } from "@/types/api";
import type { PageViewModel } from "@/lib/api/read-models";

/**
 * Minimal fixture read model — proves the boundary without Foreign Tour Detail
 * or product APIs (those belong to later tasks).
 */
export type BoundarySmokeReadModel = PageViewModel<{
  label: string;
  source: "fixture";
}>;

export async function loadBoundarySmokeFixture(): Promise<
  ApiResult<BoundarySmokeReadModel>
> {
  return apiOk(
    asPageViewModel({
      label: "api-boundary-ok",
      source: "fixture" as const,
    }),
  );
}
