export const HOTEL_BOOKING_ACCESS_TOKEN_HEADER =
  "X-TravelCore-Hotel-Booking-Access-Token";
export const HOTEL_BOOKING_IDEMPOTENCY_HEADER = "Idempotency-Key";
export const HOTEL_BOOKING_PUBLIC_INITIATIONS_PATH =
  "/api/hotel-booking/public/initiations";

export function publicHotelBookingReadPath(hotelBookingId: string): string {
  return `/api/hotel-booking/public/${encodeURIComponent(hotelBookingId)}`;
}

export function publicHotelBookingAvailabilityPath(hotelBookingId: string): string {
  return `/api/hotel-booking/public/${encodeURIComponent(hotelBookingId)}/availability`;
}

export function publicHotelBookingRateOffersPath(hotelBookingId: string): string {
  return `/api/hotel-booking/public/${encodeURIComponent(hotelBookingId)}/rate-offers`;
}

export function publicHotelBookingPaymentPath(hotelBookingId: string): string {
  return `/api/hotel-booking/public/${encodeURIComponent(hotelBookingId)}/payment`;
}

export function publicHotelBookingPaymentInitiationPath(hotelBookingId: string): string {
  return `/api/hotel-booking/public/${encodeURIComponent(hotelBookingId)}/payment/initiation`;
}

export function publicHotelBookingCancellationPath(hotelBookingId: string): string {
  return `/api/hotel-booking/public/${encodeURIComponent(hotelBookingId)}/cancellation`;
}

export function hotelBookingAccessStorageKey(hotelBookingId: string): string {
  return `tc.hotel-booking.access.${hotelBookingId}`;
}

export type PublicHotelBookingGuestInput = {
  givenName: string;
  familyName: string;
  category: "Adult" | "Child";
  isLeadGuest: boolean;
  /** Child occupancy uses AgeAtCheckIn years; birth date is not collected. */
  ageAtCheckInYears: number | null;
};

export type PublicHotelBookingRoomInput = {
  guests: PublicHotelBookingGuestInput[];
};

export type PublicHotelBookingRoomRead = {
  roomReservationId: string;
  ordinal: number;
  guests: Array<{
    guestId: string;
    givenName: string;
    familyName: string;
    category: string;
    ageAtCheckInYears: number | null;
    isLeadGuest: boolean;
  }>;
};

export type PublicHotelBookingCancellationTerm = {
  effectiveFrom: string;
  effectiveUntil: string | null;
  penaltyAmount: number;
  currencyCode: string;
  currentlyExecutable: boolean;
};

export type PublicHotelBookingMonetaryRead = {
  snapshotId: string;
  currencyCode: string;
  totalAmount: number;
  offerExpiresAt: string | null;
  offerExpired: boolean;
  cancellationTerms: PublicHotelBookingCancellationTerm[];
  publicExplanation: string | null;
};

export type PublicHotelBookingInitiationResult = {
  hotelBookingId: string;
  status: string;
  presentationState: string;
  accessToken: string | null;
  accessTokenIssued: boolean;
  confirmed: boolean;
  placeId: string;
  checkInDate: string;
  checkOutDate: string;
  rooms: PublicHotelBookingRoomRead[];
};

export type PublicHotelBookingReadResult = {
  hotelBookingId: string;
  status: string;
  presentationState: string;
  confirmed: boolean;
  placeId: string;
  checkInDate: string;
  checkOutDate: string;
  contact: { email: string | null; phone: string | null } | null;
  rooms: PublicHotelBookingRoomRead[];
  monetary: PublicHotelBookingMonetaryRead | null;
  hold: { status: string; expiresAt: string | null } | null;
  reservation: { status: string; confirmationCode: string | null } | null;
  cancellation: {
    status: string;
    financialOutcome: string | null;
    penaltyAmount: number | null;
    refundAmount: number | null;
    currencyCode: string | null;
  } | null;
  paymentStatus: string | null;
  refundStatus: string | null;
  cancellationAvailable: boolean;
  rateExpired: boolean;
  safeMessage: string | null;
};

export type PublicHotelBookingPaymentReadResult = {
  hotelBookingId: string;
  hotelBookingStatus: string;
  hotelBookingConfirmed: boolean;
  presentationState: string;
  paymentId: string | null;
  paymentStatus: string | null;
  amount: number | null;
  currencyCode: string | null;
  providerInitiationPossible: boolean;
  latestAttemptStatus: string | null;
  refundStatus: string | null;
  safeAction: string;
  redirectUri: string | null;
  monetary: PublicHotelBookingMonetaryRead | null;
};
