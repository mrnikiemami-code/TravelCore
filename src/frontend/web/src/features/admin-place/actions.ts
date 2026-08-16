"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type {
  PlaceDetailView,
  PlaceMediaLinkView,
  PlaceSummaryView,
  PlaceTranslationView,
} from "@/features/admin-place/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiAddress = {
  line1?: string | null;
  line2?: string | null;
  locality?: string | null;
  administrativeArea?: string | null;
  postalCode?: string | null;
  countryCode?: string | null;
};

type ApiPlace = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  catalogStatus: string;
  classificationCode?: string | null;
  facilities?: string[] | null;
  destinationId?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  address?: ApiAddress | null;
  hotel?: { starRating?: number | null } | null;
  restaurant?: { cuisineType?: string | null } | null;
  attraction?: { categoryCode?: string | null } | null;
  localizedName?: string | null;
  localizedDescription?: string | null;
  locale?: string | null;
  createdAt: string;
  updatedAt: string;
};

type ApiTranslation = {
  placeId: string;
  localeCode: string;
  name: string;
  description?: string | null;
  slug?: string | null;
};

type ApiMediaLink = {
  placeId: string;
  mediaAssetId: string;
  role: string;
  sortOrder: number;
};

type ApiSlugHit = {
  destinationId: string;
  localeCode: string;
  slug: string;
  kind: string;
  code: string;
  englishName: string;
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

function mapPlace(p: ApiPlace): PlaceSummaryView {
  return {
    id: p.id,
    kind: p.kind,
    code: p.code,
    englishName: p.englishName,
    catalogStatus: p.catalogStatus,
    classificationCode: p.classificationCode ?? null,
    facilities: p.facilities ?? [],
    destinationId: p.destinationId ?? null,
    latitude: p.latitude ?? null,
    longitude: p.longitude ?? null,
    address: p.address
      ? {
          line1: p.address.line1 ?? null,
          line2: p.address.line2 ?? null,
          locality: p.address.locality ?? null,
          administrativeArea: p.address.administrativeArea ?? null,
          postalCode: p.address.postalCode ?? null,
          countryCode: p.address.countryCode ?? null,
        }
      : null,
    hotelStarRating: p.hotel?.starRating ?? null,
    restaurantCuisineType: p.restaurant?.cuisineType ?? null,
    attractionCategoryCode: p.attraction?.categoryCode ?? null,
    localizedName: p.localizedName ?? null,
    localizedDescription: p.localizedDescription ?? null,
    locale: p.locale ?? null,
    createdAt: p.createdAt,
    updatedAt: p.updatedAt,
  };
}

function mapTranslation(t: ApiTranslation): PlaceTranslationView {
  return {
    placeId: t.placeId,
    localeCode: t.localeCode,
    name: t.name,
    description: t.description ?? null,
    slug: t.slug ?? null,
  };
}

function mapMediaLink(m: ApiMediaLink): PlaceMediaLinkView {
  return {
    placeId: m.placeId,
    mediaAssetId: m.mediaAssetId,
    role: m.role,
    sortOrder: m.sortOrder,
  };
}

function failMessage(
  result: { message: string; status?: number },
): { ok: false; message: string; status?: number } {
  return { ok: false, message: result.message, status: result.status };
}

export async function listPlacesAction(input: {
  kind?: string;
  take?: number;
}): Promise<
  | { ok: true; items: PlaceSummaryView[] }
  | { ok: false; message: string; status?: number }
> {
  const take = Math.min(Math.max(input.take ?? 50, 1), 200);
  const params = new URLSearchParams();
  params.set("take", String(take));
  if (input.kind && input.kind.trim()) {
    params.set("kind", input.kind.trim());
  }
  const result = await apiGetJson<ApiPlace[]>(
    `/api/place/places/?${params.toString()}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapPlace) };
}

export async function createPlaceAction(input: {
  kind: string;
  code: string;
  englishName: string;
  starRating?: number | null;
  cuisineType?: string | null;
  categoryCode?: string | null;
}): Promise<
  | { ok: true; place: PlaceSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const body: Record<string, unknown> = {
    kind: input.kind,
    code: input.code,
    englishName: input.englishName,
  };
  if (input.kind === "Hotel" && input.starRating != null) {
    body.starRating = input.starRating;
  }
  if (input.kind === "Restaurant" && input.cuisineType) {
    body.cuisineType = input.cuisineType;
  }
  if (input.kind === "Attraction" && input.categoryCode) {
    body.categoryCode = input.categoryCode;
  }

  const result = await apiSendJson<ApiPlace>("/api/place/places/", {
    method: "POST",
    headers: await authHeaders(),
    body,
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, place: mapPlace(result.data) };
}

export async function openPlaceByCodeAction(input: {
  code: string;
  locale?: string;
}): Promise<
  | { ok: true; detail: PlaceDetailView }
  | { ok: false; message: string; status?: number }
> {
  const code = encodeURIComponent(input.code.trim());
  const params = new URLSearchParams();
  if (input.locale) params.set("locale", input.locale);
  const qs = params.toString();
  const headers = await authHeaders();
  const placeResult = await apiGetJson<ApiPlace>(
    `/api/place/places/by-code/${code}${qs ? `?${qs}` : ""}`,
    { headers, cache: "no-store" },
  );
  if (!placeResult.ok) return failMessage(placeResult);
  return loadPlaceDetailBundle(placeResult.data.id, headers);
}

export async function loadPlaceDetailAction(
  placeId: string,
): Promise<
  | { ok: true; detail: PlaceDetailView }
  | { ok: false; message: string; status?: number }
> {
  return loadPlaceDetailBundle(placeId, await authHeaders());
}

async function loadPlaceDetailBundle(
  placeId: string,
  headers: HeadersInit,
): Promise<
  | { ok: true; detail: PlaceDetailView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(placeId.trim());
  const placeResult = await apiGetJson<ApiPlace>(`/api/place/places/${id}`, {
    headers,
    cache: "no-store",
  });
  if (!placeResult.ok) return failMessage(placeResult);

  const translationsResult = await apiGetJson<ApiTranslation[]>(
    `/api/place/places/${id}/translations`,
    { headers, cache: "no-store" },
  );
  if (!translationsResult.ok) return failMessage(translationsResult);

  const mediaResult = await apiGetJson<ApiMediaLink[]>(
    `/api/place/places/${id}/media`,
    { headers, cache: "no-store" },
  );
  if (!mediaResult.ok) return failMessage(mediaResult);

  return {
    ok: true,
    detail: {
      place: mapPlace(placeResult.data),
      translations: (translationsResult.data ?? []).map(mapTranslation),
      mediaLinks: (mediaResult.data ?? []).map(mapMediaLink),
    },
  };
}

export async function upsertPlaceTranslationAction(input: {
  placeId: string;
  localeCode: string;
  name: string;
  description?: string | null;
}): Promise<
  | { ok: true; translation: PlaceTranslationView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.placeId.trim());
  const locale = encodeURIComponent(input.localeCode.trim());
  const result = await apiSendJson<ApiTranslation>(
    `/api/place/places/${id}/translations/${locale}`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: {
        name: input.name,
        description: input.description ?? null,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, translation: mapTranslation(result.data) };
}

export async function setPlaceTranslationSlugAction(input: {
  placeId: string;
  localeCode: string;
  slug: string | null;
}): Promise<
  | { ok: true; translation: PlaceTranslationView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.placeId.trim());
  const locale = encodeURIComponent(input.localeCode.trim());
  const result = await apiSendJson<ApiTranslation>(
    `/api/place/places/${id}/translations/${locale}/slug`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { slug: input.slug },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, translation: mapTranslation(result.data) };
}

export async function publishPlaceSeoRouteAction(input: {
  placeId: string;
  localeCode: string;
  slug: string;
}): Promise<
  | { ok: true; publicPath: string }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<{ publicPath: string }>(
    "/api/seo/publication/place",
    {
      method: "POST",
      headers: await authHeaders(),
      body: {
        placeId: input.placeId,
        locale: input.localeCode,
        slug: input.slug,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, publicPath: result.data.publicPath };
}

export async function resolveDestinationBySlugAction(input: {
  localeCode: string;
  slug: string;
}): Promise<
  | {
      ok: true;
      destination: {
        id: string;
        code: string;
        englishName: string;
        kind: string;
        slug: string;
      };
    }
  | { ok: false; message: string; status?: number }
> {
  const locale = encodeURIComponent(input.localeCode.trim());
  const slug = encodeURIComponent(input.slug.trim());
  const result = await apiGetJson<ApiSlugHit>(
    `/api/destination/destinations/by-slug/${locale}/${slug}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return {
    ok: true,
    destination: {
      id: result.data.destinationId,
      code: result.data.code,
      englishName: result.data.englishName,
      kind: result.data.kind,
      slug: result.data.slug,
    },
  };
}

export async function setPlaceDestinationLinkAction(input: {
  placeId: string;
  destinationId: string | null;
}): Promise<
  | { ok: true; place: PlaceSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.placeId.trim());
  const result = await apiSendJson<ApiPlace>(
    `/api/place/places/${id}/destination-link`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { destinationId: input.destinationId },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, place: mapPlace(result.data) };
}

export async function setPlaceGeoAction(input: {
  placeId: string;
  latitude: number | null;
  longitude: number | null;
}): Promise<
  | { ok: true; place: PlaceSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.placeId.trim());
  const result = await apiSendJson<ApiPlace>(`/api/place/places/${id}/geo`, {
    method: "PUT",
    headers: await authHeaders(),
    body: { latitude: input.latitude, longitude: input.longitude },
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, place: mapPlace(result.data) };
}

export async function setPlaceAddressAction(input: {
  placeId: string;
  line1?: string | null;
  line2?: string | null;
  locality?: string | null;
  administrativeArea?: string | null;
  postalCode?: string | null;
  countryCode?: string | null;
}): Promise<
  | { ok: true; place: PlaceSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.placeId.trim());
  const result = await apiSendJson<ApiPlace>(`/api/place/places/${id}/address`, {
    method: "PUT",
    headers: await authHeaders(),
    body: {
      line1: input.line1 ?? null,
      line2: input.line2 ?? null,
      locality: input.locality ?? null,
      administrativeArea: input.administrativeArea ?? null,
      postalCode: input.postalCode ?? null,
      countryCode: input.countryCode ?? null,
    },
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, place: mapPlace(result.data) };
}

export async function setPlaceCatalogFieldsAction(input: {
  placeId: string;
  catalogStatus: string;
  classificationCode: string | null;
  facilityCodes: string[];
}): Promise<
  | { ok: true; place: PlaceSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.placeId.trim());
  const headers = await authHeaders();

  const statusResult = await apiSendJson<ApiPlace>(
    `/api/place/places/${id}/catalog-status`,
    {
      method: "PUT",
      headers,
      body: { catalogStatus: input.catalogStatus },
    },
  );
  if (!statusResult.ok) return failMessage(statusResult);

  const classResult = await apiSendJson<ApiPlace>(
    `/api/place/places/${id}/classification`,
    {
      method: "PUT",
      headers,
      body: { classificationCode: input.classificationCode },
    },
  );
  if (!classResult.ok) return failMessage(classResult);

  const facilitiesResult = await apiSendJson<ApiPlace>(
    `/api/place/places/${id}/facilities`,
    {
      method: "PUT",
      headers,
      body: { facilityCodes: input.facilityCodes },
    },
  );
  if (!facilitiesResult.ok) return failMessage(facilitiesResult);
  return { ok: true, place: mapPlace(facilitiesResult.data) };
}

