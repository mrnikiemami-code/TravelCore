export const FLIGHT_BOOKING_ACCESS_TOKEN_HEADER =
  "X-TravelCore-Flight-Booking-Access-Token";
export const FLIGHT_BOOKING_IDEMPOTENCY_HEADER = "Idempotency-Key";
export const FLIGHT_BOOKING_PUBLIC_SEARCH_PATH = "/api/flight-booking/public/search";
export const FLIGHT_BOOKING_PUBLIC_INITIATIONS_PATH =
  "/api/flight-booking/public/initiations";

export function publicFlightBookingReadPath(flightBookingId: string): string {
  return `/api/flight-booking/public/${encodeURIComponent(flightBookingId)}`;
}

export function publicFlightBookingOffersPath(flightBookingId: string): string {
  return `/api/flight-booking/public/${encodeURIComponent(flightBookingId)}/offers`;
}

export function publicFlightBookingReservationsPath(flightBookingId: string): string {
  return `/api/flight-booking/public/${encodeURIComponent(flightBookingId)}/reservations`;
}

export function publicFlightBookingPaymentPath(flightBookingId: string): string {
  return `/api/flight-booking/public/${encodeURIComponent(flightBookingId)}/payment`;
}

export function publicFlightBookingPaymentInitiationPath(flightBookingId: string): string {
  return `/api/flight-booking/public/${encodeURIComponent(flightBookingId)}/payment/initiation`;
}

export function publicFlightBookingCancellationPath(flightBookingId: string): string {
  return `/api/flight-booking/public/${encodeURIComponent(flightBookingId)}/cancellation`;
}

export function flightBookingAccessStorageKey(flightBookingId: string): string {
  return `tc.flight-booking.access.${flightBookingId}`;
}

export type PublicFlightPassengerInput = {
  givenName: string;
  familyName: string;
  category: "Adult" | "Child" | "Infant";
};

export type PublicFlightSegmentRead = {
  segmentId?: string;
  ordinal: number;
  originIata: string;
  destinationIata: string;
  departureAt: string;
  departureTimeZoneId: string;
  arrivalAt: string;
  arrivalTimeZoneId: string;
  marketingCarrierIata: string;
  operatingCarrierIata: string | null;
  flightNumber: string | null;
};

export type PublicFlightJourneyRead = {
  journeyId?: string;
  ordinal: number;
  segments: PublicFlightSegmentRead[];
};

export type PublicFlightSearchOptionRead = {
  sourceOptionReference: string;
  tripType: string;
  journeys: PublicFlightJourneyRead[];
  observedAt: string;
  expiresAt: string | null;
};

export type PublicFlightSearchResult = {
  completion: string;
  sourceConfigured: boolean;
  safeMessage: string | null;
  options: PublicFlightSearchOptionRead[];
};

export type PublicFlightPassengerRead = {
  passengerId: string;
  givenName: string;
  familyName: string;
  category: string;
};

export type PublicFlightBaggageRead = {
  quantity: number | null;
  weight: number | null;
  unit: string | null;
  category: string | null;
  passengerCategory: string | null;
};

export type PublicFlightFareRulesRead = {
  refundable: boolean;
  changeable: boolean;
  ticketingDeadline: string | null;
  cancelPenaltyAmount: number | null;
  cancelPenaltyCurrencyCode: string | null;
  baggage: PublicFlightBaggageRead[];
};

export type PublicFlightOfferRead = {
  snapshotId: string;
  currencyCode: string;
  totalAmount: number;
  offerExpiresAt: string | null;
  offerExpired: boolean;
  ticketingDeadline: string | null;
  fareRules: PublicFlightFareRulesRead | null;
};

export type PublicFlightBookingInitiationResult = {
  flightBookingId: string;
  status: string;
  presentationState: string;
  accessToken: string | null;
  accessTokenIssued: boolean;
  confirmed: boolean;
  tripType: string;
  journeys: PublicFlightJourneyRead[];
  passengers: PublicFlightPassengerRead[];
};

export type PublicFlightBookingReadResult = {
  flightBookingId: string;
  status: string;
  presentationState: string;
  confirmed: boolean;
  tripType: string;
  journeys: PublicFlightJourneyRead[];
  passengers: PublicFlightPassengerRead[];
  offer: PublicFlightOfferRead | null;
  reservation: {
    presentationStatus: string;
    reservationLocator: string | null;
    expiresAt: string | null;
  } | null;
  tickets: Array<{
    passengerId: string;
    status: string;
    ticketNumber: string | null;
  }>;
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
  offerExpired: boolean;
  safeMessage: string | null;
};

export type PublicFlightBookingPaymentReadResult = {
  flightBookingId: string;
  flightBookingStatus: string;
  flightBookingConfirmed: boolean;
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
  offer: PublicFlightOfferRead | null;
};
