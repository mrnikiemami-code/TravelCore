"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type { AgencyOfferModerationQueueView } from "@/features/admin-agency-offer-governance/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiAgencyOfferModerationQueueItem = {
  offerId: string;
  agencyProfileId: string;
  tourProductId: string;
  titleOverride: string | null;
  highlight: string | null;
  salesChannel: string;
  status: string;
  visibility: string;
  publicationStatus: string;
  createdAt: string;
  updatedAt: string;
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

function mapItem(item: ApiAgencyOfferModerationQueueItem): AgencyOfferModerationQueueView {
  return {
    offerId: item.offerId,
    agencyProfileId: item.agencyProfileId,
    tourProductId: item.tourProductId,
    titleOverride: item.titleOverride,
    highlight: item.highlight,
    salesChannel: item.salesChannel,
    status: item.status,
    visibility: item.visibility,
    publicationStatus: item.publicationStatus,
    createdAt: item.createdAt,
    updatedAt: item.updatedAt,
  };
}

export async function listPendingAgencyOffersAction(input?: {
  take?: number;
}): Promise<
  | { ok: true; items: AgencyOfferModerationQueueView[] }
  | { ok: false; message: string; status?: number }
> {
  const params = new URLSearchParams();
  if (input?.take) params.set("take", String(input.take));
  const qs = params.toString();
  const result = await apiGetJson<ApiAgencyOfferModerationQueueItem[]>(
    `/api/agency-marketplace/moderation/offers/pending${qs ? `?${qs}` : ""}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapItem) };
}

async function mutateOfferAction(
  offerId: string,
  action: "approve" | "reject" | "suspend",
): Promise<
  | { ok: true; item: AgencyOfferModerationQueueView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiAgencyOfferModerationQueueItem>(
    `/api/agency-marketplace/moderation/offers/${encodeURIComponent(offerId)}/${action}`,
    {
      method: "POST",
      headers: await authHeaders(),
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty response." };
  return { ok: true, item: mapItem(result.data) };
}

export async function approveAgencyOfferAction(offerId: string) {
  return mutateOfferAction(offerId, "approve");
}

export async function rejectAgencyOfferAction(offerId: string) {
  return mutateOfferAction(offerId, "reject");
}

export async function suspendAgencyOfferAction(offerId: string) {
  return mutateOfferAction(offerId, "suspend");
}

type ApiPolicyDecision = {
  kind: string;
  code: string;
  reason: string;
  policyName: string;
};

type ApiPolicyEvaluationReport = {
  offerId: string;
  agencyProfileId: string;
  tourProductId: string;
  publicationStatus: string;
  salesChannel: string;
  aggregate: ApiPolicyDecision;
  hookDecisions: ApiPolicyDecision[];
};

export type AgencyOfferPolicyEvaluationView = {
  offerId: string;
  publicationStatus: string;
  salesChannel: string;
  aggregateKind: string;
  aggregateCode: string;
  aggregateReason: string;
  hooks: Array<{
    policyName: string;
    kind: string;
    code: string;
    reason: string;
  }>;
};

export async function evaluateAgencyOfferPoliciesAction(offerId: string): Promise<
  | { ok: true; report: AgencyOfferPolicyEvaluationView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiPolicyEvaluationReport>(
    `/api/agency-marketplace/moderation/offers/${encodeURIComponent(offerId)}/policy-evaluation`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty response." };
  const data = result.data;
  return {
    ok: true,
    report: {
      offerId: data.offerId,
      publicationStatus: data.publicationStatus,
      salesChannel: data.salesChannel,
      aggregateKind: data.aggregate.kind,
      aggregateCode: data.aggregate.code,
      aggregateReason: data.aggregate.reason,
      hooks: (data.hookDecisions ?? []).map((h) => ({
        policyName: h.policyName,
        kind: h.kind,
        code: h.code,
        reason: h.reason,
      })),
    },
  };
}

type ApiGovernanceHistoryItem = {
  eventId: string;
  offerId: string;
  agencyProfileId: string;
  kind: string;
  actorKind: string;
  actorAccountId: string | null;
  fromPublicationStatus: string | null;
  toPublicationStatus: string | null;
  policyCode: string | null;
  policyName: string | null;
  reason: string | null;
  occurredAt: string;
};

export type AgencyOfferGovernanceHistoryView = {
  eventId: string;
  kind: string;
  actorKind: string;
  fromPublicationStatus: string | null;
  toPublicationStatus: string | null;
  policyCode: string | null;
  reason: string | null;
  occurredAt: string;
};

export async function listAgencyOfferGovernanceHistoryAction(offerId: string): Promise<
  | { ok: true; items: AgencyOfferGovernanceHistoryView[] }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiGovernanceHistoryItem[]>(
    `/api/agency-marketplace/moderation/offers/${encodeURIComponent(offerId)}/governance-history?take=50`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return {
    ok: true,
    items: (result.data ?? []).map((x) => ({
      eventId: x.eventId,
      kind: x.kind,
      actorKind: x.actorKind,
      fromPublicationStatus: x.fromPublicationStatus,
      toPublicationStatus: x.toPublicationStatus,
      policyCode: x.policyCode,
      reason: x.reason,
      occurredAt: x.occurredAt,
    })),
  };
}
