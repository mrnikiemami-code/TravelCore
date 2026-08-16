"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type {
  CountryCatalogView,
  DestinationPathNodeView,
  DestinationPathView,
  DestinationSlugHitView,
  DestinationSummaryView,
  DestinationTranslationView,
} from "@/features/admin-destination-hierarchy/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiDestination = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  parentId?: string | null;
  isoCountryCode?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  localizedName?: string | null;
  localizedDescription?: string | null;
  locale?: string | null;
};

type ApiPathNode = {
  id: string;
  kind: string;
  code: string;
  englishName: string;
  parentId?: string | null;
  depthFromRoot: number;
};

type ApiPath = {
  destinationId: string;
  ancestorsRootFirst: ApiPathNode[];
  self: ApiPathNode;
  breadcrumbRootFirst: ApiPathNode[];
};

type ApiTranslation = {
  destinationId: string;
  localeCode: string;
  name: string;
  description?: string | null;
  slug?: string | null;
};

type ApiCountry = {
  alpha2Code: string;
  alpha3Code: string;
  numericCode?: string | null;
  englishName: string;
};

type ApiSlugHit = {
  destinationId: string;
  localeCode: string;
  slug: string;
  kind: string;
  code: string;
  englishName: string;
};

type ApiDescendants = {
  destinationId: string;
  maxDepth: number;
  nodes: ApiPathNode[];
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

function mapDestination(d: ApiDestination): DestinationSummaryView {
  return {
    id: d.id,
    kind: d.kind,
    code: d.code,
    englishName: d.englishName,
    parentId: d.parentId ?? null,
    isoCountryCode: d.isoCountryCode ?? null,
    latitude: d.latitude ?? null,
    longitude: d.longitude ?? null,
    localizedName: d.localizedName ?? null,
    localizedDescription: d.localizedDescription ?? null,
    locale: d.locale ?? null,
  };
}

function mapNode(n: ApiPathNode): DestinationPathNodeView {
  return {
    id: n.id,
    kind: n.kind,
    code: n.code,
    englishName: n.englishName,
    parentId: n.parentId ?? null,
    depthFromRoot: n.depthFromRoot,
  };
}

function mapPath(p: ApiPath): DestinationPathView {
  return {
    destinationId: p.destinationId,
    ancestorsRootFirst: (p.ancestorsRootFirst ?? []).map(mapNode),
    self: mapNode(p.self),
    breadcrumbRootFirst: (p.breadcrumbRootFirst ?? []).map(mapNode),
  };
}

function mapTranslation(t: ApiTranslation): DestinationTranslationView {
  return {
    destinationId: t.destinationId,
    localeCode: t.localeCode,
    name: t.name,
    description: t.description ?? null,
    slug: t.slug ?? null,
  };
}

function failMessage(
  result: { message: string; status?: number },
): { ok: false; message: string; status?: number } {
  return { ok: false, message: result.message, status: result.status };
}

export async function listCountriesAction(): Promise<
  | { ok: true; items: CountryCatalogView[] }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiCountry[]>("/api/reference-data/countries", {
    headers: await authHeaders(),
    cache: "no-store",
  });
  if (!result.ok) return failMessage(result);
  return {
    ok: true,
    items: (result.data ?? []).map((c) => ({
      alpha2Code: c.alpha2Code,
      alpha3Code: c.alpha3Code,
      numericCode: c.numericCode ?? null,
      englishName: c.englishName,
    })),
  };
}

export async function openBySlugAction(input: {
  localeCode: string;
  slug: string;
}): Promise<
  | { ok: true; hit: DestinationSlugHitView; destination: DestinationSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const locale = encodeURIComponent(input.localeCode.trim());
  const slug = encodeURIComponent(input.slug.trim());
  const hitResult = await apiGetJson<ApiSlugHit>(
    `/api/destination/destinations/by-slug/${locale}/${slug}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!hitResult.ok) return failMessage(hitResult);

  const destResult = await apiGetJson<ApiDestination>(
    `/api/destination/destinations/${hitResult.data.destinationId}?locale=${locale}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!destResult.ok) return failMessage(destResult);

  return {
    ok: true,
    hit: {
      destinationId: hitResult.data.destinationId,
      localeCode: hitResult.data.localeCode,
      slug: hitResult.data.slug,
      kind: hitResult.data.kind,
      code: hitResult.data.code,
      englishName: hitResult.data.englishName,
    },
    destination: mapDestination(destResult.data),
  };
}

