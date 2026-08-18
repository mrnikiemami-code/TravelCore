"use client";

import { useRef, useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { FieldMessage, LtrValue, Stack, Text } from "@/components/ui";
import { initiatePublicBookingAction } from "@/features/booking/actions";
import { getPublicBookingCopy } from "@/features/booking/copy";
import { bookingAccessStorageKey } from "@/features/booking/types";
import type { AppLocale } from "@/lib/i18n";

type DepartureOption = {
  id: string;
  label: string;
};

type PassengerDraft = {
  givenName: string;
  familyName: string;
  category: "Adult" | "Child" | "Infant";
};

export function PublicBookingPrepareForm({
  locale,
  slug,
  departures,
  initialDepartureId,
}: {
  locale: AppLocale;
  slug: string;
  departures: DepartureOption[];
  initialDepartureId?: string;
}) {
  const copy = getPublicBookingCopy(locale);
  const router = useRouter();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [tourDepartureId, setTourDepartureId] = useState(
    initialDepartureId && departures.some((d) => d.id === initialDepartureId)
      ? initialDepartureId
      : (departures[0]?.id ?? ""),
  );
  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [passengers, setPassengers] = useState<PassengerDraft[]>([
    { givenName: "", familyName: "", category: "Adult" },
  ]);
  const idempotencyKeyRef = useRef<string | null>(null);

  function ensureIdempotencyKey(): string {
    if (!idempotencyKeyRef.current) {
      idempotencyKeyRef.current = crypto.randomUUID();
    }
    return idempotencyKeyRef.current;
  }

  function submit() {
    setError(null);
    startTransition(() => {
      void (async () => {
        const result = await initiatePublicBookingAction({
          tourDepartureId,
          displayName,
          email,
          phone,
          passengers,
          idempotencyKey: ensureIdempotencyKey(),
        });
        if (!result.ok) {
          setError(result.message);
          return;
        }
        if (result.data.accessToken) {
          sessionStorage.setItem(
            bookingAccessStorageKey(result.data.bookingId),
            result.data.accessToken,
          );
        }
        router.push(`/${locale}/bookings/${result.data.bookingId}`);
      })();
    });
  }

  if (departures.length === 0) {
    return (
      <Stack gap="sm">
        <Text>{copy.missingDeparture}</Text>
        <a className="underline" href={`/${locale}/tours/${encodeURIComponent(slug)}`}>
          {copy.backToTour}
        </a>
      </Stack>
    );
  }

  return (
    <form
      className="flex flex-col gap-4"
      onSubmit={(event) => {
        event.preventDefault();
        submit();
      }}
    >
      <label className="flex flex-col gap-1">
        <Text role="label">{copy.selectDeparture}</Text>
        <select
          className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
          value={tourDepartureId}
          onChange={(event) => setTourDepartureId(event.target.value)}
          required
        >
          {departures.map((departure) => (
            <option key={departure.id} value={departure.id}>
              {departure.label}
            </option>
          ))}
        </select>
      </label>

      <Stack gap="sm">
        <Text as="h2" role="heading">
          {copy.contactHeading}
        </Text>
        <label className="flex flex-col gap-1">
          <Text role="label">{copy.displayName}</Text>
          <input
            className="min-h-touch rounded-md border border-border px-3 py-2"
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
          />
        </label>
        <label className="flex flex-col gap-1">
          <Text role="label">{copy.email}</Text>
          <LtrValue>
            <input
              className="min-h-touch rounded-md border border-border px-3 py-2"
              type="email"
              autoComplete="email"
              inputMode="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </LtrValue>
        </label>
        <label className="flex flex-col gap-1">
          <Text role="label">{copy.phone}</Text>
          <LtrValue>
            <input
              className="min-h-touch rounded-md border border-border px-3 py-2"
              type="tel"
              autoComplete="tel"
              inputMode="tel"
              value={phone}
              onChange={(event) => setPhone(event.target.value)}
            />
          </LtrValue>
        </label>
      </Stack>

      <Stack gap="sm">
        <Text as="h2" role="heading">
          {copy.passengersHeading}
        </Text>
        {passengers.map((passenger, index) => (
          <div key={index} className="grid grid-cols-1 gap-2 sm:grid-cols-3">
            <label className="flex flex-col gap-1">
              <Text role="label">{copy.givenName}</Text>
              <input
                className="min-h-touch rounded-md border border-border px-3 py-2"
                autoComplete="given-name"
                value={passenger.givenName}
                required
                onChange={(event) =>
                  setPassengers((rows) =>
                    rows.map((row, i) =>
                      i === index ? { ...row, givenName: event.target.value } : row,
                    ),
                  )
                }
              />
            </label>
            <label className="flex flex-col gap-1">
              <Text role="label">{copy.familyName}</Text>
              <input
                className="min-h-touch rounded-md border border-border px-3 py-2"
                autoComplete="family-name"
                value={passenger.familyName}
                required
                onChange={(event) =>
                  setPassengers((rows) =>
                    rows.map((row, i) =>
                      i === index ? { ...row, familyName: event.target.value } : row,
                    ),
                  )
                }
              />
            </label>
            <label className="flex flex-col gap-1">
              <Text role="label">{copy.category}</Text>
              <select
                className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
                value={passenger.category}
                onChange={(event) =>
                  setPassengers((rows) =>
                    rows.map((row, i) =>
                      i === index
                        ? {
                            ...row,
                            category: event.target.value as PassengerDraft["category"],
                          }
                        : row,
                    ),
                  )
                }
              >
                <option value="Adult">{copy.adult}</option>
                <option value="Child">{copy.child}</option>
                <option value="Infant">{copy.infant}</option>
              </select>
            </label>
          </div>
        ))}
        <button
          type="button"
          className="min-h-touch self-start rounded-md border border-border px-3 py-2 text-sm"
          onClick={() =>
            setPassengers((rows) => [
              ...rows,
              { givenName: "", familyName: "", category: "Adult" },
            ])
          }
        >
          {copy.addPassenger}
        </button>
      </Stack>

      {error ? (
        <FieldMessage id="public-booking-prepare-error" tone="error">
          {error}
        </FieldMessage>
      ) : null}
      <button
        type="submit"
        className="min-h-touch rounded-md border border-border px-4 py-2"
        disabled={pending}
      >
        {pending ? copy.submitting : copy.submit}
      </button>
    </form>
  );
}
