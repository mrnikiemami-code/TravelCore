"use server";

import { apiSendJson } from "@/lib/api/client";
import type { TripPlannerDraftState, TripPlannerTimingKind } from "@/features/trip-planner/types";
import { createEmptyTripPlannerDraft } from "@/features/trip-planner/types";

const DRAFT_HEADER = "X-TripPlanner-Draft-Token";

type ApiCreateIntent = {
  intentId: string;
  draftAccessToken: string;
  publicPath: string;
  createdAt: string;
};

type ApiIntentDraft = {
  intentId: string;
  draftAccessToken: string;
  planningRevision: number;
  planningNote?: string | null;
  preferences: {
    timing: {
      kind: string;
      exactStartDate?: string | null;
      exactEndDate?: string | null;
      flexibleEarliestStart?: string | null;
      flexibleLatestStart?: string | null;
      flexibleMaxTripDurationDays?: number | null;
      approximateYear?: number | null;
      approximateMonth?: number | null;
      approximateSeason?: string | null;
    };
    travelers?: {
      adultCount: number;
      childCount: number;
      infantCount: number;
    } | null;
    budget?: {
      minimumAmount?: number | null;
      maximumAmount?: number | null;
      currencyCode: string;
    } | null;
    accommodation?: string | null;
    transport?: string | null;
    tripStyle?: string | null;
    interestCodes?: string[] | null;
    destination: {
      undecided: boolean;
      logicalDestinationIds?: string[] | null;
    };
    travelerNote?: string | null;
  };
  createdAt: string;
  updatedAt: string;
  leadSubmitted: boolean;
  leadId?: string | null;
  publicPath: string;
};

type ApiSubmitLead = {
  intentId: string;
  leadId: string;
  leadStatus: string;
  submittedAt: string;
  alreadySubmitted: boolean;
};

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

function draftHeaders(token: string): HeadersInit {
  return { [DRAFT_HEADER]: token };
}

function parseIds(raw: string): string[] {
  return raw
    .split(",")
    .map((x) => x.trim())
    .filter(Boolean);
}

function buildUpdateBody(state: TripPlannerDraftState) {
  const timingKind = state.timingKind;
  const timing = {
    kind: timingKind,
    exactStartDate: timingKind === "ExactDates" ? state.exactStart || null : null,
    exactEndDate: timingKind === "ExactDates" ? state.exactEnd || null : null,
    flexibleEarliestStart:
      timingKind === "FlexibleRange" ? state.flexibleEarliest || null : null,
    flexibleLatestStart:
      timingKind === "FlexibleRange" ? state.flexibleLatest || null : null,
    flexibleMaxTripDurationDays: null,
    approximateYear: null,
    approximateMonth: null,
    approximateSeason: null,
  };

  const ids = parseIds(state.destinationIds);
  return {
    planningNote: null,
    timing,
    travelers: {
      adultCount: Number(state.adults || "0"),
      childCount: Number(state.children || "0"),
      infantCount: Number(state.infants || "0"),
    },
    budget:
      state.budgetMin || state.budgetMax
        ? {
            minimumAmount: state.budgetMin ? Number(state.budgetMin) : null,
            maximumAmount: state.budgetMax ? Number(state.budgetMax) : null,
            currencyCode: state.currency || "USD",
          }
        : null,
    accommodation: state.accommodation || null,
    transport: state.transport || null,
    tripStyle: state.tripStyle || null,
    interestCodes: state.interests
      ? state.interests.split(",").map((x) => x.trim()).filter(Boolean)
      : [],
    destination: {
      undecided: state.destinationUndecided || ids.length === 0,
      logicalDestinationIds: ids.length > 0 ? ids : null,
    },
    travelerNote: state.travelerNote || null,
  };
}

