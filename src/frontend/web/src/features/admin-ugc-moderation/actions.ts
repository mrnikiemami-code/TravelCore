"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type { ModerationQueueTravelogueView } from "@/features/admin-ugc-moderation/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiModerationQueueTravelogue = {
  travelogueId: string;
  actorId: string;
  localeCode: string;
  title: string;
  bodyPreview: string;
  moderationStatus: string;
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

function mapItem(item: ApiModerationQueueTravelogue): ModerationQueueTravelogueView {
  return {
    travelogueId: item.travelogueId,
    actorId: item.actorId,
    localeCode: item.localeCode,
    title: item.title,
    bodyPreview: item.bodyPreview,
    moderationStatus: item.moderationStatus,
    publicationStatus: item.publicationStatus,
    createdAt: item.createdAt,
    updatedAt: item.updatedAt,
  };
}

export async function listPendingTraveloguesAction(input?: {
  take?: number;
}): Promise<
  | { ok: true; items: ModerationQueueTravelogueView[] }
  | { ok: false; message: string; status?: number }
> {
  const params = new URLSearchParams();
  if (input?.take) params.set("take", String(input.take));
  const qs = params.toString();
  const result = await apiGetJson<ApiModerationQueueTravelogue[]>(
    `/api/ugc/moderation/travelogues/pending${qs ? `?${qs}` : ""}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapItem) };
}

async function mutateTravelogueAction(
  travelogueId: string,
  action: "approve" | "reject" | "publish",
): Promise<
  | { ok: true; item: ModerationQueueTravelogueView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiModerationQueueTravelogue>(
    `/api/ugc/moderation/travelogues/${encodeURIComponent(travelogueId)}/${action}`,
    {
      method: "POST",
      headers: await authHeaders(),
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty response." };
  return { ok: true, item: mapItem(result.data) };
}

export async function approveTravelogueAction(travelogueId: string) {
  return mutateTravelogueAction(travelogueId, "approve");
}

export async function rejectTravelogueAction(travelogueId: string) {
  return mutateTravelogueAction(travelogueId, "reject");
}

export async function publishTravelogueAction(travelogueId: string) {
  return mutateTravelogueAction(travelogueId, "publish");
}
