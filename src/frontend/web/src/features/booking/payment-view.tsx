"use client";

import { useEffect, useRef, useState, useTransition } from "react";
import { MoneyText, Stack, Surface, Text } from "@/components/ui";
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

function bookingStatusLabel(locale: AppLocale, status: string): string {
  if (locale === "fa") {
    if (status === "Pending") return "رزرو موقت";
    if (status === "Confirmed") return "تأییدشده";
    return status;
  }
  if (locale === "ar") {
    if (status === "Pending") return "حجز مؤقت";
    if (status === "Confirmed") return "مؤكد";
    return status;
  }
  if (status === "Pending") return "Pending";
  if (status === "Confirmed") return "Confirmed";
  return status;
}

function paymentStatusLabel(locale: AppLocale, status: string): string {
  if (locale === "fa") {
    if (status === "None" || status === "Unavailable") return "پرداخت فعال نیست";
    if (status === "Initiated") return "پرداخت آغاز شده";
    if (status === "Succeeded") return "پرداخت دریافت شد";
    return status;
  }
  if (locale === "ar") {
    if (status === "None" || status === "Unavailable") return "الدفع غير مفعّل";
    if (status === "Initiated") return "بدأ الدفع";
    if (status === "Succeeded") return "تم استلام الدفع";
    return status;
  }
  if (status === "None" || status === "Unavailable") return "Payment not active";
  if (status === "Initiated") return "Payment started";
  if (status === "Succeeded") return "Payment received";
  return status;
}

/**
 * Tour public payment surface (TC-P36-T005 commerce polish).
 * Browser return ≠ success; UI shows only server payment/booking truth.
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
    void readPublicBookingPaymentAction(bookingId, accessToken).then(
      (result) => {
        if (!result.ok) {
          setError(result.message);
          return;
        }
        setData(result.data);
      },
    );
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
          const refreshed = await readPublicBookingPaymentAction(
            bookingId,
            token(),
          );
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
    return (
      <div className="rounded-2xl border border-border bg-surface p-6">
        <Text>{copy.unauthorized}</Text>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="rounded-2xl border border-border bg-surface p-6">
        <Text role="muted">{copy.submitting}</Text>
      </div>
    );
  }

  const canInitiate =
    data.safeAction === "Initiate" || data.safeAction === "Retry";

  const statusMessage =
    data.safeAction === "Wait"
      ? copy.payWaiting
      : data.safeAction === "Succeeded" && data.bookingStatus !== "Confirmed"
        ? copy.payReceivedPendingConfirm
        : data.safeAction === "CompensationPending" ||
            data.safeAction === "RefundSucceeded"
          ? copy.payCompensation
          : canInitiate
            ? copy.paySandboxNote
            : data.safeAction === "Succeeded"
              ? copy.payNote
              : copy.payUnavailable;

  if (!canInitiate && data.safeAction === "Unavailable") {
    return (
      <Stack gap="md">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#1D4ED8]">
            {locale === "fa" ? "پرداخت" : locale === "ar" ? "الدفع" : "Payment"}
          </p>
          <Text as="h1" role="heading" className="mt-2 text-2xl font-semibold">
            {copy.paymentBoundaryTitle}
          </Text>
        </div>
        {returnedFromProvider ? <Text>{copy.payReturned}</Text> : null}
        <Surface className="rounded-2xl border-[#1D4ED8]/15 bg-gradient-to-br from-surface to-[#1D4ED8]/[0.04] p-5">
          <Stack gap="sm">
            <Text>{copy.paymentBoundaryBody}</Text>
            <Text role="caption">{copy.paymentBoundaryNote}</Text>
            <Text role="muted">{copy.payUnavailable}</Text>
            <div className="mt-2 flex flex-wrap gap-2">
              <span className="rounded-full bg-[#1D4ED8]/10 px-3 py-1 text-xs font-semibold text-[#1D4ED8]">
                {bookingStatusLabel(locale, data.bookingStatus)}
              </span>
              <span className="rounded-full bg-muted px-3 py-1 text-xs font-medium text-foreground">
                {paymentStatusLabel(locale, data.paymentStatus)}
              </span>
            </div>
          </Stack>
        </Surface>
      </Stack>
    );
  }

  return (
    <Stack gap="md">
      <div>
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#1D4ED8]">
          {locale === "fa" ? "پرداخت" : locale === "ar" ? "الدفع" : "Payment"}
        </p>
        <Text as="h1" role="heading" className="mt-2 text-2xl font-semibold">
          {copy.payTitle}
        </Text>
        <p className="mt-2 text-sm text-muted-foreground">
          {returnedFromProvider ? copy.payReturned : copy.payNote}
        </p>
      </div>

      <div className="flex flex-wrap gap-2">
        <span className="rounded-full bg-[#1D4ED8]/10 px-3 py-1 text-xs font-semibold text-[#1D4ED8]">
          {bookingStatusLabel(locale, data.bookingStatus)}
        </span>
        <span className="rounded-full bg-muted px-3 py-1 text-xs font-medium text-foreground">
          {paymentStatusLabel(locale, data.paymentStatus)}
        </span>
        {data.bookingStatus !== "Confirmed" ? (
          <span className="rounded-full bg-[#F59E0B]/15 px-3 py-1 text-xs font-semibold text-[#92400E]">
            {copy.notConfirmed}
          </span>
        ) : null}
      </div>

      {data.amount != null && data.currencyCode ? (
        <Surface className="rounded-2xl p-5">
          <p className="text-xs font-medium text-muted-foreground">
            {copy.monetaryLabel}
          </p>
          <MoneyText
            money={{
              amount: String(data.amount),
              currencyCode: data.currencyCode,
            }}
            locale={locale}
            className="mt-2 text-2xl font-semibold"
          />
        </Surface>
      ) : null}

      <Text>{statusMessage}</Text>
      {error ? <Text>{error}</Text> : null}
      {canInitiate ? (
        <button
          type="button"
          className="min-h-touch rounded-lg border border-amber-700/40 bg-[#F59E0B] px-4 py-3 text-sm font-semibold text-[#0E172A] focus-visible:outline"
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