export async function setPlaceCoverAction(input: {
  placeId: string;
  mediaAssetId: string;
}): Promise<
  | { ok: true; link: PlaceMediaLinkView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.placeId.trim());
  const result = await apiSendJson<ApiMediaLink>(
    `/api/place/places/${id}/media/cover`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { mediaAssetId: input.mediaAssetId },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, link: mapMediaLink(result.data) };
}

export async function removePlaceCoverAction(
  placeId: string,
): Promise<{ ok: true } | { ok: false; message: string; status?: number }> {
  const id = encodeURIComponent(placeId.trim());
  const result = await apiSendJson<unknown>(
    `/api/place/places/${id}/media/cover`,
    { method: "DELETE", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true };
}

export async function addPlaceGalleryItemAction(input: {
  placeId: string;
  mediaAssetId: string;
}): Promise<
  | { ok: true; link: PlaceMediaLinkView }
  | { ok: false; message: string; status?: number }
> {
  const id = encodeURIComponent(input.placeId.trim());
  const result = await apiSendJson<ApiMediaLink>(
    `/api/place/places/${id}/media/gallery`,
    {
      method: "POST",
      headers: await authHeaders(),
      body: { mediaAssetId: input.mediaAssetId },
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, link: mapMediaLink(result.data) };
}

export async function removePlaceGalleryItemAction(input: {
  placeId: string;
  mediaAssetId: string;
}): Promise<{ ok: true } | { ok: false; message: string; status?: number }> {
  const id = encodeURIComponent(input.placeId.trim());
  const mediaId = encodeURIComponent(input.mediaAssetId.trim());
  const result = await apiSendJson<unknown>(
    `/api/place/places/${id}/media/gallery/${mediaId}`,
    { method: "DELETE", headers: await authHeaders() },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true };
}
