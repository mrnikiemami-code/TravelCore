"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendFormData, apiSendJson } from "@/lib/api/client";
import type {
  MediaAssetDetailView,
  MediaAssetSummaryView,
  MediaTranslationView,
  MediaVariantView,
} from "@/features/admin-media/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiAsset = {
  id: string;
  contentType: string;
  byteSize: number;
  width?: number | null;
  height?: number | null;
  focalX?: number | null;
  focalY?: number | null;
  status: string;
  createdAt: string;
  updatedAt: string;
};

type ApiVariant = {
  id: string;
  mediaAssetId: string;
  profile: string;
  status: string;
  width?: number | null;
  height?: number | null;
  byteSize?: number | null;
  contentType?: string | null;
  failureReason?: string | null;
};

type ApiTranslation = {
  mediaAssetId: string;
  localeCode: string;
  altText: string;
  caption?: string | null;
  publicationStatus: string;
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

function mapAsset(a: ApiAsset): MediaAssetSummaryView {
  return {
    id: a.id,
    contentType: a.contentType,
    byteSize: a.byteSize,
    width: a.width ?? null,
    height: a.height ?? null,
    focalX: a.focalX ?? null,
    focalY: a.focalY ?? null,
    status: a.status,
    createdAt: a.createdAt,
    updatedAt: a.updatedAt,
  };
}

function mapVariant(v: ApiVariant): MediaVariantView {
  return {
    id: v.id,
    mediaAssetId: v.mediaAssetId,
    profile: v.profile,
    status: v.status,
    width: v.width ?? null,
    height: v.height ?? null,
    byteSize: v.byteSize ?? null,
    contentType: v.contentType ?? null,
    failureReason: v.failureReason ?? null,
  };
}

function mapTranslation(t: ApiTranslation): MediaTranslationView {
  return {
    mediaAssetId: t.mediaAssetId,
    localeCode: t.localeCode,
    altText: t.altText,
    caption: t.caption ?? null,
    publicationStatus: t.publicationStatus,
  };
}

function failMessage(
  result: { message: string; status?: number },
): { ok: false; message: string; status?: number } {
  return { ok: false, message: result.message, status: result.status };
}

export async function listMediaAssetsAction(input: {
  status?: string;
  take?: number;
}): Promise<
  | { ok: true; items: MediaAssetSummaryView[] }
  | { ok: false; message: string; status?: number }
> {
  const take = Math.min(Math.max(input.take ?? 50, 1), 200);
  const params = new URLSearchParams();
  params.set("take", String(take));
  if (input.status && input.status.trim()) {
    params.set("status", input.status.trim());
  }
  const result = await apiGetJson<ApiAsset[]>(
    `/api/media/assets/?${params.toString()}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapAsset) };
}

export async function uploadMediaAssetAction(formData: FormData): Promise<
  | { ok: true; asset: MediaAssetSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const file = formData.get("file");
  if (!(file instanceof File) || file.size <= 0) {
    return { ok: false, message: "A non-empty file is required.", status: 400 };
  }

  const outbound = new FormData();
  outbound.set("file", file, file.name);

  const result = await apiSendFormData<ApiAsset>("/api/media/assets/upload", {
    formData: outbound,
    headers: await authHeaders(),
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, asset: mapAsset(result.data) };
}

export async function loadMediaAssetDetailAction(
  mediaAssetId: string,
): Promise<
  | { ok: true; detail: MediaAssetDetailView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(mediaAssetId.trim());
  const headers = await authHeaders();

  const assetResult = await apiGetJson<ApiAsset>(`/api/media/assets/${id}`, {
    headers,
    cache: "no-store",
  });
  if (!assetResult.ok) return failMessage(assetResult);

  const variantsResult = await apiGetJson<ApiVariant[]>(
    `/api/media/assets/${id}/variants`,
    { headers, cache: "no-store" },
  );
  if (!variantsResult.ok) return failMessage(variantsResult);

  const translationsResult = await apiGetJson<ApiTranslation[]>(
    `/api/media/assets/${id}/translations`,
    { headers, cache: "no-store" },
  );
  if (!translationsResult.ok) return failMessage(translationsResult);

  return {
    ok: true,
    detail: {
      asset: mapAsset(assetResult.data),
      variants: (variantsResult.data ?? []).map(mapVariant),
      translations: (translationsResult.data ?? []).map(mapTranslation),
    },
  };
}

export async function generateMediaVariantsAction(
  mediaAssetId: string,
): Promise<
  | { ok: true; variants: MediaVariantView[] }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(mediaAssetId.trim());
  const result = await apiSendJson<ApiVariant[]>(
    `/api/media/assets/${id}/variants/generate`,
    { method: "POST", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, variants: (result.data ?? []).map(mapVariant) };
}

export async function upsertMediaTranslationAction(input: {
  mediaAssetId: string;
  localeCode: string;
  altText: string;
  caption?: string | null;
  publicationStatus?: string;
}): Promise<
  | { ok: true; translation: MediaTranslationView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.mediaAssetId.trim());
  const locale = encodeURIComponent(input.localeCode.trim());
  const result = await apiSendJson<ApiTranslation>(
    `/api/media/assets/${id}/translations/${locale}`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: {
        altText: input.altText,
        caption: input.caption ?? null,
        publicationStatus: input.publicationStatus ?? undefined,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, translation: mapTranslation(result.data) };
}

export async function setMediaFocalPointAction(input: {
  mediaAssetId: string;
  focalX: number | null;
  focalY: number | null;
}): Promise<
  | { ok: true; asset: MediaAssetSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.mediaAssetId.trim());
  const headers = await authHeaders();
  const focalResult = await apiSendJson<{
    mediaAssetId: string;
    focalX?: number | null;
    focalY?: number | null;
  }>(`/api/media/assets/${id}/focal-point`, {
    method: "PUT",
    headers,
    body: { focalX: input.focalX, focalY: input.focalY },
  });
  if (!focalResult.ok) return failMessage(focalResult);

  const assetResult = await apiGetJson<ApiAsset>(`/api/media/assets/${id}`, {
    headers,
    cache: "no-store",
  });
  if (!assetResult.ok) return failMessage(assetResult);
  return { ok: true, asset: mapAsset(assetResult.data) };
}
