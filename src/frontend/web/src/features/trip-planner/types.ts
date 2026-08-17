export type TripPlannerTimingKind =
  | "Undecided"
  | "ExactDates"
  | "FlexibleRange"
  | "ApproximatePeriod";

export type TripPlannerDraftState = {
  intentId: string;
  draftAccessToken: string;
  planningRevision: number;
  destinationUndecided: boolean;
  destinationIds: string;
  timingKind: TripPlannerTimingKind;
  exactStart: string;
  exactEnd: string;
  flexibleEarliest: string;
  flexibleLatest: string;
  adults: string;
  children: string;
  infants: string;
  budgetMin: string;
  budgetMax: string;
  currency: string;
  accommodation: string;
  transport: string;
  tripStyle: string;
  interests: string;
  travelerNote: string;
  displayName: string;
  email: string;
  phone: string;
  followUpAllowed: boolean;
  marketingAllowed: boolean;
  privacyVersion: string;
  preferredChannel: string;
  leadSubmitted: boolean;
  leadId: string | null;
};

export type TripPlannerStep =
  | "destination"
  | "timing"
  | "travelers"
  | "preferences"
  | "budget"
  | "contact"
  | "consent"
  | "review";

export const TRIP_PLANNER_STEPS: TripPlannerStep[] = [
  "destination",
  "timing",
  "travelers",
  "preferences",
  "budget",
  "contact",
  "consent",
  "review",
];

export function createEmptyTripPlannerDraft(
  intentId: string,
  draftAccessToken: string,
): TripPlannerDraftState {
  return {
    intentId,
    draftAccessToken,
    planningRevision: 1,
    destinationUndecided: true,
    destinationIds: "",
    timingKind: "Undecided",
    exactStart: "",
    exactEnd: "",
    flexibleEarliest: "",
    flexibleLatest: "",
    adults: "2",
    children: "0",
    infants: "0",
    budgetMin: "",
    budgetMax: "",
    currency: "USD",
    accommodation: "Any",
    transport: "Any",
    tripStyle: "Balanced",
    interests: "",
    travelerNote: "",
    displayName: "",
    email: "",
    phone: "",
    followUpAllowed: true,
    marketingAllowed: false,
    privacyVersion: "P18-PRIVACY-V1",
    preferredChannel: "Email",
    leadSubmitted: false,
    leadId: null,
  };
}
