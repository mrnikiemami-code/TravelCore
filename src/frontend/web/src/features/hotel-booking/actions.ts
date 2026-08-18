"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import {
  HOTEL_BOOKING_ACCESS_TOKEN_HEADER,
  HOTEL_BOOKING_IDEMPOTENCY_HEADER,
  HOTEL_BOOKING_PUBLIC_INITIATIONS_PATH,
  publicHotelBookingReadPath,
  publicHotelBookingPaymentPath,
  publicHotelBookingPaymentInitiationPath,
  publicHotelBookingCancellationPath,
  type PublicHotelBookingInitiationResult,
  type PublicHotelBookingRoomInput,
  type PublicHotelBookingReadResult,
  type PublicHotelBookingPaymentReadResult,
} from "@/features/hotel-booking/types";

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

export async function initiatePublicHotelBookingAction(input: {
  placeId: string;
  checkInDate: string;
  checkOutDate: string;
  email: string;
  phone: string;
  rooms: PublicHotelBookingRoomInput[];
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicHotelBookingInitiationResult }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<PublicHotelBookingInitiationResult>(
    HOTEL_BOOKING_PUBLIC_INITIATIONS_PATH,
    {
      method: "POST",
      cache: "no-store",
      headers: await identityHeaders({
        [HOTEL_BOOKING_IDEMPOTENCY_HEADER]: input.idempotencyKey,
      }),
      body: {
        placeId: input.placeId,
        checkInDate: input.checkInDate,
        checkOutDate: input.checkOutDate,
        contact: {
          email: input.email || null,
          phone: input.phone || null,
        },
        rooms: input.rooms,
        idempotencyKey: input.idempotencyKey,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty initiation response." };
  return { ok: true, data: result.data };
}

export async function readPublicHotelBookingAction(
  hotelBookingId: string,
  accessToken: string | null,
): Promise<
  | { ok: true; data: PublicHotelBookingReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {};
  if (accessToken) {
    extra[HOTEL_BOOKING_ACCESS_TOKEN_HEADER] = accessToken;
  }

  const result = await apiGetJson<PublicHotelBookingReadResult>(
    publicHotelBookingReadPath(hotelBookingId),
    {
      cache: "no-store",
      headers: await identityHeaders(extra),
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty hotel booking response." };
  return { ok: true, data: result.data };
}

export async function readPublicHotelBookingPaymentAction(
  hotelBookingId: string,
  accessToken: string | null,
): Promise<
  | { ok: true; data: PublicHotelBookingPaymentReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {};
  if (accessToken) {
    extra[HOTEL_BOOKING_ACCESS_TOKEN_HEADER] = accessToken;
  }

  const result = await apiGetJson<PublicHotelBookingPaymentReadResult>(
    publicHotelBookingPaymentPath(hotelBookingId),
    {
      cache: "no-store",
      headers: await identityHeaders(extra),
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty payment response." };
  return { ok: true, data: result.data };
}

export async function initiatePublicHotelBookingPaymentAction(input: {
  hotelBookingId: string;
  accessToken: string | null;
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicHotelBookingPaymentReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {
    [HOTEL_BOOKING_IDEMPOTENCY_HEADER]: input.idempotencyKey,
  };
  if (input.accessToken) {
    extra[HOTEL_BOOKING_ACCESS_TOKEN_HEADER] = input.accessToken;
  }

  const result = await apiSendJson<PublicHotelBookingPaymentReadResult>(
    publicHotelBookingPaymentInitiationPath(input.hotelBookingId),
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

export async function requestPublicHotelBookingCancellationAction(input: {
  hotelBookingId: string;
  accessToken: string | null;
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicHotelBookingReadResult; outcome?: string }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {
    [HOTEL_BOOKING_IDEMPOTENCY_HEADER]: input.idempotencyKey,
  };
  if (input.accessToken) {
    extra[HOTEL_BOOKING_ACCESS_TOKEN_HEADER] = input.accessToken;
  }

  const result = await apiSendJson<{
    outcome?: string;
    booking?: PublicHotelBookingReadResult;
  }>(publicHotelBookingCancellationPath(input.hotelBookingId), {
    method: "POST",
    cache: "no-store",
    headers: await identityHeaders(extra),
    body: { idempotencyKey: input.idempotencyKey },
  });
  if (!result.ok) return failMessage(result);
  const booking = result.data?.booking;
  if (!booking) return { ok: false, message: "Empty cancellation response." };
  return { ok: true, data: booking, outcome: result.data?.outcome };
}
