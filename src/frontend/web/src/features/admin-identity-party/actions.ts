"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type {
  AccountStatusView,
  PartySummaryView,
} from "@/features/admin-identity-party/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiAccount = {
  id: string;
  email: string;
  status: string;
  associatedPartyId?: string | null;
};

type ApiPartySummary = {
  id: string;
  kind: string;
  displayName: string;
  status: string;
  primaryEmail?: string | null;
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

function mapAccount(a: ApiAccount): AccountStatusView {
  return {
    id: a.id,
    email: a.email,
    status: a.status,
    associatedPartyId: a.associatedPartyId ?? null,
  };
}

function mapParty(p: ApiPartySummary): PartySummaryView {
  return {
    id: p.id,
    kind: p.kind,
    displayName: p.displayName,
    status: p.status,
    primaryEmail: p.primaryEmail ?? null,
  };
}

export async function createIdentityAccountAction(input: {
  email: string;
  password: string;
}): Promise<{ ok: true; account: AccountStatusView } | { ok: false; message: string; status?: number }> {
  const result = await apiSendJson<ApiAccount>("/api/identity/accounts/", {
    method: "POST",
    body: { email: input.email, password: input.password },
    headers: await authHeaders(),
  });
  if (!result.ok) {
    return { ok: false, message: result.message, status: result.status };
  }
  return { ok: true, account: mapAccount(result.data) };
}

export async function searchPartiesAction(query: string): Promise<{
  ok: true;
  items: PartySummaryView[];
} | { ok: false; message: string; status?: number }> {
  const q = encodeURIComponent(query.trim());
  const result = await apiGetJson<{ items: ApiPartySummary[] }>(
    `/api/party/parties/?q=${q}&take=20`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) {
    return { ok: false, message: result.message, status: result.status };
  }
  return { ok: true, items: (result.data.items ?? []).map(mapParty) };
}

export async function createPersonPartyAction(input: {
  displayName: string;
  givenName: string;
  familyName: string;
}): Promise<{ ok: true; party: PartySummaryView } | { ok: false; message: string; status?: number }> {
  const result = await apiSendJson<ApiPartySummary>("/api/party/parties/", {
    method: "POST",
    body: {
      kind: "Person",
      displayName: input.displayName,
      givenName: input.givenName,
      familyName: input.familyName,
    },
    headers: await authHeaders(),
  });
  if (!result.ok) {
    return { ok: false, message: result.message, status: result.status };
  }
  return {
    ok: true,
    party: mapParty({
      id: result.data.id,
      kind: result.data.kind,
      displayName: result.data.displayName,
      status: result.data.status,
      primaryEmail: result.data.primaryEmail,
    }),
  };
}

export async function linkPartyAction(input: {
  accountId: string;
  partyId: string;
  mode: "link" | "replace";
}): Promise<{ ok: true; account: AccountStatusView } | { ok: false; message: string; status?: number }> {
  const path = `/api/identity/accounts/${input.accountId}/party-association`;
  const result = await apiSendJson<ApiAccount>(path, {
    method: input.mode === "replace" ? "PUT" : "POST",
    body: { partyId: input.partyId },
    headers: await authHeaders(),
  });
  if (!result.ok) {
    return { ok: false, message: result.message, status: result.status };
  }
  return { ok: true, account: mapAccount(result.data) };
}

export async function unlinkPartyAction(accountId: string): Promise<
  { ok: true; account: AccountStatusView } | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiAccount>(
    `/api/identity/accounts/${accountId}/party-association`,
    { method: "DELETE", headers: await authHeaders() },
  );
  if (!result.ok) {
    return { ok: false, message: result.message, status: result.status };
  }
  return { ok: true, account: mapAccount(result.data) };
}

export async function loadAccountAction(accountId: string): Promise<
  { ok: true; account: AccountStatusView } | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiAccount>(
    `/api/identity/accounts/${accountId}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) {
    return { ok: false, message: result.message, status: result.status };
  }
  return { ok: true, account: mapAccount(result.data) };
}

export async function loadPartySummaryAction(partyId: string): Promise<
  { ok: true; party: PartySummaryView } | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiPartySummary>(`/api/party/parties/${partyId}`, {
    headers: await authHeaders(),
    cache: "no-store",
  });
  if (!result.ok) {
    return { ok: false, message: result.message, status: result.status };
  }
  return { ok: true, party: mapParty(result.data) };
}
