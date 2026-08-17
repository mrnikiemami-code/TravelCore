"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type {
  TourMediaView,
  TourProductDetailView,
  TourProductSummaryView,
  TourTranslationView,
} from "@/features/admin-tour/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiTourProduct = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
  classificationCode?: string | null;
  originDestinationId?: string | null;
  agencyId?: string | null;
  destinationIds?: string[] | null;
  localizedTitle?: string | null;
  localizedDescription?: string | null;
  localizedSlug?: string | null;
  createdAt: string;
  updatedAt: string;
};

type ApiTranslation = {
  tourProductId: string;
  localeCode: string;
  title: string;
  description?: string | null;
  slug?: string | null;
  updatedAt: string;
};

type ApiMedia = {
  id: string;
  code: string;
  cover?: { mediaAssetId: string; role: string; sortOrder: number } | null;
  gallery?: { mediaAssetId: string; role: string; sortOrder: number }[] | null;
};

async function authHeaders(): Promise<HeadersInit> {
  const jar = await cookies();
  const ticket = jar.get(AUTH_COOKIE)?.value;
  const headers = new Headers();
  if (ticket) {
    headers.set("cookie", `${AUTH_COOKIE}=${ticket}`);
  }
  return headers;
}

function failMessage(result: {
  ok: false;
  status?: number;
  message?: string;
}): { ok: false; message: string; status?: number } {
  return {
    ok: false,
    message: result.message ?? "Request failed.",
    status: result.status,
  };
}

function mapSummary(p: ApiTourProduct): TourProductSummaryView {
  return {
    id: p.id,
    kind: p.kind,
    code: p.code,
    englishName: p.englishName,
    catalogStatus: p.catalogStatus,
    classificationCode: p.classificationCode ?? null,
    localizedTitle: p.localizedTitle ?? null,
    localizedSlug: p.localizedSlug ?? null,
    createdAt: p.createdAt,
    updatedAt: p.updatedAt,
  };
}

function mapDetail(p: ApiTourProduct): TourProductDetailView {
  return {
    ...mapSummary(p),
    originDestinationId: p.originDestinationId ?? null,
    agencyId: p.agencyId ?? null,
    destinationIds: p.destinationIds ?? [],
    localizedDescription: p.localizedDescription ?? null,
  };
}

function mapMedia(m: ApiMedia): TourMediaView {
  return {
    id: m.id,
    code: m.code,
    coverMediaAssetId: m.cover?.mediaAssetId ?? null,
    galleryMediaAssetIds: (m.gallery ?? []).map((g) => g.mediaAssetId),
  };
}

export async function listTourProductsAction(input?: {
  kind?: string;
  take?: number;
}): Promise<
  | { ok: true; items: TourProductSummaryView[] }
  | { ok: false; message: string; status?: number }
> {
  const params = new URLSearchParams();
  if (input?.kind) params.set("kind", input.kind);
  if (input?.take) params.set("take", String(input.take));
  const qs = params.toString();
  const result = await apiGetJson<ApiTourProduct[]>(
    `/api/tour/products${qs ? `?${qs}` : ""}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapSummary) };
}

export async function createTourProductAction(input: {
  kind: "Experience" | "Package";
  code: string;
  englishName: string;
}): Promise<
  | { ok: true; item: TourProductSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTourProduct>("/api/tour/products/", {
    method: "POST",
    headers: await authHeaders(),
    body: input,
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapSummary(result.data) };
}

export async function openTourByCodeAction(input: {
  code: string;
  locale?: string;
}): Promise<
  | { ok: true; item: TourProductDetailView }
  | { ok: false; message: string; status?: number }
> {
  const localeQs = input.locale
    ? `?locale=${encodeURIComponent(input.locale)}`
    : "";
  const result = await apiGetJson<ApiTourProduct>(
    `/api/tour/products/by-code/${encodeURIComponent(input.code)}${localeQs}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapDetail(result.data) };
}

