import { apiSendJson } from "@/lib/api/client";
import { isApiOk } from "@/lib/api/result";
import type {
  SeoBreadcrumbListJsonLd,
  SeoBreadcrumbNodeInput,
} from "@/lib/seo/structured-data-contract";

/**
 * Server-only: compose truthful BreadcrumbList JSON-LD via SEO projection API.
 */
export async function loadSeoBreadcrumbJsonLd(
  locale: string,
  nodes: SeoBreadcrumbNodeInput[],
): Promise<SeoBreadcrumbListJsonLd | null> {
  const result = await apiSendJson<SeoBreadcrumbListJsonLd>(
    "/api/seo/structured-data/breadcrumb",
    {
      method: "POST",
      body: { locale, nodes },
      cache: "no-store",
    },
  );

  if (!isApiOk(result) || result.status === 204) {
    return null;
  }

  return result.data;
}
