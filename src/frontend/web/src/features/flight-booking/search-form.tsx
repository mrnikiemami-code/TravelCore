"use client";

import { useId, useRef, useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { FieldMessage, LtrValue, Stack, Text } from "@/components/ui";
import {
  initiatePublicFlightBookingAction,
  searchPublicFlightsAction,
} from "@/features/flight-booking/actions";
import { getPublicFlightBookingCopy } from "@/features/flight-booking/copy";
import {
  flightBookingAccessStorageKey,
  type PublicFlightSearchOptionRead,
  type PublicFlightSearchResult,
} from "@/features/flight-booking/types";
import type { AppLocale } from "@/lib/i18n";

type PassengerDraft = {
  givenName: string;
  familyName: string;
  category: "Adult" | "Child" | "Infant";
};

function emptyPassenger(category: PassengerDraft["category"]): PassengerDraft {
  return { givenName: "", familyName: "", category };
}

export function PublicFlightSearchForm({ locale }: { locale: AppLocale }) {
  const copy = getPublicFlightBookingCopy(locale);
  const router = useRouter();
  const errorId = useId();
  const countsId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [originIata, setOriginIata] = useState("");
  const [destinationIata, setDestinationIata] = useState("");
  const [tripType, setTripType] = useState<"OneWay" | "RoundTrip">("OneWay");
  const [departureDate, setDepartureDate] = useState("");
  const [returnDate, setReturnDate] = useState("");
  const [adultCount, setAdultCount] = useState(1);
  const [childCount, setChildCount] = useState(0);
  const [infantCount, setInfantCount] = useState(0);
  const [search, setSearch] = useState<PublicFlightSearchResult | null>(null);
  const [selected, setSelected] = useState<PublicFlightSearchOptionRead | null>(null);
  const [passengers, setPassengers] = useState<PassengerDraft[]>([emptyPassenger("Adult")]);
  const idempotencyKeyRef = useRef<string | null>(null);

  function rebuildPassengers(adults: number, children: number, infants: number) {
    const next: PassengerDraft[] = [];
    for (let i = 0; i < adults; i++) next.push(emptyPassenger("Adult"));
    for (let i = 0; i < children; i++) next.push(emptyPassenger("Child"));
    for (let i = 0; i < infants; i++) next.push(emptyPassenger("Infant"));
    setPassengers(next);
  }

  function runSearch() {
    setError(null);
    setSelected(null);
    startTransition(() => {
      void (async () => {
        const result = await searchPublicFlightsAction({
          originIata,
          destinationIata,
          tripType,
          departureDate,
          returnDate: tripType === "RoundTrip" ? returnDate : null,
          adultCount,
          childCount,
          infantCount,
        });
        if (!result.ok) {
          setError(result.message);
          setSearch(null);
          return;
        }
        setSearch(result.data);
        rebuildPassengers(adultCount, childCount, infantCount);
      })();
    });
  }

  function submit() {
    if (!selected) {
      return;
    }
    setError(null);
    startTransition(() => {
      void (async () => {
        if (!idempotencyKeyRef.current) {
          idempotencyKeyRef.current = crypto.randomUUID();
        }
        const result = await initiatePublicFlightBookingAction({
          tripType: selected.tripType,
          journeys: selected.journeys,
          passengers,
          idempotencyKey: idempotencyKeyRef.current,
        });
        if (!result.ok) {
          setError(result.message);
          return;
        }
        if (result.data.accessToken) {
          sessionStorage.setItem(
            flightBookingAccessStorageKey(result.data.flightBookingId),
            result.data.accessToken,
          );
        }
        router.push(`/${locale}/flight-bookings/${result.data.flightBookingId}`);
      })();
    });
  }

  const statusMessage = search
    ? !search.sourceConfigured
      ? copy.unavailable
      : search.options.length === 0
        ? search.safeMessage ?? copy.noResults
        : null
    : null;

  return (
    <form
      className="flex flex-col gap-4"
      onSubmit={(event) => {
        event.preventDefault();
        if (selected) {
          submit();
        } else {
          runSearch();
        }
      }}
      aria-describedby={error ? errorId : undefined}
    >
      <Stack gap="sm">
        <label className="flex flex-col gap-1">
          <Text role="label">{copy.origin}</Text>
          <LtrValue>
            <input
              className="min-h-touch rounded-md border border-border px-3 py-2 uppercase"
              name="originIata"
              required
              maxLength={3}
              autoComplete="off"
              value={originIata}
              onChange={(event) => setOriginIata(event.target.value.toUpperCase())}
            />
          </LtrValue>
        </label>
        <label className="flex flex-col gap-1">
          <Text role="label">{copy.destination}</Text>
          <LtrValue>
            <input
              className="min-h-touch rounded-md border border-border px-3 py-2 uppercase"
              name="destinationIata"
              required
              maxLength={3}
              autoComplete="off"
              value={destinationIata}
              onChange={(event) => setDestinationIata(event.target.value.toUpperCase())}
            />
          </LtrValue>
        </label>
        <fieldset className="flex flex-col gap-2">
          <Text as="legend" role="label">
            {copy.tripType}
          </Text>
          <label className="flex min-h-11 items-center gap-2">
            <input
              type="radio"
              name="tripType"
              value="OneWay"
              checked={tripType === "OneWay"}
              onChange={() => setTripType("OneWay")}
            />
            <Text>{copy.oneWay}</Text>
          </label>
          <label className="flex min-h-11 items-center gap-2">
            <input
              type="radio"
              name="tripType"
              value="RoundTrip"
              checked={tripType === "RoundTrip"}
              onChange={() => setTripType("RoundTrip")}
            />
            <Text>{copy.roundTrip}</Text>
          </label>
        </fieldset>
        <label className="flex flex-col gap-1">
          <Text role="label">{copy.departureDate}</Text>
          <input
            className="min-h-touch rounded-md border border-border px-3 py-2"
            type="date"
            required
            value={departureDate}
            onChange={(event) => setDepartureDate(event.target.value)}
          />
        </label>
        {tripType === "RoundTrip" ? (
          <label className="flex flex-col gap-1">
            <Text role="label">{copy.returnDate}</Text>
            <input
              className="min-h-touch rounded-md border border-border px-3 py-2"
              type="date"
              required
              value={returnDate}
              onChange={(event) => setReturnDate(event.target.value)}
            />
          </label>
        ) : null}
        <fieldset className="flex flex-col gap-2" aria-describedby={countsId}>
          <Text as="legend" role="label">
            {copy.passengerCounts}
          </Text>
          <p id={countsId} className="sr-only">
            {copy.passengerCounts}
          </p>
          <label className="flex flex-col gap-1">
            <Text role="label">{copy.adults}</Text>
            <input
              className="min-h-touch rounded-md border border-border px-3 py-2"
              type="number"
              min={1}
              required
              value={adultCount}
              onChange={(event) => setAdultCount(Number(event.target.value))}
            />
          </label>
          <label className="flex flex-col gap-1">
            <Text role="label">{copy.children}</Text>
            <input
              className="min-h-touch rounded-md border border-border px-3 py-2"
              type="number"
              min={0}
              value={childCount}
              onChange={(event) => setChildCount(Number(event.target.value))}
            />
          </label>
          <label className="flex flex-col gap-1">
            <Text role="label">{copy.infants}</Text>
            <input
              className="min-h-touch rounded-md border border-border px-3 py-2"
              type="number"
              min={0}
              value={infantCount}
              onChange={(event) => setInfantCount(Number(event.target.value))}
            />
          </label>
        </fieldset>
      </Stack>

      <p className="text-body text-foreground" aria-live="polite">
        {pending ? copy.searching : statusMessage}
      </p>

      {search?.options.map((option) => (
        <Stack key={option.sourceOptionReference} gap="sm">
          {option.journeys.map((journey) => (
            <ul key={journey.ordinal} className="list-inside list-disc">
              {journey.segments.map((segment) => (
                <li key={`${option.sourceOptionReference}-${journey.ordinal}-${segment.ordinal}`}>
                  <LtrValue>
                    {segment.originIata} → {segment.destinationIata}
                  </LtrValue>{" "}
                  · {copy.departure}{" "}
                  <LtrValue>
                    {segment.departureAt} ({segment.departureTimeZoneId})
                  </LtrValue>{" "}
                  · {copy.arrival}{" "}
                  <LtrValue>
                    {segment.arrivalAt} ({segment.arrivalTimeZoneId})
                  </LtrValue>
                </li>
              ))}
            </ul>
          ))}
          <button
            type="button"
            className="min-h-11 rounded-md border px-4 py-2 focus-visible:outline"
            onClick={() => setSelected(option)}
          >
            {selected?.sourceOptionReference === option.sourceOptionReference
              ? copy.selected
              : copy.selectOption}
          </button>
        </Stack>
      ))}

      {selected ? (
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.passengersHeading}
          </Text>
          {passengers.map((passenger, index) => (
            <fieldset key={`${passenger.category}-${index}`} className="flex flex-col gap-2">
              <Text as="legend" role="label">
                {passenger.category === "Adult"
                  ? copy.adult
                  : passenger.category === "Child"
                    ? copy.child
                    : copy.infant}{" "}
                {index + 1}
              </Text>
              <label className="flex flex-col gap-1">
                <Text role="label">{copy.givenName}</Text>
                <input
                  className="min-h-touch rounded-md border border-border px-3 py-2"
                  required
                  autoComplete="given-name"
                  value={passenger.givenName}
                  onChange={(event) => {
                    const next = [...passengers];
                    next[index] = { ...passenger, givenName: event.target.value };
                    setPassengers(next);
                  }}
                />
              </label>
              <label className="flex flex-col gap-1">
                <Text role="label">{copy.familyName}</Text>
                <input
                  className="min-h-touch rounded-md border border-border px-3 py-2"
                  required
                  autoComplete="family-name"
                  value={passenger.familyName}
                  onChange={(event) => {
                    const next = [...passengers];
                    next[index] = { ...passenger, familyName: event.target.value };
                    setPassengers(next);
                  }}
                />
              </label>
            </fieldset>
          ))}
        </Stack>
      ) : null}

      {error ? (
        <FieldMessage id={errorId} tone="error">
          {error}
        </FieldMessage>
      ) : null}

      <button
        type="submit"
        className="min-h-11 rounded-md border px-4 py-2 focus-visible:outline"
        disabled={pending}
      >
        {pending
          ? copy.submitting
          : selected
            ? copy.submit
            : copy.searchAction}
      </button>
    </form>
  );
}
