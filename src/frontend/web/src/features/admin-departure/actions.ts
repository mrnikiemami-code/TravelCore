"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import type {
  TourDepartureDetailView,
  TourDepartureSummaryView,
} from "@/features/admin-departure/types";

const AUTH_COOKIE = "TravelCore.Identity";

type ApiTourDeparture = {
  id: string;
  tourProductId: string;
  status: string;
  startDate?: string | null;
  endDate?: string | null;
  timeZoneId?: string | null;
  minimumPax?: number | null;
  maximumPax?: number | null;
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

function mapDeparture(d: ApiTourDeparture): TourDepartureDetailView {
  return {
    id: d.id,
    tourProductId: d.tourProductId,
    status: d.status,
    startDate: d.startDate ?? null,
    endDate: d.endDate ?? null,
    timeZoneId: d.timeZoneId ?? null,
    minimumPax: d.minimumPax ?? null,
    maximumPax: d.maximumPax ?? null,
    createdAt: d.createdAt,
    updatedAt: d.updatedAt,
  };
}

export async function listTourDeparturesAction(input?: {
  tourProductId?: string;
  status?: string;
  take?: number;
}): Promise<
  | { ok: true; items: TourDepartureSummaryView[] }
  | { ok: false; message: string; status?: number }
> {
  const params = new URLSearchParams();
  if (input?.tourProductId) params.set("tourProductId", input.tourProductId);
  if (input?.status) params.set("status", input.status);
  if (input?.take) params.set("take", String(input.take));
  const qs = params.toString();
  const result = await apiGetJson<ApiTourDeparture[]>(
    `/api/tour/departures${qs ? `?${qs}` : ""}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  return { ok: true, items: (result.data ?? []).map(mapDeparture) };
}

export async function getTourDepartureAction(
  id: string,
): Promise<
  | { ok: true; item: TourDepartureDetailView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiGetJson<ApiTourDeparture>(
    `/api/tour/departures/${encodeURIComponent(id)}`,
    { headers: await authHeaders(), cache: "no-store" },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty response." };
  return { ok: true, item: mapDeparture(result.data) };
}

export async function createTourDepartureAction(input: {
  tourProductId: string;
}): Promise<
  | { ok: true; item: TourDepartureDetailView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTourDeparture>("/api/tour/departures/", {
    method: "POST",
    headers: await authHeaders(),
    body: { tourProductId: input.tourProductId },
  });
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty response." };
  return { ok: true, item: mapDeparture(result.data) };
}

export async function setTourDepartureScheduleAction(input: {
  id: string;
  startDate: string;
  endDate: string;
  timeZoneId: string;
}): Promise<
  | { ok: true; item: TourDepartureDetailView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTourDeparture>(
    `/api/tour/departures/${encodeURIComponent(input.id)}/schedule`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: {
        startDate: input.startDate,
        endDate: input.endDate,
        timeZoneId: input.timeZoneId,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty response." };
  return { ok: true, item: mapDeparture(result.data) };
}

export async function setTourDepartureCapacityAction(input: {
  id: string;
  minimumPax: number;
  maximumPax: number;
}): Promise<
  | { ok: true; item: TourDepartureDetailView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTourDeparture>(
    `/api/tour/departures/${encodeURIComponent(input.id)}/capacity`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: {
        minimumPax: input.minimumPax,
        maximumPax: input.maximumPax,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty response." };
  return { ok: true, item: mapDeparture(result.data) };
}

export async function setTourDepartureStatusAction(input: {
  id: string;
  status: string;
}): Promise<
  | { ok: true; item: TourDepartureDetailView }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiTourDeparture>(
    `/api/tour/departures/${encodeURIComponent(input.id)}/status`,
    {
      method: "PUT",
      headers: await authHeaders(),
      body: { status: input.status },
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty response." };
  return { ok: true, item: mapDeparture(result.data) };
}
