"use client";

import { useMemo, useState, useTransition } from "react";
import type { AppLocale } from "@/lib/i18n";
import { LtrValue, Stack, Surface, Text } from "@/components/ui";
import {
  createTripIntentAction,
  submitTripLeadAction,
  syncTripIntentDraftAction,
} from "@/features/trip-planner/actions";
import { getTripPlannerWorkflowCopy } from "@/features/trip-planner/copy";
import {
  TRIP_PLANNER_STEPS,
  type TripPlannerDraftState,
  type TripPlannerStep,
  type TripPlannerTimingKind,
} from "@/features/trip-planner/types";

export type TripPlannerWorkflowIslandProps = {
  locale: AppLocale;
  apiConfigured: boolean;
};

const TIMING_OPTIONS: TripPlannerTimingKind[] = [
  "Undecided",
  "ExactDates",
  "FlexibleRange",
  "ApproximatePeriod",
];

export function TripPlannerWorkflowIsland({
  locale,
  apiConfigured,
}: TripPlannerWorkflowIslandProps) {
  const copy = getTripPlannerWorkflowCopy(locale);
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [stepIndex, setStepIndex] = useState(0);
  const [draft, setDraft] = useState<TripPlannerDraftState | null>(null);

  const step = TRIP_PLANNER_STEPS[stepIndex] ?? "destination";
  const stepLabel = useMemo(() => stepTitle(copy, step), [copy, step]);

  function patchDraft(partial: Partial<TripPlannerDraftState>) {
    setDraft((prev) => (prev ? { ...prev, ...partial } : prev));
  }

  function run(job: () => Promise<void>) {
    setError(null);
    startTransition(() => {
      void (async () => {
        try {
          await job();
        } catch (e) {
          setError(e instanceof Error ? e.message : String(e));
        }
      })();
    });
  }

  if (draft?.leadSubmitted) {
    return (
      <Surface className="flex flex-col gap-3 p-4">
        <Text as="h2" role="heading">
          {copy.submittedTitle}
        </Text>
        <Text role="muted">{copy.submittedBody}</Text>
        {draft.leadId ? (
          <Text role="caption">
            Lead · <LtrValue>{draft.leadId}</LtrValue>
          </Text>
        ) : null}
      </Surface>
    );
  }

  return (
    <Stack gap="md">
      <Text role="muted">{copy.pageIntro}</Text>
      <Text role="caption">{copy.honestCtaNote}</Text>
      {!apiConfigured ? <Text role="caption">{copy.apiMissing}</Text> : null}
      {error ? (
        <Text role="caption">
          {copy.errorPrefix} {error}
        </Text>
      ) : null}
      {pending ? <Text role="caption">{copy.busy}</Text> : null}

      {!draft ? (
        <Surface className="flex flex-col gap-3 p-4">
          <button
            className="min-h-touch w-fit rounded-md bg-foreground px-4 text-background disabled:opacity-50"
            type="button"
            disabled={!apiConfigured || pending}
            onClick={() =>
              run(async () => {
                const result = await createTripIntentAction(locale);
                if (!result.ok) throw new Error(result.message);
                setDraft(result.state);
                setStepIndex(0);
              })
            }
          >
            {copy.startPlanning}
          </button>
        </Surface>
      ) : (
        <Surface className="flex flex-col gap-4 p-4">
          <Text as="h2" role="heading">
            {stepLabel}
          </Text>

          {step === "destination" ? (
            <DestinationStep draft={draft} copy={copy} onChange={patchDraft} />
          ) : null}
          {step === "timing" ? (
            <TimingStep draft={draft} copy={copy} onChange={patchDraft} />
          ) : null}
          {step === "travelers" ? (
            <TravelersStep draft={draft} copy={copy} onChange={patchDraft} />
          ) : null}
          {step === "preferences" ? (
            <PreferencesStep draft={draft} copy={copy} onChange={patchDraft} />
          ) : null}
          {step === "budget" ? (
            <BudgetStep draft={draft} copy={copy} onChange={patchDraft} />
          ) : null}
          {step === "contact" ? (
            <ContactStep draft={draft} copy={copy} onChange={patchDraft} />
          ) : null}
          {step === "consent" ? (
            <ConsentStep draft={draft} copy={copy} onChange={patchDraft} />
          ) : null}
          {step === "review" ? (
            <ReviewStep draft={draft} copy={copy} />
          ) : null}

          <div className="flex flex-wrap gap-2">
            {stepIndex > 0 ? (
              <button
                className="min-h-touch rounded-md border border-border px-4 disabled:opacity-50"
                type="button"
                disabled={pending}
                onClick={() => setStepIndex((i) => Math.max(0, i - 1))}
              >
                {copy.back}
              </button>
            ) : null}
            {step !== "review" ? (
              <button
                className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                type="button"
                disabled={!apiConfigured || pending}
                onClick={() =>
                  run(async () => {
                    const synced = await syncTripIntentDraftAction(draft);
                    if (!synced.ok) throw new Error(synced.message);
                    setDraft(synced.state);
                    setStepIndex((i) => Math.min(TRIP_PLANNER_STEPS.length - 1, i + 1));
                  })
                }
              >
                {copy.next}
              </button>
            ) : (
              <button
                className="min-h-touch rounded-md bg-foreground px-4 text-background disabled:opacity-50"
                type="button"
                disabled={!apiConfigured || pending}
                onClick={() =>
                  run(async () => {
                    const submitted = await submitTripLeadAction(draft);
                    if (!submitted.ok) throw new Error(submitted.message);
                    setDraft(submitted.state);
                  })
                }
              >
                {copy.submitLead}
              </button>
            )}
          </div>
        </Surface>
      )}
    </Stack>
  );
}