export async function loadTourDetailAction(input: {
  id: string;
  locale?: string;
}): Promise<
  | { ok: true; item: TourProductDetailView }
  | { ok: false; message: string; status?: number }
> {
  const localeQs = input.locale
    ? `?locale=${encodeURIComponent(input.locale)}`
    : "";
  const result = await apiGetJson<ApiTourProduct>(
    `/api/tour/products/${encodeURIComponent(input.id)}${localeQs}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapDetail(result.data) };
}

export async function upsertTourTranslationAction(input: {
  id: string;
  localeCode: string;
  title: string;
  description?: string | null;
}): Promise<
  | { ok: true; translation: TourTranslationView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTranslation>(
    `/api/tour/products/${encodeURIComponent(input.id)}/translations/${encodeURIComponent(input.localeCode)}`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { title: input.title, description: input.description ?? null },
    },
  );
  if (!result.ok) return failMessage(result);
  return {
    ok: true,
    translation: {
      tourProductId: result.data.tourProductId,
      localeCode: result.data.localeCode,
      title: result.data.title,
      description: result.data.description ?? null,
      slug: result.data.slug ?? null,
      updatedAt: result.data.updatedAt,
    },
  };
}

export async function setTourTranslationSlugAction(input: {
  id: string;
  localeCode: string;
  slug: string | null;
}): Promise<
  | { ok: true; item: TourProductDetailView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTourProduct>(
    `/api/tour/products/${encodeURIComponent(input.id)}/translations/${encodeURIComponent(input.localeCode)}/slug`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { slug: input.slug },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapDetail(result.data) };
}

export async function setTourCatalogStatusAction(input: {
  id: string;
  catalogStatus: "Draft" | "Published" | "Inactive";
}): Promise<
  | { ok: true; item: TourProductDetailView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTourProduct>(
    `/api/tour/products/${encodeURIComponent(input.id)}/catalog-status`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { catalogStatus: input.catalogStatus },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, item: mapDetail(result.data) };
}

export async function setTourClassificationAction(input: {
  id: string;
  classificationCode: string | null;
}): Promise<
  | { ok: true }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson(
    `/api/tour/products/${encodeURIComponent(input.id)}/classification`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { classificationCode: input.classificationCode },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true };
}

export async function loadTourMediaAction(input: {
  id: string;
}): Promise<
  | { ok: true; media: TourMediaView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiMedia>(
    `/api/tour/products/${encodeURIComponent(input.id)}/media`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, media: mapMedia(result.data) };
}

export async function setTourCoverAction(input: {
  id: string;
  mediaAssetId: string;
}): Promise<
  | { ok: true; media: TourMediaView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiMedia>(
    `/api/tour/products/${encodeURIComponent(input.id)}/media/cover`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { mediaAssetId: input.mediaAssetId },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, media: mapMedia(result.data) };
}

export async function removeTourCoverAction(input: {
  id: string;
}): Promise<
  | { ok: true; media: TourMediaView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiMedia>(
    `/api/tour/products/${encodeURIComponent(input.id)}/media/cover`,
    {
      method: "DELETE",
      headers: await authHeaders(),
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, media: mapMedia(result.data) };
}

export async function addTourGalleryItemAction(input: {
  id: string;
  mediaAssetId: string;
}): Promise<
  | { ok: true; media: TourMediaView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiMedia>(
    `/api/tour/products/${encodeURIComponent(input.id)}/media/gallery`,
    {
      method: "POST",
      headers: await authHeaders(),
      body: { mediaAssetId: input.mediaAssetId },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, media: mapMedia(result.data) };
}

export async function publishTourSeoRouteAction(input: {
  tourProductId: string;
  localeCode: string;
  slug: string;
}): Promise<
  | { ok: true; publicPath: string }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<{ publicPath: string }>(
    "/api/seo/publication/tour-product",
    {
      method: "POST",
      headers: await authHeaders(),
      body: {
        tourProductId: input.tourProductId,
        locale: input.localeCode,
        slug: input.slug,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, publicPath: result.data.publicPath };
}