export async function loadDestinationBundleAction(
  destinationId: string,
  locale?: string,
): Promise<
  | {
      ok: true;
      destination: DestinationSummaryView;
      path: DestinationPathView;
      children: DestinationSummaryView[];
      descendants: DestinationPathNodeView[];
      translations: DestinationTranslationView[];
    }
  | { ok: false; message: string; status?: number }
> {
  const headers = await authHeaders();
  const localeQuery = locale ? `?locale=${encodeURIComponent(locale)}` : "";

  const [destResult, pathResult, childrenResult, descendantsResult, translationsResult] =
    await Promise.all([
      apiGetJson<ApiDestination>(
        `/api/destination/destinations/${destinationId}${localeQuery}`,
        { headers, cache: "no-store" },
      ),
      apiGetJson<ApiPath>(`/api/destination/destinations/${destinationId}/path`, {
        headers,
        cache: "no-store",
      }),
      apiGetJson<ApiDestination[]>(
        `/api/destination/destinations/${destinationId}/children`,
        { headers, cache: "no-store" },
      ),
      apiGetJson<ApiDescendants>(
        `/api/destination/destinations/${destinationId}/descendants?depth=1`,
        { headers, cache: "no-store" },
      ),
      apiGetJson<ApiTranslation[]>(
        `/api/destination/destinations/${destinationId}/translations`,
        { headers, cache: "no-store" },
      ),
    ]);

  if (!destResult.ok) return failMessage(destResult);
  if (!pathResult.ok) return failMessage(pathResult);
  if (!childrenResult.ok) return failMessage(childrenResult);
  if (!descendantsResult.ok) return failMessage(descendantsResult);
  if (!translationsResult.ok) return failMessage(translationsResult);

  return {
    ok: true,
    destination: mapDestination(destResult.data),
    path: mapPath(pathResult.data),
    children: (childrenResult.data ?? []).map(mapDestination),
    descendants: (descendantsResult.data.nodes ?? []).map(mapNode),
    translations: (translationsResult.data ?? []).map(mapTranslation),
  };
}

export async function createDestinationAction(input: {
  kind: string;
  code: string;
  englishName: string;
  parentId?: string | null;
  isoCountryCode?: string | null;
}): Promise<
  | { ok: true; destination: DestinationSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiDestination>("/api/destination/destinations/", {
    method: "POST",
    body: {
      kind: input.kind,
      code: input.code,
      englishName: input.englishName,
      parentId: input.parentId ?? null,
      isoCountryCode: input.isoCountryCode ?? null,
    },
    headers: await authHeaders(),
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, destination: mapDestination(result.data) };
}

export async function upsertTranslationAction(input: {
  destinationId: string;
  localeCode: string;
  name: string;
  description?: string | null;
  slug?: string | null;
}): Promise<
  | { ok: true; translation: DestinationTranslationView }
  | { ok: false; message: string; status?: number }
> {
  const locale = encodeURIComponent(input.localeCode.trim());
  const result = await apiSendJson<ApiTranslation>(
    `/api/destination/destinations/${input.destinationId}/translations/${locale}`,
    {
      method: "PUT",
      body: {
        name: input.name,
        description: input.description ?? null,
        slug: input.slug ?? null,
      },
      headers: await authHeaders(),
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, translation: mapTranslation(result.data) };
}

export async function setTranslationSlugAction(input: {
  destinationId: string;
  localeCode: string;
  slug: string | null;
}): Promise<
  | { ok: true; translation: DestinationTranslationView }
  | { ok: false; message: string; status?: number }
> {
  const locale = encodeURIComponent(input.localeCode.trim());
  const result = await apiSendJson<ApiTranslation>(
    `/api/destination/destinations/${input.destinationId}/translations/${locale}/slug`,
    {
      method: "PUT",
      body: { slug: input.slug },
      headers: await authHeaders(),
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, translation: mapTranslation(result.data) };
}

export async function setGeoAction(input: {
  destinationId: string;
  latitude: number | null;
  longitude: number | null;
}): Promise<
  | { ok: true; destination: DestinationSummaryView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiDestination>(
    `/api/destination/destinations/${input.destinationId}/geo`,
    {
      method: "PUT",
      body: {
        latitude: input.latitude,
        longitude: input.longitude,
      },
      headers: await authHeaders(),
    },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, destination: mapDestination(result.data) };
}