function stepTitle(
  copy: ReturnType<typeof getTripPlannerWorkflowCopy>,
  step: TripPlannerStep,
): string {
  switch (step) {
    case "destination":
      return copy.stepDestination;
    case "timing":
      return copy.stepTiming;
    case "travelers":
      return copy.stepTravelers;
    case "preferences":
      return copy.stepPreferences;
    case "budget":
      return copy.stepBudget;
    case "contact":
      return copy.stepContact;
    case "consent":
      return copy.stepConsent;
    case "review":
      return copy.stepReview;
    default:
      return copy.stepReview;
  }
}

type StepProps = {
  draft: TripPlannerDraftState;
  copy: ReturnType<typeof getTripPlannerWorkflowCopy>;
  onChange: (partial: Partial<TripPlannerDraftState>) => void;
};

function DestinationStep({ draft, copy, onChange }: StepProps) {
  return (
    <Stack gap="sm">
      <label className="flex items-center gap-2 text-sm">
        <input
          type="checkbox"
          checked={draft.destinationUndecided}
          onChange={(e) => onChange({ destinationUndecided: e.target.checked })}
        />
        {copy.destinationUndecided}
      </label>
      {!draft.destinationUndecided ? (
        <label className="flex flex-col gap-1 text-sm">
          {copy.destinationIdsLabel}
          <LtrValue>
            <input
              className="min-h-touch rounded-md border border-border px-3"
              value={draft.destinationIds}
              onChange={(e) => onChange({ destinationIds: e.target.value })}
            />
          </LtrValue>
        </label>
      ) : null}
    </Stack>
  );
}

function TimingStep({ draft, copy, onChange }: StepProps) {
  return (
    <Stack gap="sm">
      <label className="flex flex-col gap-1 text-sm">
        {copy.timingKindLabel}
        <select
          className="min-h-touch rounded-md border border-border px-3"
          value={draft.timingKind}
          onChange={(e) =>
            onChange({ timingKind: e.target.value as TripPlannerTimingKind })
          }
        >
          {TIMING_OPTIONS.map((kind) => (
            <option key={kind} value={kind}>
              {timingLabel(copy, kind)}
            </option>
          ))}
        </select>
      </label>
      {draft.timingKind === "ExactDates" ? (
        <>
          <LtrField
            label={copy.exactStartLabel}
            value={draft.exactStart}
            onChange={(v) => onChange({ exactStart: v })}
          />
          <LtrField
            label={copy.exactEndLabel}
            value={draft.exactEnd}
            onChange={(v) => onChange({ exactEnd: v })}
          />
        </>
      ) : null}
      {draft.timingKind === "FlexibleRange" ? (
        <>
          <LtrField
            label={copy.flexibleEarliestLabel}
            value={draft.flexibleEarliest}
            onChange={(v) => onChange({ flexibleEarliest: v })}
          />
          <LtrField
            label={copy.flexibleLatestLabel}
            value={draft.flexibleLatest}
            onChange={(v) => onChange({ flexibleLatest: v })}
          />
        </>
      ) : null}
    </Stack>
  );
}

function timingLabel(
  copy: ReturnType<typeof getTripPlannerWorkflowCopy>,
  kind: TripPlannerTimingKind,
): string {
  switch (kind) {
    case "Undecided":
      return copy.timingUndecided;
    case "ExactDates":
      return copy.timingExact;
    case "FlexibleRange":
      return copy.timingFlexible;
    case "ApproximatePeriod":
      return copy.timingApproximate;
    default:
      return kind;
  }
}

