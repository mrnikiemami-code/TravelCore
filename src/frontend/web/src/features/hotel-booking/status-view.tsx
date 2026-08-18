"use client";

import { useEffect, useId, useRef, useState, useTransition } from "react";
import { BidiText, FieldMessage, LtrValue, MoneyText, Stack, Text } from "@/components/ui";
import {
  readPublicHotelBookingAction,
  requestPublicHotelBookingCancellationAction,
} from "@/features/hotel-booking/actions";
import { getPublicHotelBookingCopy } from "@/features/hotel-booking/copy";
import {
  hotelBookingAccessStorageKey,
  type PublicHotelBookingReadResult,
} from "@/features/hotel-booking/types";
import type { AppLocale } from "@/lib/i18n";

export function PublicHotelBookingStatusView({
  locale,
  hotelBookingId,
}: {
  locale: AppLocale;
  hotelBookingId: string;
}) {
  const copy = getPublicHotelBookingCopy(locale);
  const statusId = useId();
  const [data, setData] = useState<PublicHotelBookingReadResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();
  const cancelKeyRef = useRef<string | null>(null);

  function token(): string | null {
    return typeof sessionStorage === "undefined"
      ? null
      : sessionStorage.getItem(hotelBookingAccessStorageKey(hotelBookingId));
  }

  useEffect(() => {
    const accessToken =
      typeof sessionStorage === "undefined"
        ? null
        : sessionStorage.getItem(hotelBookingAccessStorageKey(hotelBookingId));
    void readPublicHotelBookingAction(hotelBookingId, accessToken).then((result) => {
      if (!result.ok) {
        setError(result.message);
        return;
      }
      setData(result.data);
    });
  }, [hotelBookingId]);

  function cancel() {
    startTransition(() => {
      void (async () => {
        if (!cancelKeyRef.current) {
          cancelKeyRef.current = crypto.randomUUID();
        }
        const result = await requestPublicHotelBookingCancellationAction({
          hotelBookingId,
          accessToken: token(),
          idempotencyKey: cancelKeyRef.current,
        });
        if (!result.ok) {
          setError(result.message);
          return;
        }
        setData(result.data);
      })();
    });
  }

  if (error && !data) {
    return <Text>{copy.unauthorized}</Text>;
  }

  if (!data) {
    return (
      <Text role="muted" aria-live="polite">
        {copy.loading}
      </Text>
    );
  }

  const statusNote =
    data.presentationState === "CancellationPending"
      ? copy.cancelPending
      : data.presentationState === "RefundPending"
        ? copy.refundPending
        : data.refundStatus === "Succeeded"
          ? copy.refundSucceeded
          : data.safeMessage ?? copy.pendingNote;

  return (
    <Stack gap="md">
      <Text as="h1" role="heading">
        {copy.statusTitle}
      </Text>
      <p className="text-body text-foreground" aria-live="polite">
        {statusNote}
      </p>
      <Text role="caption">
        {data.confirmed ? copy.statusTitle : copy.notConfirmed} ·{" "}
        <LtrValue>{data.status}</LtrValue> · <LtrValue>{data.presentationState}</LtrValue>
      </Text>
      <Text>
        {copy.checkIn}: <LtrValue>{data.checkInDate}</LtrValue> · {copy.checkOut}:{" "}
        <LtrValue>{data.checkOutDate}</LtrValue>
      </Text>
      {data.monetary ? (
        <Text>
          {copy.monetaryLabel}:{" "}
          <MoneyText
            money={{
              amount: String(data.monetary.totalAmount),
              currencyCode: data.monetary.currencyCode,
            }}
            locale={locale}
          />
        </Text>
      ) : null}
      {data.hold ? (
        <Text>
          {copy.holdLabel}: <LtrValue>{data.hold.status}</LtrValue>
        </Text>
      ) : null}
      {data.reservation?.confirmationCode ? (
        <Text>
          {copy.confirmationCode}: <LtrValue>{data.reservation.confirmationCode}</LtrValue>
        </Text>
      ) : null}
      {data.monetary?.cancellationTerms.length ? (
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.termsHeading}
          </Text>
          <ul className="list-inside list-disc">
            {data.monetary.cancellationTerms.map((term, index) => (
              <li key={`${term.effectiveFrom}-${index}`}>
                <MoneyText
                  money={{
                    amount: String(term.penaltyAmount),
                    currencyCode: term.currencyCode,
                  }}
                  locale={locale}
                />
                {!term.currentlyExecutable ? ` — ${copy.termsNotExecutable}` : null}
              </li>
            ))}
          </ul>
        </Stack>
      ) : null}
      {data.rooms.map((room) => (
        <Stack key={room.roomReservationId} gap="sm">
          <Text as="h2" role="heading">
            {copy.roomLabel} {room.ordinal}
          </Text>
          <ul className="list-inside list-disc">
            {room.guests.map((guest) => (
              <li key={guest.guestId}>
                <BidiText>
                  {guest.givenName} {guest.familyName}
                </BidiText>{" "}
                · <LtrValue>{guest.category}</LtrValue>
                {guest.isLeadGuest ? ` · ${copy.leadGuest}` : null}
                {guest.ageAtCheckInYears != null
                  ? ` · ${copy.ageAtCheckIn} ${guest.ageAtCheckInYears}`
                  : null}
              </li>
            ))}
          </ul>
        </Stack>
      ))}
      {error ? (
        <FieldMessage id={`${statusId}-error`} tone="error">
          {error}
        </FieldMessage>
      ) : null}
      <a
        className="underline"
        href={`/${locale}/hotel-bookings/${encodeURIComponent(hotelBookingId)}/payment`}
      >
        {copy.payTitle}
      </a>
      {data.cancellationAvailable ? (
        <button
          type="button"
          className="min-h-11 rounded-md border px-4 py-2 focus-visible:outline"
          disabled={pending}
          onClick={cancel}
        >
          {pending ? copy.submitting : copy.cancelAction}
        </button>
      ) : null}
    </Stack>
  );
}
