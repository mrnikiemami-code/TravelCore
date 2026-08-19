"use server";

import { cookies } from "next/headers";
import { apiGetJson, apiSendJson } from "@/lib/api/client";
import {
  FLIGHT_BOOKING_ACCESS_TOKEN_HEADER,
  FLIGHT_BOOKING_IDEMPOTENCY_HEADER,
  FLIGHT_BOOKING_PUBLIC_INITIATIONS_PATH,
  FLIGHT_BOOKING_PUBLIC_SEARCH_PATH,
  publicFlightBookingReadPath,
  publicFlightBookingOffersPath,
  publicFlightBookingReservationsPath,
  publicFlightBookingPaymentPath,
  publicFlightBookingPaymentInitiationPath,
  publicFlightBookingCancellationPath,
  type PublicFlightBookingInitiationResult,
  type PublicFlightBookingReadResult,
  type PublicFlightBookingPaymentReadResult,
  type PublicFlightPassengerInput,
  type PublicFlightSearchResult,
  type PublicFlightJourneyRead,
} from "@/features/flight-booking/types";

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

export async function searchPublicFlightsAction(input: {
  originIata: string;
  destinationIata: string;
  tripType: string;
  departureDate: string;
  returnDate: string | null;
  adultCount: number;
  childCount: number;
  infantCount: number;
}): Promise<
  | { ok: true; data: PublicFlightSearchResult }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<PublicFlightSearchResult>(FLIGHT_BOOKING_PUBLIC_SEARCH_PATH, {
    method: "POST",
    cache: "no-store",
    headers: await identityHeaders(),
    body: {
      originIata: input.originIata,
      destinationIata: input.destinationIata,
      tripType: input.tripType,
      departureDate: input.departureDate,
      returnDate: input.returnDate,
      adultCount: input.adultCount,
      childCount: input.childCount,
      infantCount: input.infantCount,
    },
  });
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty search response." };
  return { ok: true, data: result.data };
}

export async function initiatePublicFlightBookingAction(input: {
  tripType: string;
  journeys: PublicFlightJourneyRead[];
  passengers: PublicFlightPassengerInput[];
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicFlightBookingInitiationResult }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<PublicFlightBookingInitiationResult>(
    FLIGHT_BOOKING_PUBLIC_INITIATIONS_PATH,
    {
      method: "POST",
      cache: "no-store",
      headers: await identityHeaders({
        [FLIGHT_BOOKING_IDEMPOTENCY_HEADER]: input.idempotencyKey,
      }),
      body: {
        tripType: input.tripType,
        journeys: input.journeys.map((journey) => ({
          segments: journey.segments.map((segment) => ({
            originIata: segment.originIata,
            destinationIata: segment.destinationIata,
            departureAt: segment.departureAt,
            departureTimeZoneId: segment.departureTimeZoneId,
            arrivalAt: segment.arrivalAt,
            arrivalTimeZoneId: segment.arrivalTimeZoneId,
            marketingCarrierIata: segment.marketingCarrierIata,
            operatingCarrierIata: segment.operatingCarrierIata,
            flightNumber: segment.flightNumber,
          })),
        })),
        passengers: input.passengers,
        idempotencyKey: input.idempotencyKey,
      },
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty initiation response." };
  return { ok: true, data: result.data };
}

export async function readPublicFlightBookingAction(
  flightBookingId: string,
  accessToken: string | null,
): Promise<
  | { ok: true; data: PublicFlightBookingReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {};
  if (accessToken) {
    extra[FLIGHT_BOOKING_ACCESS_TOKEN_HEADER] = accessToken;
  }

  const result = await apiGetJson<PublicFlightBookingReadResult>(
    publicFlightBookingReadPath(flightBookingId),
    {
      cache: "no-store",
      headers: await identityHeaders(extra),
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty flight booking response." };
  return { ok: true, data: result.data };
}

export async function acceptPublicFlightOfferAction(input: {
  flightBookingId: string;
  accessToken: string | null;
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicFlightBookingReadResult }
  | { ok: false; message: string; status?: number }
> {
  return mutateBooking(
    publicFlightBookingOffersPath(input.flightBookingId),
    input.accessToken,
    input.idempotencyKey,
  );
}

export async function requestPublicFlightReservationAction(input: {
  flightBookingId: string;
  accessToken: string | null;
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicFlightBookingReadResult }
  | { ok: false; message: string; status?: number }
> {
  return mutateBooking(
    publicFlightBookingReservationsPath(input.flightBookingId),
    input.accessToken,
    input.idempotencyKey,
  );
}

export async function readPublicFlightBookingPaymentAction(
  flightBookingId: string,
  accessToken: string | null,
): Promise<
  | { ok: true; data: PublicFlightBookingPaymentReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {};
  if (accessToken) {
    extra[FLIGHT_BOOKING_ACCESS_TOKEN_HEADER] = accessToken;
  }

  const result = await apiGetJson<PublicFlightBookingPaymentReadResult>(
    publicFlightBookingPaymentPath(flightBookingId),
    {
      cache: "no-store",
      headers: await identityHeaders(extra),
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty payment response." };
  return { ok: true, data: result.data };
}

export async function initiatePublicFlightBookingPaymentAction(input: {
  flightBookingId: string;
  accessToken: string | null;
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicFlightBookingPaymentReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {
    [FLIGHT_BOOKING_IDEMPOTENCY_HEADER]: input.idempotencyKey,
  };
  if (input.accessToken) {
    extra[FLIGHT_BOOKING_ACCESS_TOKEN_HEADER] = input.accessToken;
  }

  const result = await apiSendJson<PublicFlightBookingPaymentReadResult>(
    publicFlightBookingPaymentInitiationPath(input.flightBookingId),
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

export async function requestPublicFlightBookingCancellationAction(input: {
  flightBookingId: string;
  accessToken: string | null;
  idempotencyKey: string;
}): Promise<
  | { ok: true; data: PublicFlightBookingReadResult; outcome?: string }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {
    [FLIGHT_BOOKING_IDEMPOTENCY_HEADER]: input.idempotencyKey,
  };
  if (input.accessToken) {
    extra[FLIGHT_BOOKING_ACCESS_TOKEN_HEADER] = input.accessToken;
  }

  const result = await apiSendJson<{
    outcome?: string;
    booking?: PublicFlightBookingReadResult;
  }>(publicFlightBookingCancellationPath(input.flightBookingId), {
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

async function mutateBooking(
  path: string,
  accessToken: string | null,
  idempotencyKey: string,
): Promise<
  | { ok: true; data: PublicFlightBookingReadResult }
  | { ok: false; message: string; status?: number }
> {
  const extra: HeadersInit = {
    [FLIGHT_BOOKING_IDEMPOTENCY_HEADER]: idempotencyKey,
  };
  if (accessToken) {
    extra[FLIGHT_BOOKING_ACCESS_TOKEN_HEADER] = accessToken;
  }

  const result = await apiSendJson<PublicFlightBookingReadResult>(path, {
    method: "POST",
    cache: "no-store",
    headers: await identityHeaders(extra),
    body: { idempotencyKey },
  });
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty journey response." };
  return { ok: true, data: result.data };
}
