"use client";

import { useEffect, useId, useRef, useState, useTransition } from "react";
import { FieldMessage, LtrValue, MoneyText, Stack, Text } from "@/components/ui";
import {
  initiatePublicHotelBookingPaymentAction,
  readPublicHotelBookingPaymentAction,
} from "@/features/hotel-booking/actions";
import { getPublicHotelBookingCopy } from "@/features/hotel-booking/copy";
import {
  hotelBookingAccessStorageKey,
  type PublicHotelBookingPaymentReadResult,
} from "@/features/hotel-booking/types";
import type { AppLocale } from "@/lib/i18n";

export function PublicHotelBookingPaymentView({
  locale,
  hotelBookingId,
  returnedFromProvider,
}: {
  locale: AppLocale;
  hotelBookingId: string;
  returnedFromProvider?: boolean;
}) {
  const copy = getPublicHotelBookingCopy(locale);
  const statusId = useId();
  const [data, setData] = useState<PublicHotelBookingPaymentReadResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();
  const idempotencyKeyRef = useRef<string | null>(null);

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
    void readPublicHotelBookingPaymentAction(hotelBookingId, accessToken).then((result) => {
      if (!result.ok) {
        setError(result.message);
        return;
      }
      setData(result.data);
    });
  }, [hotelBookingId]);

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

        const result = await initiatePublicHotelBookingPaymentAction({
          hotelBookingId,
          accessToken: token(),
          idempotencyKey: idempotencyKeyRef.current,
        });
        if (!result.ok) {
          setError(result.message);
          const refreshed = await readPublicHotelBookingPaymentAction(hotelBookingId, token());
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
      : data.safeAction === "Succeeded" && !data.hotelBookingConfirmed
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
        {copy.notConfirmed} · <LtrValue>{data.hotelBookingStatus}</LtrValue> ·{" "}
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
