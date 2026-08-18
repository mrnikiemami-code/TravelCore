"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import {
  BOOKING_ACCESS_TOKEN_HEADER,
  BOOKING_IDEMPOTENCY_HEADER,
  BOOKING_PUBLIC_INITIATIONS_PATH,
  publicBookingReadPath,
  publicBookingPaymentPath,
  publicBookingPaymentInitiationPath,
  type PublicBookingInitiationResult,
  type PublicBookingPassengerInput,
  type PublicBookingReadResult,
  type PublicBookingPaymentReadResult,
} from "@/features/booking/types";

const AUTH_COOKIE = "TravelCore.Identity";

async function identityHeaders(extra?: HeadersInit): Promise<HeadersInit> {
  const jar = await cookies();
  const ticket = jar.get(AUTH_COOKIE)?.value;
  const headers = new Headers(extra);
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

export async function initiatePublicBookingAction(input: {
  tourDepartureId: string;
  displayName: string;
  email: string;
  phone: string;
  passengers: PublicBookingPassengerInput[];
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicBookingInitiationResult }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<PublicBookingInitiationResult>(
    BOOKING_PUBLIC_INITIATIONS_PATH,
    {
      method: "POST",
      cache: "no-store",
      headers: await identityHeaders({
        [BOOKING_IDEMPOTENCY_HEADER]: input.idempotencyKey,
      }),
      body: {
        tourDepartureId: input.tourDepartureId,
        contact: {
          displayName: input.displayName || null,
          email: input.email || null,
          phone: input.phone || null,
        },
        passengers: input.passengers,
        idempotencyKey: input.idempotencyKey,
        sourceKind: "Direct",
      },
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty initiation response." };
  return { ok: true, data: result.data };
}

export async function readPublicBookingAction(
  bookingId: string,
  accessToken: string | null,
): Promise<
  | { ok: true; data: PublicBookingReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {};
  if (accessToken) {
    extra[BOOKING_ACCESS_TOKEN_HEADER] = accessToken;
  }

  const result = await apiGetJson<PublicBookingReadResult>(
    publicBookingReadPath(bookingId),
    {
      cache: "no-store",
      headers: await identityHeaders(extra),
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty booking response." };
  return { ok: true, data: result.data };
}

export async function readPublicBookingPaymentAction(
  bookingId: string,
  accessToken: string | null,
): Promise<
  | { ok: true; data: PublicBookingPaymentReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {};
  if (accessToken) {
    extra[BOOKING_ACCESS_TOKEN_HEADER] = accessToken;
  }

  const result = await apiGetJson<PublicBookingPaymentReadResult>(
    publicBookingPaymentPath(bookingId),
    {
      cache: "no-store",
      headers: await identityHeaders(extra),
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty payment response." };
  return { ok: true, data: result.data };
}

export async function initiatePublicBookingPaymentAction(input: {
  bookingId: string;
  accessToken: string | null;
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicBookingPaymentReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {
    [BOOKING_IDEMPOTENCY_HEADER]: input.idempotencyKey,
  };
  if (input.accessToken) {
    extra[BOOKING_ACCESS_TOKEN_HEADER] = input.accessToken;
  }

  const result = await apiSendJson<PublicBookingPaymentReadResult>(
    publicBookingPaymentInitiationPath(input.bookingId),
    {
      method: "POST",
      cache: "no-store",
      headers: await identityHeaders(extra),
      body: { idempotencyKey: input.idempotencyKey },
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty payment initiation response." };
  return { ok: true, data: result.data };
}
