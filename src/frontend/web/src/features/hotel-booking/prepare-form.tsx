"use client";

import { useId, useRef, useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { FieldMessage, LtrValue, Stack, Text } from "@/components/ui";
import { initiatePublicHotelBookingAction } from "@/features/hotel-booking/actions";
import { getPublicHotelBookingCopy } from "@/features/hotel-booking/copy";
import { hotelBookingAccessStorageKey } from "@/features/hotel-booking/types";
import type { AppLocale } from "@/lib/i18n";

type GuestDraft = {
  givenName: string;
  familyName: string;
  category: "Adult" | "Child";
  ageAtCheckInYears: string;
};

type RoomDraft = {
  guests: GuestDraft[];
};

function emptyGuest(): GuestDraft {
  return { givenName: "", familyName: "", category: "Adult", ageAtCheckInYears: "" };
}

export function PublicHotelBookingPrepareForm({
  locale,
  slug,
  placeId,
}: {
  locale: AppLocale;
  slug: string;
  placeId: string;
}) {
  const copy = getPublicHotelBookingCopy(locale);
  const router = useRouter();
  const errorId = useId();
  const [pending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [checkInDate, setCheckInDate] = useState("");
  const [checkOutDate, setCheckOutDate] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [rooms, setRooms] = useState<RoomDraft[]>([{ guests: [emptyGuest()] }]);
  const [leadKey, setLeadKey] = useState("0-0");
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
        const result = await initiatePublicHotelBookingAction({
          placeId,
          checkInDate,
          checkOutDate,
          email,
          phone,
          rooms: rooms.map((room, roomIndex) => ({
            guests: room.guests.map((guest, guestIndex) => ({
              givenName: guest.givenName,
              familyName: guest.familyName,
              category: guest.category,
              isLeadGuest: leadKey === `${roomIndex}-${guestIndex}`,
              ageAtCheckInYears:
                guest.category === "Child" && guest.ageAtCheckInYears
                  ? Number(guest.ageAtCheckInYears)
                  : null,
            })),
          })),
          idempotencyKey: ensureIdempotencyKey(),
        });
        if (!result.ok) {
          setError(result.message);
          return;
        }
        if (result.data.accessToken) {
          sessionStorage.setItem(
            hotelBookingAccessStorageKey(result.data.hotelBookingId),
            result.data.accessToken,
          );
        }
        router.push(`/${locale}/hotel-bookings/${result.data.hotelBookingId}`);
      })();
    });
  }

  return (
    <form
      className="flex flex-col gap-4"
      onSubmit={(event) => {
        event.preventDefault();
        submit();
      }}
      aria-describedby={error ? errorId : undefined}
    >
      <Stack gap="sm">
        <Text as="h2" role="heading">
          {copy.stayHeading}
        </Text>
        <label className="flex flex-col gap-1">
          <Text role="label">{copy.checkIn}</Text>
          <input
            className="min-h-touch rounded-md border border-border px-3 py-2"
            type="date"
            required
            value={checkInDate}
            onChange={(event) => setCheckInDate(event.target.value)}
          />
        </label>
        <label className="flex flex-col gap-1">
          <Text role="label">{copy.checkOut}</Text>
          <input
            className="min-h-touch rounded-md border border-border px-3 py-2"
            type="date"
            required
            value={checkOutDate}
            onChange={(event) => setCheckOutDate(event.target.value)}
          />
        </label>
      </Stack>

      <Stack gap="sm">
        <Text as="h2" role="heading">
          {copy.contactHeading}
        </Text>
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

      <Stack gap="md">
        <Text as="h2" role="heading">
          {copy.roomsHeading}
        </Text>
        {rooms.map((room, roomIndex) => (
          <fieldset
            key={roomIndex}
            className="flex flex-col gap-3 rounded-md border border-border p-3"
          >
            <legend>
              <Text role="label">
                {copy.roomLabel} {roomIndex + 1}
              </Text>
            </legend>
            <Text as="h3" role="heading">
              {copy.guestsHeading}
            </Text>
            {room.guests.map((guest, guestIndex) => {
              const key = `${roomIndex}-${guestIndex}`;
              return (
                <div key={key} className="grid grid-cols-1 gap-2 sm:grid-cols-2">
                  <label className="flex flex-col gap-1">
                    <Text role="label">{copy.givenName}</Text>
                    <input
                      className="min-h-touch rounded-md border border-border px-3 py-2"
                      autoComplete="given-name"
                      required
                      value={guest.givenName}
                      onChange={(event) =>
                        setRooms((current) =>
                          current.map((row, ri) =>
                            ri === roomIndex
                              ? {
                                  ...row,
                                  guests: row.guests.map((item, gi) =>
                                    gi === guestIndex
                                      ? { ...item, givenName: event.target.value }
                                      : item,
                                  ),
                                }
                              : row,
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
                      required
                      value={guest.familyName}
                      onChange={(event) =>
                        setRooms((current) =>
                          current.map((row, ri) =>
                            ri === roomIndex
                              ? {
                                  ...row,
                                  guests: row.guests.map((item, gi) =>
                                    gi === guestIndex
                                      ? { ...item, familyName: event.target.value }
                                      : item,
                                  ),
                                }
                              : row,
                          ),
                        )
                      }
                    />
                  </label>
                  <label className="flex flex-col gap-1">
                    <Text role="label">{copy.category}</Text>
                    <select
                      className="min-h-touch rounded-md border border-border bg-background px-3 py-2"
                      value={guest.category}
                      onChange={(event) =>
                        setRooms((current) =>
                          current.map((row, ri) =>
                            ri === roomIndex
                              ? {
                                  ...row,
                                  guests: row.guests.map((item, gi) =>
                                    gi === guestIndex
                                      ? {
                                          ...item,
                                          category: event.target.value as GuestDraft["category"],
                                        }
                                      : item,
                                  ),
                                }
                              : row,
                          ),
                        )
                      }
                    >
                      <option value="Adult">{copy.adult}</option>
                      <option value="Child">{copy.child}</option>
                    </select>
                  </label>
                  {guest.category === "Child" ? (
                    <label className="flex flex-col gap-1">
                      <Text role="label">{copy.ageAtCheckIn}</Text>
                      <input
                        className="min-h-touch rounded-md border border-border px-3 py-2"
                        type="number"
                        min={0}
                        max={17}
                        required
                        value={guest.ageAtCheckInYears}
                        onChange={(event) =>
                          setRooms((current) =>
                            current.map((row, ri) =>
                              ri === roomIndex
                                ? {
                                    ...row,
                                    guests: row.guests.map((item, gi) =>
                                      gi === guestIndex
                                        ? { ...item, ageAtCheckInYears: event.target.value }
                                        : item,
                                    ),
                                  }
                                : row,
                            ),
                          )
                        }
                      />
                    </label>
                  ) : null}
                  <label className="flex min-h-touch items-center gap-2">
                    <input
                      type="radio"
                      name="lead-guest"
                      checked={leadKey === key}
                      onChange={() => setLeadKey(key)}
                    />
                    <Text role="label">{copy.leadGuest}</Text>
                  </label>
                </div>
              );
            })}
            <button
              type="button"
              className="min-h-touch self-start rounded-md border border-border px-3 py-2 text-sm"
              onClick={() =>
                setRooms((current) =>
                  current.map((row, ri) =>
                    ri === roomIndex ? { ...row, guests: [...row.guests, emptyGuest()] } : row,
                  ),
                )
              }
            >
              {copy.addGuest}
            </button>
          </fieldset>
        ))}
        <button
          type="button"
          className="min-h-touch self-start rounded-md border border-border px-3 py-2 text-sm"
          onClick={() => setRooms((current) => [...current, { guests: [emptyGuest()] }])}
        >
          {copy.addRoom}
        </button>
      </Stack>

      {error ? (
        <FieldMessage id={errorId} tone="error">
          {error}
        </FieldMessage>
      ) : (
        <FieldMessage id={`${errorId}-status`} tone="status">
          {pending ? copy.submitting : copy.prepareNote}
        </FieldMessage>
      )}
      <button
        type="submit"
        className="min-h-touch rounded-md border border-border px-4 py-2 focus-visible:outline"
        disabled={pending}
      >
        {pending ? copy.submitting : copy.submit}
      </button>
      <a className="underline" href={`/${locale}/places/${encodeURIComponent(slug)}`}>
        {copy.backToPlace}
      </a>
    </form>
  );
}
