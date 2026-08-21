"use client";

import { useEffect, useState } from "react";
import {
  BidiText,
  LtrValue,
  MoneyText,
  Stack,
  Surface,
  Text,
} from "@/components/ui";
import {
  readPublicBookingAction,
  readPublicBookingPaymentAction,
} from "@/features/booking/actions";
import { getPublicBookingCopy } from "@/features/booking/copy";
import {
  bookingAccessStorageKey,
  type PublicBookingPaymentReadResult,
  type PublicBookingReadResult,
} from "@/features/booking/types";
import type { AppLocale } from "@/lib/i18n";

function bookingStatusLabel(locale: AppLocale, status: string): string {
  if (locale === "fa") {
    if (status === "Pending") return "رزرو موقت (Pending)";
    if (status === "Confirmed") return "تأییدشده";
    return status;
  }
  if (locale === "ar") {
    if (status === "Pending") return "حجز مؤقت (Pending)";
    if (status === "Confirmed") return "مؤكد";
    return status;
  }
  if (status === "Pending") return "Pending booking";
  if (status === "Confirmed") return "Confirmed";
  return status;
}

/**
 * Public Pending booking status (TC-P36-T005 commerce polish).
 * Links to payment only when public payment read says initiation is available.
 */
export function PublicBookingStatusView({
  locale,
  bookingId,
}: {
  locale: AppLocale;
  bookingId: string;
}) {
  const copy = getPublicBookingCopy(locale);
  const [data, setData] = useState<PublicBookingReadResult | null>(null);
  const [payment, setPayment] = useState<PublicBookingPaymentReadResult | null>(
    null,
  );
  const [paymentChecked, setPaymentChecked] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const token =
      typeof sessionStorage === "undefined"
        ? null
        : sessionStorage.getItem(bookingAccessStorageKey(bookingId));
    void readPublicBookingAction(bookingId, token).then((result) => {
      if (!result.ok) {
        setError(result.message);
        return;
      }
      setData(result.data);
    });
    void readPublicBookingPaymentAction(bookingId, token).then((result) => {
      setPaymentChecked(true);
      if (result.ok) {
        setPayment(result.data);
      }
    });
  }, [bookingId]);

  if (error) {
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

  const sandboxAvailable =
    payment != null &&
    (payment.safeAction === "Initiate" || payment.safeAction === "Retry");

  return (
    <Stack gap="md">
      <div>
        <p className="text-xs font-semibold uppercase tracking-[0.16em] text-[#1D4ED8]">
          {locale === "fa" ? "رزرو" : locale === "ar" ? "الحجز" : "Booking"}
        </p>
        <Text as="h1" role="heading" className="mt-2 text-2xl font-semibold">
          {copy.statusTitle}
        </Text>
        <p className="mt-2 text-sm text-muted-foreground">{copy.pendingNote}</p>
      </div>

      <div className="flex flex-wrap gap-2">
        <span className="rounded-full bg-[#1D4ED8]/10 px-3 py-1 text-xs font-semibold text-[#1D4ED8]">
          {bookingStatusLabel(locale, data.status)}
        </span>
        {data.status !== "Confirmed" ? (
          <span className="rounded-full bg-[#F59E0B]/15 px-3 py-1 text-xs font-semibold text-[#92400E]">
            {copy.notConfirmed}
          </span>
        ) : null}
      </div>

      {data.monetary ? (
        <Surface className="rounded-2xl p-5">
          <p className="text-xs font-medium text-muted-foreground">
            {copy.monetaryLabel}
          </p>
          <MoneyText
            money={{
              amount: String(data.monetary.totalAmount),
              currencyCode: data.monetary.currency,
            }}
            locale={locale}
            className="mt-2 text-2xl font-semibold"
          />
        </Surface>
      ) : null}

      {data.hold ? (
        <Text role="caption">
          {copy.holdLabel}: <LtrValue>{String(data.hold.seatCount)}</LtrValue>
        </Text>
      ) : null}

      <Surface className="rounded-2xl p-5">
        <p className="text-sm font-semibold text-foreground">
          {locale === "fa"
            ? "مسافران"
            : locale === "ar"
              ? "المسافرون"
              : "Travelers"}
        </p>
        <ul className="mt-3 space-y-2 text-sm">
          {data.passengers.map((passenger) => (
            <li
              key={passenger.passengerId}
              className="rounded-lg border border-border bg-background px-3 py-2"
            >
              <BidiText>
                {passenger.givenName} {passenger.familyName}
              </BidiText>{" "}
              · <LtrValue>{passenger.category}</LtrValue>
            </li>
          ))}
        </ul>
      </Surface>

      {sandboxAvailable ? (
        <Surface className="rounded-2xl border-amber-700/30 bg-amber-50 p-5 dark:bg-amber-950/30">
          <Stack gap="sm">
            <Text role="caption">{copy.paySandboxNote}</Text>
            <a
              className="inline-flex min-h-touch items-center justify-center rounded-lg border border-amber-700/40 bg-[#F59E0B] px-4 py-2 text-sm font-semibold text-[#0E172A] hover:opacity-95"
              href={`/${locale}/bookings/${encodeURIComponent(bookingId)}/payment`}
            >
              {copy.payGoToSandbox}
            </a>
          </Stack>
        </Surface>
      ) : paymentChecked ? (
        <Surface className="rounded-2xl border-[#1D4ED8]/15 bg-gradient-to-br from-surface to-[#1D4ED8]/[0.04] p-5">
          <Stack gap="sm">
            <Text as="h2" role="heading" className="text-[#1D4ED8]">
              {copy.paymentBoundaryTitle}
            </Text>
            <Text>{copy.paymentBoundaryBody}</Text>
            <Text role="caption">{copy.paymentBoundaryNote}</Text>
            <Text role="muted">{copy.payUnavailable}</Text>
          </Stack>
        </Surface>
      ) : (
        <Text role="muted">{copy.submitting}</Text>
      )}
    </Stack>
  );
}
