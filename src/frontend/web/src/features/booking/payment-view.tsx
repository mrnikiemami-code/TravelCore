"use client";

import { useEffect, useRef, useState, useTransition } from "react";
import { LtrValue, MoneyText, Stack, Text } from "@/components/ui";
import {
  initiatePublicBookingPaymentAction,
  readPublicBookingPaymentAction,
} from "@/features/booking/actions";
import { getPublicBookingCopy } from "@/features/booking/copy";
import {
  bookingAccessStorageKey,
  type PublicBookingPaymentReadResult,
} from "@/features/booking/types";
import type { AppLocale } from "@/lib/i18n";

export function PublicBookingPaymentView({
  locale,
  bookingId,
  returnedFromProvider,
}: {
  locale: AppLocale;
  bookingId: string;
  returnedFromProvider?: boolean;
}) {
  const copy = getPublicBookingCopy(locale);
  const [data, setData] = useState<PublicBookingPaymentReadResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();
  const idempotencyKeyRef = useRef<string | null>(null);

  function token(): string | null {
    return typeof sessionStorage === "undefined"
      ? null
      : sessionStorage.getItem(bookingAccessStorageKey(bookingId));
  }

  useEffect(() => {
    const accessToken =
      typeof sessionStorage === "undefined"
        ? null
        : sessionStorage.getItem(bookingAccessStorageKey(bookingId));
    void readPublicBookingPaymentAction(bookingId, accessToken).then((result) => {
      if (!result.ok) {
        setError(result.message);
        return;
      }
      setData(result.data);
    });
  }, [bookingId]);

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

        const result = await initiatePublicBookingPaymentAction({
          bookingId,
          accessToken: token(),
          idempotencyKey: idempotencyKeyRef.current,
        });
        if (!result.ok) {
          setError(result.message);
          const refreshed = await readPublicBookingPaymentAction(bookingId, token());
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
    return <Text role="muted">{copy.submitting}</Text>;
  }

  const statusMessage =
    data.safeAction === "Wait"
      ? copy.payWaiting
      : data.safeAction === "Succeeded" && !data.bookingConfirmed
        ? copy.payReceivedPendingConfirm
        : data.safeAction === "CompensationPending" || data.safeAction === "RefundSucceeded"
          ? copy.payCompensation
          : copy.payUnavailable;

  const canInitiate = data.safeAction === "Initiate" || data.safeAction === "Retry";

  return (
    <Stack gap="md">
      <Text as="h1" role="heading">
        {copy.payTitle}
      </Text>
      {returnedFromProvider ? <Text>{copy.payReturned}</Text> : <Text>{copy.payNote}</Text>}
      <Text role="caption">
        {copy.notConfirmed} · <LtrValue>{data.bookingStatus}</LtrValue> ·{" "}
        <LtrValue>{data.paymentStatus}</LtrValue>
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
      <Text>{statusMessage}</Text>
      {error ? <Text>{error}</Text> : null}
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
