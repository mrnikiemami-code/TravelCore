"use client";

import { useEffect, useState } from "react";
import { BidiText, LtrValue, MoneyText, Stack, Surface, Text } from "@/components/ui";
import { readPublicBookingAction } from "@/features/booking/actions";
import { getPublicBookingCopy } from "@/features/booking/copy";
import {
  bookingAccessStorageKey,
  type PublicBookingReadResult,
} from "@/features/booking/types";
import type { AppLocale } from "@/lib/i18n";

/**
 * Public Pending booking status + I4 Option A payment boundary (TC-P33-T008).
 * Preserves Pending. Does not imply a live payment provider is ready.
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
  }, [bookingId]);

  if (error) {
    return <Text>{copy.unauthorized}</Text>;
  }

  if (!data) {
    return <Text role="muted">{copy.submitting}</Text>;
  }

  return (
    <Stack gap="md">
      <Text as="h1" role="heading">
        {copy.statusTitle}
      </Text>
      <Text>{copy.pendingNote}</Text>
      <Text role="caption">
        {copy.notConfirmed} · <LtrValue>{data.status}</LtrValue>
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
    </Stack>
  );
}
