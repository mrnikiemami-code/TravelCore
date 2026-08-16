"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type {
  SeoDestinationPostureView,
  SeoIndexabilityView,
  SeoIndexPolicyView,
  SeoRouteView,
} from "@/features/admin-destination-seo/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiRoute = {
  id: string;
  resourceType: string;
  resourceId: string;
  locale: string;
  path: string;
};

type ApiPolicy = {
  id: string;
  resourceType: string;
  resourceId: string;
  locale: string;
  indexDirective: string;
  followDirective: string;
  updatedAt: string;
};

type ApiIndexability = {
  locale: string;
  path: string;
  effectiveIndex: string;
  effectiveFollow: string;
  robotsDirective: string;
  configuredIndex?: string | null;
  configuredFollow?: string | null;
  isIndexable: boolean;
  reasons?: string[];
};

type ApiPosture = {
  destinationId: string;
  locale: string;
  routes: ApiRoute[];
  configuredPolicy?: ApiPolicy | null;
  effectiveIndexability?: ApiIndexability | null;
  notes: string;
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

function mapRoute(r: ApiRoute): SeoRouteView {
  return {
    id: r.id,
    resourceType: r.resourceType,
    resourceId: r.resourceId,
    locale: r.locale,
    path: r.path,
  };
}

function mapPolicy(p: ApiPolicy): SeoIndexPolicyView {
  return {
    id: p.id,
    resourceType: p.resourceType,
    resourceId: p.resourceId,
    locale: p.locale,
    indexDirective: p.indexDirective,
    followDirective: p.followDirective,
    updatedAt: p.updatedAt,
  };
}

function mapIndexability(i: ApiIndexability): SeoIndexabilityView {
  return {
    locale: i.locale,
    path: i.path,
    effectiveIndex: i.effectiveIndex,
    effectiveFollow: i.effectiveFollow,
    robotsDirective: i.robotsDirective,
    configuredIndex: i.configuredIndex ?? null,
    configuredFollow: i.configuredFollow ?? null,
    isIndexable: i.isIndexable,
    reasons: i.reasons ?? [],
  };
}

function mapPosture(p: ApiPosture): SeoDestinationPostureView {
  return {
    destinationId: p.destinationId,
    locale: p.locale,
    routes: (p.routes ?? []).map(mapRoute),
    configuredPolicy: p.configuredPolicy ? mapPolicy(p.configuredPolicy) : null,
    effectiveIndexability: p.effectiveIndexability
      ? mapIndexability(p.effectiveIndexability)
      : null,
    notes: p.notes,
  };
}

function failMessage(
  result: { message: string; status?: number },
): { ok: false; message: string; status?: number } {
  return { ok: false, message: result.message, status: result.status };
}

export async function loadDestinationSeoPostureAction(input: {
  destinationId: string;
  locale: string;
}): Promise<
  | { ok: true; posture: SeoDestinationPostureView }
  | { ok: false; message: string; status?: number }
> {
  const locale = encodeURIComponent(input.locale.trim());
  const result = await apiGetJson<ApiPosture>(
    `/api/seo/admin/destination-posture/${input.destinationId}/${locale}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, posture: mapPosture(result.data) };
}

export async function setDestinationIndexPolicyAction(input: {
  destinationId: string;
  locale: string;
  indexDirective: string;
  followDirective: string;
}): Promise<
  | { ok: true; policy: SeoIndexPolicyView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiPolicy>("/api/seo/index-policies", {
    method: "PUT",
    body: {
      resourceType: "Destination",
      resourceId: input.destinationId,
      locale: input.locale.trim(),
      indexDirective: input.indexDirective,
      followDirective: input.followDirective,
    },
    headers: await authHeaders(),
  });
  if (!result.ok) return failMessage(result);
  return { ok: true, policy: mapPolicy(result.data) };
}

export async function publishDestinationSeoRouteAction(input: {
  destinationId: string;
  locale: string;
  slug: string;
}): Promise<{ ok: true } | { ok: false; message: string; status?: number }> {
  const result = await apiSendJson<unknown>("/api/seo/publication/destination", {
    method: "POST",
    body: {
      destinationId: input.destinationId,
      locale: input.locale.trim(),
      slug: input.slug.trim(),
    },
    headers: await authHeaders(),
  });
  if (!result.ok) return failMessage(result);
  return { ok: true };
}
