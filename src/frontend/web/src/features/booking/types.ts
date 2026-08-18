export const BOOKING_ACCESS_TOKEN_HEADER = "X-TravelCore-Booking-Access-Token";
export const BOOKING_IDEMPOTENCY_HEADER = "Idempotency-Key";
export const BOOKING_PUBLIC_INITIATIONS_PATH = "/api/booking/public/initiations";

export function publicBookingReadPath(bookingId: string): string {
  return `/api/booking/public/${encodeURIComponent(bookingId)}`;
}

export function bookingAccessStorageKey(bookingId: string): string {
  return `tc.booking.access.${bookingId}`;
}

export type PublicBookingPassengerInput = {
  givenName: string;
  familyName: string;
  category: "Adult" | "Child" | "Infant";
};

export type PublicBookingInitiationResult = {
  bookingId: string;
  status: string;
  sourceKind: string;
  tourDepartureId: string;
  accessToken: string | null;
  accessTokenIssued: boolean;
  confirmed: boolean;
  monetary: {
    quoteId: string;
    sourcePriceId: string;
    currency: string;
    totalAmount: number;
    quoteExpiresAt: string;
  } | null;
  hold: {
    status: string;
    expiresAt: string;
    seatCount: number;
  } | null;
};

export type PublicBookingReadResult = {
  bookingId: string;
  status: string;
  sourceKind: string;
  tourDepartureId: string;
  confirmed: boolean;
  contact: {
    displayName: string | null;
    email: string | null;
    phone: string | null;
  } | null;
  passengers: Array<{
    passengerId: string;
    givenName: string;
    familyName: string;
    category: string;
    sequence: number;
  }>;
  monetary: PublicBookingInitiationResult["monetary"];
  hold: PublicBookingInitiationResult["hold"];
};
