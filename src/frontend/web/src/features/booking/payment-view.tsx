"use client";

import { useEffect, useRef, useState, useTransition } from "react";
import { LtrValue, MoneyText, Stack, Surface, Text } from "@/components/ui";
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

/**
 * Tour public payment surface (TC-P34-T004).
 * Restores read/initiate against real Payment APIs.
 * When initiation is available (sandbox eligible), CTA is labeled NON-PRODUCTION.
 * Browser return ≠ success; UI shows only server payment/booking truth.
 * When unavailable, keeps I4 Option A honest stop (no misleading pay CTA).
 */
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

  const canInitiate = data.safeAction === "Initiate" || data.safeAction === "Retry";

  const statusMessage =
    data.safeAction === "Wait"
      ? copy.payWaiting
      : data.safeAction === "Succeeded" && data.bookingStatus !== "Confirmed"
        ? copy.payReceivedPendingConfirm
        : data.safeAction === "CompensationPending" || data.safeAction === "RefundSucceeded"
          ? copy.payCompensation
          : canInitiate
            ? copy.paySandboxNote
            : data.safeAction === "Succeeded"
              ? copy.payNote
              : copy.payUnavailable;

  // Unavailable → Option A honest stop (no pay CTA theater).
  if (!canInitiate && data.safeAction === "Unavailable") {
    return (
      <Stack gap="md">
        <Text as="h1" role="heading">
          {copy.paymentBoundaryTitle}
        </Text>
        {returnedFromProvider ? <Text>{copy.payReturned}</Text> : null}
        <Surface className="border-primary/15 bg-gradient-to-br from-surface to-primary/5">
          <Stack gap="sm">
            <Text>{copy.paymentBoundaryBody}</Text>
            <Text role="caption">{copy.paymentBoundaryNote}</Text>
            <Text role="muted">{copy.payUnavailable}</Text>
            <Text role="caption">
              <span className="font-mono text-xs">{bookingId}</span>
            </Text>
            <Text role="caption">
              {copy.notConfirmed} · <LtrValue>{data.bookingStatus}</LtrValue> ·{" "}
              <LtrValue>{data.paymentStatus}</LtrValue>
            </Text>
          </Stack>
        </Surface>
      </Stack>
    );
  }

  return (
    <Stack gap="md">
      <Text as="h1" role="heading">
        {copy.payTitle}
      </Text>
      {returnedFromProvider ? <Text>{copy.payReturned}</Text> : <Text>{copy.payNote}</Text>}
      {canInitiate ? <Text role="caption">{copy.paySandboxNote}</Text> : null}
      <Text role="caption">
        {data.bookingStatus !== "Confirmed" ? (
          <>
            {copy.notConfirmed} ·{" "}
          </>
        ) : null}
        <LtrValue>{data.bookingStatus}</LtrValue> · <LtrValue>{data.paymentStatus}</LtrValue>
        {data.safeAction ? (
          <>
            {" "}
            · <LtrValue>{data.safeAction}</LtrValue>
          </>
        ) : null}
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
          className="min-h-11 rounded-md border border-amber-700/40 bg-amber-50 px-4 py-2 text-amber-950 focus-visible:outline dark:bg-amber-950/30 dark:text-amber-100"
          disabled={pending}
          onClick={pay}
          aria-label={copy.paySandboxAction}
        >
          {pending
            ? copy.submitting
            : data.safeAction === "Retry"
              ? `${copy.payRetry} · ${copy.paySandboxAction}`
              : copy.paySandboxAction}
        </button>
      ) : null}
    </Stack>
  );
}