function TravelersStep({ draft, copy, onChange }: StepProps) {
  return (
    <Stack gap="sm">
      <LtrField label={copy.adultsLabel} value={draft.adults} onChange={(v) => onChange({ adults: v })} />
      <LtrField
        label={copy.childrenLabel}
        value={draft.children}
        onChange={(v) => onChange({ children: v })}
      />
      <LtrField label={copy.infantsLabel} value={draft.infants} onChange={(v) => onChange({ infants: v })} />
    </Stack>
  );
}

function PreferencesStep({ draft, copy, onChange }: StepProps) {
  return (
    <Stack gap="sm">
      <LtrField
        label={copy.accommodationLabel}
        value={draft.accommodation}
        onChange={(v) => onChange({ accommodation: v })}
      />
      <LtrField label={copy.transportLabel} value={draft.transport} onChange={(v) => onChange({ transport: v })} />
      <LtrField label={copy.tripStyleLabel} value={draft.tripStyle} onChange={(v) => onChange({ tripStyle: v })} />
      <LtrField label={copy.interestsLabel} value={draft.interests} onChange={(v) => onChange({ interests: v })} />
      <label className="flex flex-col gap-1 text-sm">
        {copy.travelerNoteLabel}
        <textarea
          className="min-h-[5rem] rounded-md border border-border px-3 py-2"
          value={draft.travelerNote}
          onChange={(e) => onChange({ travelerNote: e.target.value })}
        />
      </label>
    </Stack>
  );
}

function BudgetStep({ draft, copy, onChange }: StepProps) {
  return (
    <Stack gap="sm">
      <LtrField label={copy.budgetMinLabel} value={draft.budgetMin} onChange={(v) => onChange({ budgetMin: v })} />
      <LtrField label={copy.budgetMaxLabel} value={draft.budgetMax} onChange={(v) => onChange({ budgetMax: v })} />
      <LtrField label={copy.currencyLabel} value={draft.currency} onChange={(v) => onChange({ currency: v })} />
    </Stack>
  );
}

function ContactStep({ draft, copy, onChange }: StepProps) {
  return (
    <Stack gap="sm">
      <label className="flex flex-col gap-1 text-sm">
        {copy.displayNameLabel}
        <input
          className="min-h-touch rounded-md border border-border px-3"
          value={draft.displayName}
          onChange={(e) => onChange({ displayName: e.target.value })}
        />
      </label>
      <LtrField label={copy.emailLabel} value={draft.email} onChange={(v) => onChange({ email: v })} />
      <LtrField label={copy.phoneLabel} value={draft.phone} onChange={(v) => onChange({ phone: v })} />
    </Stack>
  );
}

function ConsentStep({ draft, copy, onChange }: StepProps) {
  return (
    <Stack gap="sm">
      <label className="flex items-center gap-2 text-sm">
        <input
          type="checkbox"
          checked={draft.followUpAllowed}
          onChange={(e) => onChange({ followUpAllowed: e.target.checked })}
        />
        {copy.followUpLabel}
      </label>
      <label className="flex items-center gap-2 text-sm">
        <input
          type="checkbox"
          checked={draft.marketingAllowed}
          onChange={(e) => onChange({ marketingAllowed: e.target.checked })}
        />
        {copy.marketingLabel}
      </label>
      <LtrField
        label={copy.privacyVersionLabel}
        value={draft.privacyVersion}
        onChange={(v) => onChange({ privacyVersion: v })}
      />
      <LtrField
        label={copy.preferredChannelLabel}
        value={draft.preferredChannel}
        onChange={(v) => onChange({ preferredChannel: v })}
      />
    </Stack>
  );
}

function ReviewStep({
  draft,
  copy,
}: {
  draft: TripPlannerDraftState;
  copy: ReturnType<typeof getTripPlannerWorkflowCopy>;
}) {
  return (
    <Stack gap="sm">
      <Text role="muted">{copy.reviewHeading}</Text>
      <Text role="caption">
        {copy.stepDestination}:{" "}
        {draft.destinationUndecided ? copy.destinationUndecided : draft.destinationIds || "—"}
      </Text>
      <Text role="caption">
        {copy.stepTiming}: {draft.timingKind}
      </Text>
      <Text role="caption">
        {copy.stepTravelers}: <LtrValue>{`${draft.adults}/${draft.children}/${draft.infants}`}</LtrValue>
      </Text>
      <Text role="caption">
        {copy.emailLabel}: <LtrValue>{draft.email || "—"}</LtrValue>
      </Text>
    </Stack>
  );
}

function LtrField({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <label className="flex flex-col gap-1 text-sm">
      {label}
      <LtrValue>
        <input
          className="min-h-touch rounded-md border border-border px-3"
          value={value}
          onChange={(e) => onChange(e.target.value)}
        />
      </LtrValue>
    </label>
  );
}
