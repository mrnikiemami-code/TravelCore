"use client";

import { useEffect, useId, useRef, useState, useTransition } from "react";
import { FieldMessage, LtrValue, MoneyText, Stack, Text } from "@/components/ui";
import {
  initiatePublicFlightBookingPaymentAction,
  readPublicFlightBookingPaymentAction,
} from "@/features/flight-booking/actions";
import { getPublicFlightBookingCopy } from "@/features/flight-booking/copy";
import {
  flightBookingAccessStorageKey,
  type PublicFlightBookingPaymentReadResult,
} from "@/features/flight-booking/types";
import type { AppLocale } from "@/lib/i18n";

export function PublicFlightBookingPaymentView({
  locale,
  flightBookingId,
  returnedFromProvider,
}: {
  locale: AppLocale;
  flightBookingId: string;
  returnedFromProvider?: boolean;
}) {
  const copy = getPublicFlightBookingCopy(locale);
  const statusId = useId();
  const [data, setData] = useState<PublicFlightBookingPaymentReadResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();
  const idempotencyKeyRef = useRef<string | null>(null);

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
    void readPublicFlightBookingPaymentAction(flightBookingId, accessToken).then((result) => {
      if (!result.ok) {
        setError(result.message);
        return;
      }
      setData(result.data);
    });
  }, [flightBookingId]);

  function pay() {
    if (pending) {
      return;
    }

    setError(null);
    startTransition(() => {
      void (async () => {
        if (!idempotencyKeyRef.current) {
          idempotencyKeyRef.current = crypto.randomUUID();
        }

        const result = await initiatePublicFlightBookingPaymentAction({
          flightBookingId,
          accessToken: token(),
          idempotencyKey: idempotencyKeyRef.current,
        });
        if (!result.ok) {
          setError(result.message);
          const refreshed = await readPublicFlightBookingPaymentAction(flightBookingId, token());
          if (refreshed.ok) {
            setData(refreshed.data);
          }
          return;
        }

        setData(result.data);
        if (result.data.redirectUri) {
          window.location.assign(result.data.redirectUri);
        }
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

  const statusMessage =
    data.safeAction === "Wait"
      ? copy.payWaiting
      : data.safeAction === "Succeeded" && !data.flightBookingConfirmed
        ? copy.payReceivedPendingConfirm
        : data.safeAction === "CompensationPending"
          ? copy.payCompensation
          : data.safeAction === "RefundSucceeded"
            ? copy.refundSucceeded
            : copy.payUnavailable;

  const canInitiate = data.safeAction === "Initiate" || data.safeAction === "Retry";

  return (
    <Stack gap="md">
      <Text as="h1" role="heading">
        {copy.payTitle}
      </Text>
      {returnedFromProvider ? <Text>{copy.payReturned}</Text> : <Text>{copy.payNote}</Text>}
      <p className="text-body text-foreground" aria-live="polite">
        {statusMessage}
      </p>
      <Text role="caption">
        {copy.notConfirmed} · <LtrValue>{data.flightBookingStatus}</LtrValue> ·{" "}
        <LtrValue>{data.paymentStatus ?? "Pending"}</LtrValue>
      </Text>
      {data.amount != null && data.currencyCode ? (
        <Text>
          {copy.monetaryLabel}:{" "}
          <MoneyText
            money={{
              amount: String(data.amount),
              currencyCode: data.currencyCode,
            }}
            locale={locale}
          />
        </Text>
      ) : null}
      {error ? (
        <FieldMessage id={`${statusId}-error`} tone="error">
          {error}
        </FieldMessage>
      ) : null}
      {canInitiate ? (
        <button
          type="button"
          className="min-h-11 rounded-md border px-4 py-2 focus-visible:outline"
          disabled={pending}
          onClick={pay}
        >
          {pending ? copy.submitting : data.safeAction === "Retry" ? copy.payRetry : copy.payAction}
        </button>
      ) : null}
    </Stack>
  );
}