function mapDraftToState(draft: ApiIntentDraft, previous: TripPlannerDraftState): TripPlannerDraftState {
  const p = draft.preferences;
  return {
    ...previous,
    intentId: draft.intentId,
    draftAccessToken: draft.draftAccessToken,
    planningRevision: draft.planningRevision,
    destinationUndecided: p.destination.undecided,
    destinationIds: (p.destination.logicalDestinationIds ?? []).join(", "),
    timingKind: (p.timing.kind as TripPlannerTimingKind) ?? "Undecided",
    exactStart: p.timing.exactStartDate ?? "",
    exactEnd: p.timing.exactEndDate ?? "",
    flexibleEarliest: p.timing.flexibleEarliestStart ?? "",
    flexibleLatest: p.timing.flexibleLatestStart ?? "",
    adults: String(p.travelers?.adultCount ?? previous.adults),
    children: String(p.travelers?.childCount ?? previous.children),
    infants: String(p.travelers?.infantCount ?? previous.infants),
    budgetMin: p.budget?.minimumAmount != null ? String(p.budget.minimumAmount) : "",
    budgetMax: p.budget?.maximumAmount != null ? String(p.budget.maximumAmount) : "",
    currency: p.budget?.currencyCode ?? previous.currency,
    accommodation: p.accommodation ?? previous.accommodation,
    transport: p.transport ?? previous.transport,
    tripStyle: p.tripStyle ?? previous.tripStyle,
    interests: (p.interestCodes ?? []).join(", "),
    travelerNote: p.travelerNote ?? "",
    leadSubmitted: draft.leadSubmitted,
    leadId: draft.leadId ?? null,
  };
}

export async function createTripIntentAction(locale: string): Promise<
  | { ok: true; state: TripPlannerDraftState }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiCreateIntent>(
    "/api/trip-planner/public/intents",
    {
      method: "POST",
      body: { localeCode: locale },
      cache: "no-store",
    },
  );
  if (!result.ok) return failMessage(result);

  const created = result.data;
  if (!created) return { ok: false, message: "Empty create response." };

  return {
    ok: true,
    state: createEmptyTripPlannerDraft(created.intentId, created.draftAccessToken),
  };
}

export async function syncTripIntentDraftAction(state: TripPlannerDraftState): Promise<
  | { ok: true; state: TripPlannerDraftState }
  | { ok: false; message: string; status?: number }
> {
  const result = await apiSendJson<ApiIntentDraft>(
    `/api/trip-planner/public/intents/${state.intentId}`,
    {
      method: "PATCH",
      headers: draftHeaders(state.draftAccessToken),
      body: buildUpdateBody(state),
      cache: "no-store",
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty draft response." };
  return { ok: true, state: mapDraftToState(result.data, state) };
}

export async function submitTripLeadAction(state: TripPlannerDraftState): Promise<
  | { ok: true; state: TripPlannerDraftState; leadId: string; alreadySubmitted: boolean }
  | { ok: false; message: string; status?: number }
> {
  const sync = await syncTripIntentDraftAction(state);
  if (!sync.ok) return sync;

  const synced = sync.state;
  const result = await apiSendJson<ApiSubmitLead>(
    `/api/trip-planner/public/intents/${synced.intentId}/submit`,
    {
      method: "POST",
      headers: draftHeaders(synced.draftAccessToken),
      body: {
        displayName: synced.displayName || null,
        email: synced.email || null,
        phone: synced.phone || null,
        followUpContactAllowed: synced.followUpAllowed,
        marketingAllowed: synced.marketingAllowed,
        privacyNoticeVersion: synced.privacyVersion || null,
        preferredContactChannel: synced.preferredChannel || null,
      },
      cache: "no-store",
    },
  );
  if (!result.ok) return failMessage(result);
  if (!result.data) return { ok: false, message: "Empty submit response." };

  return {
    ok: true,
    leadId: result.data.leadId,
    alreadySubmitted: result.data.alreadySubmitted,
    state: {
      ...synced,
      leadSubmitted: true,
      leadId: result.data.leadId,
    },
  };
}
