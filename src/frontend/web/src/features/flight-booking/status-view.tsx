"use client";

import { useEffect, useId, useRef, useState, useTransition } from "react";
import { BidiText, FieldMessage, LtrValue, MoneyText, Stack, Text } from "@/components/ui";
import {
  acceptPublicFlightOfferAction,
  readPublicFlightBookingAction,
  requestPublicFlightBookingCancellationAction,
  requestPublicFlightReservationAction,
} from "@/features/flight-booking/actions";
import { getPublicFlightBookingCopy } from "@/features/flight-booking/copy";
import {
  flightBookingAccessStorageKey,
  type PublicFlightBookingReadResult,
} from "@/features/flight-booking/types";
import type { AppLocale } from "@/lib/i18n";

export function PublicFlightBookingStatusView({
  locale,
  flightBookingId,
}: {
  locale: AppLocale;
  flightBookingId: string;
}) {
  const copy = getPublicFlightBookingCopy(locale);
  const statusId = useId();
  const [data, setData] = useState<PublicFlightBookingReadResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();
  const offerKeyRef = useRef<string | null>(null);
  const reservationKeyRef = useRef<string | null>(null);
  const cancelKeyRef = useRef<string | null>(null);

  function token(): string | null {
    return typeof sessionStorage === "undefined"
      ? null
      : sessionStorage.getItem(flightBookingAccessStorageKey(flightBookingId));
  }

  useEffect(() => {
    const accessToken =
      typeof sessionStorage === "undefined"
        ? null
        : sessionStorage.getItem(flightBookingAccessStorageKey(flightBookingId));
    void readPublicFlightBookingAction(flightBookingId, accessToken).then((result) => {
      if (!result.ok) {
        setError(result.message);
        return;
      }
      setData(result.data);
    });
  }, [flightBookingId]);

  function run(
    action: (input: {
      flightBookingId: string;
      accessToken: string | null;
      idempotencyKey: string;
    }) => Promise<
      | { ok: true; data: PublicFlightBookingReadResult }
      | { ok: false; message: string; status?: number }
    >,
    keyRef: { current: string | null },
  ) {
    startTransition(() => {
      void (async () => {
        if (!keyRef.current) {
          keyRef.current = crypto.randomUUID();
        }
        const result = await action({
          flightBookingId,
          accessToken: token(),
          idempotencyKey: keyRef.current,
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
      <Stack gap="sm">
        <Text as="h2" role="heading">
          {copy.itineraryHeading}
        </Text>
        {data.journeys.map((journey) => (
          <ul key={journey.journeyId ?? journey.ordinal} className="list-inside list-disc">
            {journey.segments.map((segment) => (
              <li key={segment.segmentId ?? `${journey.ordinal}-${segment.ordinal}`}>
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
      </Stack>
      {data.offer ? (
        <Text>
          {copy.monetaryLabel}:{" "}
          <MoneyText
            money={{
              amount: String(data.offer.totalAmount),
              currencyCode: data.offer.currencyCode,
            }}
            locale={locale}
          />
        </Text>
      ) : null}
      {data.offer?.offerExpiresAt ? (
        <Text>
          {copy.offerExpiry}: <LtrValue>{data.offer.offerExpiresAt}</LtrValue>
        </Text>
      ) : null}
      {data.offer?.ticketingDeadline ? (
        <Text>
          {copy.ticketingDeadline}: <LtrValue>{data.offer.ticketingDeadline}</LtrValue>
        </Text>
      ) : null}
      {data.offer?.fareRules ? (
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.fareRulesHeading}
          </Text>
          <Text>
            {copy.refundable}: {data.offer.fareRules.refundable ? "yes" : "no"} · {copy.changeable}:{" "}
            {data.offer.fareRules.changeable ? "yes" : "no"}
          </Text>
          {data.offer.fareRules.baggage.length ? (
            <ul className="list-inside list-disc">
              {data.offer.fareRules.baggage.map((bag, index) => (
                <li key={`${bag.category}-${index}`}>
                  {copy.baggageHeading}: {bag.quantity ?? "-"} {bag.unit} {bag.weight ?? ""}
                </li>
              ))}
            </ul>
          ) : null}
        </Stack>
      ) : null}
      {data.reservation ? (
        <Text>
          {copy.reservationLabel}: <LtrValue>{data.reservation.presentationStatus}</LtrValue>
          {data.reservation.reservationLocator ? (
            <>
              {" "}
              · <LtrValue>{data.reservation.reservationLocator}</LtrValue>
            </>
          ) : null}
        </Text>
      ) : null}
      {data.tickets.length ? (
        <Stack gap="sm">
          <Text as="h2" role="heading">
            {copy.ticketsHeading}
          </Text>
          <ul className="list-inside list-disc">
            {data.tickets.map((ticket) => (
              <li key={ticket.passengerId}>
                {ticket.status === "Issued" ? copy.ticketIssued : copy.ticketPending}
                {ticket.ticketNumber ? (
                  <>
                    {" "}
                    · {copy.ticketNumber} <LtrValue>{ticket.ticketNumber}</LtrValue>
                  </>
                ) : null}
              </li>
            ))}
          </ul>
        </Stack>
      ) : null}
      {data.passengers.map((passenger) => (
        <Text key={passenger.passengerId}>
          <BidiText>
            {passenger.givenName} {passenger.familyName}
          </BidiText>{" "}
          · <LtrValue>{passenger.category}</LtrValue>
        </Text>
      ))}
      {error ? (
        <FieldMessage id={`${statusId}-error`} tone="error">
          {error}
        </FieldMessage>
      ) : null}
      {data.presentationState === "NeedsOffer" ? (
        <button
          type="button"
          className="min-h-11 rounded-md border px-4 py-2 focus-visible:outline"
          disabled={pending}
          onClick={() => run(acceptPublicFlightOfferAction, offerKeyRef)}
        >
          {pending ? copy.submitting : copy.acceptOffer}
        </button>
      ) : null}
      {data.presentationState === "OfferAccepted" ? (
        <button
          type="button"
          className="min-h-11 rounded-md border px-4 py-2 focus-visible:outline"
          disabled={pending}
          onClick={() => run(requestPublicFlightReservationAction, reservationKeyRef)}
        >
          {pending ? copy.submitting : copy.requestReservation}
        </button>
      ) : null}
      <a
        className="underline"
        href={`/${locale}/flight-bookings/${encodeURIComponent(flightBookingId)}/payment`}
      >
        {copy.payTitle}
      </a>
      {data.cancellationAvailable ? (
        <button
          type="button"
          className="min-h-11 rounded-md border px-4 py-2 focus-visible:outline"
          disabled={pending}
          onClick={() => run(requestPublicFlightBookingCancellationAction, cancelKeyRef)}
        >
          {pending ? copy.submitting : copy.cancelAction}
        </button>
      ) : null}
    </Stack>
  );
}
