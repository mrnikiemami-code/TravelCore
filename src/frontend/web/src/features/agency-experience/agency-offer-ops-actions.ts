"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";

const AUTH_COOKIE = "TravelCore.Identity";

export type AgencyOfferPanelItem = {
  id: string;
  agencyProfileId: string;
  tourProductId: string;
  referencedTourDepartureId: string | null;
  departureScopeMode: string;
  departureScopeIds: string[];
  salesChannel: string;
  titleOverride: string | null;
  highlight: string | null;
  commercialNotes: string | null;
  requiresManualConfirmation: boolean;
  exclusiveListing: boolean;
  salesOpen: boolean;
  status: string;
  visibility: string;
  publicationStatus: string;
  createdAt: string;
  updatedAt: string;
};

export type AgencyProfilePanel = {
  id: string;
  partyId: string;
  displayName: string;
  status: string;
};

async function identityHeaders(): Promise<HeadersInit> {
  const jar = await cookies();
  const ticket = jar.get(AUTH_COOKIE)?.value;
  const headers = new Headers();
  if (ticket) {
    headers.set("cookie", `${AUTH_COOKIE}=${ticket}`);
  }
  return headers;
}

export async function loadActingAgencyProfile(): Promise<
  | { ok: true; profile: AgencyProfilePanel }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<AgencyProfilePanel>(
    "/api/agency-marketplace/profiles/me",
    { cache: "no-store", headers: await identityHeaders() },
  );
  if (!result.ok) {
    return {
      ok: false,
      message: result.message ?? "Acting AgencyProfile unavailable.",
      status: result.status,
    };
  }
  if (!result.data) {
    return { ok: false, message: "Empty AgencyProfile response." };
  }
  return { ok: true, profile: result.data };
}

export async function loadAgencyOffersForActing(): Promise<
  | { ok: true; items: AgencyOfferPanelItem[] }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<AgencyOfferPanelItem[]>(
    "/api/agency-marketplace/offers",
    { cache: "no-store", headers: await identityHeaders() },
  );
  if (!result.ok) {
    return {
      ok: false,
      message: result.message ?? "Offer list unavailable.",
      status: result.status,
    };
  }
  return { ok: true, items: result.data ?? [] };
}

export async function loadAgencyOfferDetail(
  offerId: string,
): Promise<
  | { ok: true; item: AgencyOfferPanelItem }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<AgencyOfferPanelItem>(
    `/api/agency-marketplace/offers/${encodeURIComponent(offerId)}`,
    { cache: "no-store", headers: await identityHeaders() },
  );
  if (!result.ok) {
    return {
      ok: false,
      message: result.message ?? "Offer detail unavailable.",
      status: result.status,
    };
  }
  if (!result.data) {
    return { ok: false, message: "Empty offer response." };
  }
  return { ok: true, item: result.data };
}

export async function createAgencyOfferAction(input: {
  tourProductId: string;
  titleOverride?: string;
  salesChannel?: string;
}): Promise<
  | { ok: true; item: AgencyOfferPanelItem }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<AgencyOfferPanelItem>(
    "/api/agency-marketplace/offers",
    {
      method: "POST",
      cache: "no-store",
      headers: await identityHeaders(),
      body: {
        // Server overwrites with acting AgencyProfileId (ownership).
        agencyProfileId: "00000000-0000-0000-0000-000000000000",
        tourProductId: input.tourProductId,
        titleOverride: input.titleOverride || null,
        salesChannel: input.salesChannel || "Public",
        departureScopeMode: "All",
      },
    },
  );
  if (!result.ok) {
    return {
      ok: false,
      message: result.message ?? "Create offer failed.",
      status: result.status,
    };
  }
  if (!result.data) {
    return { ok: false, message: "Empty create response." };
  }
  return { ok: true, item: result.data };
}

export type AgencyOfferLifecycleAction =
  | "activate"
  | "list"
  | "submit"
  | "publish"
  | "unpublish"
  | "open-sales"
  | "close-sales"
  | "suspend"
  | "retire";

export async function mutateAgencyOfferLifecycleAction(
  offerId: string,
  action: AgencyOfferLifecycleAction,
): Promise<{ ok: true } | { ok: false; message: string; status?: number }> {
  const result = await apiSendJson<unknown>(
    `/api/agency-marketplace/offers/${encodeURIComponent(offerId)}/${action}`,
    {
      method: "POST",
      cache: "no-store",
      headers: await identityHeaders(),
    },
  );
  if (!result.ok) {
    return {
      ok: false,
      message: result.message ?? "Lifecycle action failed.",
      status: result.status,
    };
  }
  return { ok: true };
}
