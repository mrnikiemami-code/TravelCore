"use client";

import { useEffect, useState } from "react";
import { BidiText, LtrValue, MoneyText, Stack, Surface, Text } from "@/components/ui";
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

/**
 * Public Pending booking status (TC-P34-T004).
 * Links to payment only when public payment read says initiation is available
 * (safeAction Initiate/Retry). Otherwise keeps I4 Option A honest stop.
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
  const [payment, setPayment] = useState<PublicBookingPaymentReadResult | null>(null);
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
    return <Text>{copy.unauthorized}</Text>;
  }

  if (!data) {
    return <Text role="muted">{copy.submitting}</Text>;
  }

  const sandboxAvailable =
    payment != null &&
    (payment.safeAction === "Initiate" || payment.safeAction === "Retry");

  return (
    <Stack gap="md">
      <Text as="h1" role="heading">
        {copy.statusTitle}
      </Text>
      <Text>{copy.pendingNote}</Text>
      <Text role="caption">
        {data.status !== "Confirmed" ? (
          <>
            {copy.notConfirmed} ·{" "}
          </>
        ) : null}
        <LtrValue>{data.status}</LtrValue>
        {payment ? (
          <>
            {" "}
            · <LtrValue>{payment.paymentStatus}</LtrValue>
            {payment.safeAction ? (
              <>
                {" "}
                · <LtrValue>{payment.safeAction}</LtrValue>
              </>
            ) : null}
          </>
        ) : null}
      </Text>
      {data.monetary ? (
        <Text>
          {copy.monetaryLabel}:{" "}
          <MoneyText
            money={{
              amount: String(data.monetary.totalAmount),
              currencyCode: data.monetary.currency,
            }}
            locale={locale}
          />
        </Text>
      ) : null}
      {data.hold ? (
        <Text>
          {copy.holdLabel}: <LtrValue>{String(data.hold.seatCount)}</LtrValue> ·{" "}
          <LtrValue>{data.hold.status}</LtrValue>
        </Text>
      ) : null}
      <ul className="list-inside list-disc">
        {data.passengers.map((passenger) => (
          <li key={passenger.passengerId}>
            <BidiText>
              {passenger.givenName} {passenger.familyName}
            </BidiText>{" "}
            · <LtrValue>{passenger.category}</LtrValue>
          </li>
        ))}
      </ul>
      {sandboxAvailable ? (
        <Stack gap="sm">
          <Text role="caption">{copy.paySandboxNote}</Text>
          <a
            className="inline-flex min-h-11 items-center rounded-md border border-amber-700/40 bg-amber-50 px-4 py-2 text-amber-950 underline-offset-2 hover:underline focus-visible:outline dark:bg-amber-950/30 dark:text-amber-100"
            href={`/${locale}/bookings/${encodeURIComponent(bookingId)}/payment`}
          >
            {copy.payGoToSandbox}
          </a>
        </Stack>
      ) : paymentChecked ? (
        <Surface className="border-primary/15 bg-gradient-to-br from-surface to-primary/5">
          <Stack gap="sm">
            <Text as="h2" role="heading" className="text-primary">
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
