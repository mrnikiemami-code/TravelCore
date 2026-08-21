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

const fieldClass =
  "min-h-touch rounded-lg border border-border bg-background px-3 py-2 outline-none ring-[#1D4ED8] focus:ring-2";

/**
 * Public booking prepare form (TC-P36-T005 / P38-T005).
 * Creates Pending booking only — Quote inside Booking · no Payment · no Confirm.
 * Optional agencyOfferId is passed for server validation; FE never forges Agency SourceKind.
 */
export function PublicBookingPrepareForm({
  locale,
  slug,
  departures,
  initialDepartureId,
  agencyOfferId,
}: {
  locale: AppLocale;
  slug: string;
  departures: DepartureOption[];
  initialDepartureId?: string;
  agencyOfferId?: string;
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
          agencyOfferId: agencyOfferId || null,
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
        <a
          className="inline-flex min-h-touch items-center text-sm font-medium text-[#1D4ED8] underline-offset-2 hover:underline"
          href={`/${locale}/tours/${encodeURIComponent(slug)}`}
        >
          {copy.backToTour}
        </a>
      </Stack>
    );
  }

  return (
    <form
      className="flex flex-col gap-5"
      onSubmit={(event) => {
        event.preventDefault();
        submit();
      }}
    >
      <label className="flex flex-col gap-1.5">
        <Text role="label">{copy.selectDeparture}</Text>
        <select
          className={fieldClass}
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
        <Text as="h2" role="heading" className="text-base font-semibold">
          {copy.contactHeading}
        </Text>
        <label className="flex flex-col gap-1.5">
          <Text role="label">{copy.displayName}</Text>
          <input
            className={fieldClass}
            value={displayName}
            onChange={(event) => setDisplayName(event.target.value)}
            autoComplete="name"
          />
        </label>
        <label className="flex flex-col gap-1.5">
          <Text role="label">{copy.email}</Text>
          <LtrValue>
            <input
              className={`${fieldClass} w-full`}
              type="email"
              autoComplete="email"
              inputMode="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              required
            />
          </LtrValue>
        </label>
        <label className="flex flex-col gap-1.5">
          <Text role="label">{copy.phone}</Text>
          <LtrValue>
            <input
              className={`${fieldClass} w-full`}
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
        <Text as="h2" role="heading" className="text-base font-semibold">
          {copy.passengersHeading}
        </Text>
        {passengers.map((passenger, index) => (
          <div
            key={index}
            className="grid grid-cols-1 gap-3 rounded-xl border border-border bg-background p-3 sm:grid-cols-3"
          >
            <label className="flex flex-col gap-1.5">
              <Text role="label">{copy.givenName}</Text>
              <input
                className={fieldClass}
                autoComplete="given-name"
                value={passenger.givenName}
                required
                onChange={(event) =>
                  setPassengers((rows) =>
                    rows.map((row, i) =>
                      i === index
                        ? { ...row, givenName: event.target.value }
                        : row,
                    ),
                  )
                }
              />
            </label>
            <label className="flex flex-col gap-1.5">
              <Text role="label">{copy.familyName}</Text>
              <input
                className={fieldClass}
                autoComplete="family-name"
                value={passenger.familyName}
                required
                onChange={(event) =>
                  setPassengers((rows) =>
                    rows.map((row, i) =>
                      i === index
                        ? { ...row, familyName: event.target.value }
                        : row,
                    ),
                  )
                }
              />
            </label>
            <label className="flex flex-col gap-1.5">
              <Text role="label">{copy.category}</Text>
              <select
                className={fieldClass}
                value={passenger.category}
                onChange={(event) =>
                  setPassengers((rows) =>
                    rows.map((row, i) =>
                      i === index
                        ? {
                            ...row,
                            category: event.target
                              .value as PassengerDraft["category"],
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
          className="min-h-touch self-start rounded-lg border border-border px-3 py-2 text-sm font-medium hover:border-[#1D4ED8]/40"
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

      <div className="rounded-xl border border-[#1D4ED8]/15 bg-[#1D4ED8]/[0.04] p-3">
        <Text role="caption">{copy.prepareNote}</Text>
      </div>

      <button
        type="submit"
        className="min-h-touch rounded-lg bg-[#1D4ED8] px-4 py-3 text-sm font-semibold text-white hover:bg-[#1E40AF] disabled:opacity-60"
        disabled={pending}
      >
        {pending ? copy.submitting : copy.submit}
      </button>
    </form>
  );
}
