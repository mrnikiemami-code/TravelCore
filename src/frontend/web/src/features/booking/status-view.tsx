"use client";

import { useEffect, useState } from "react";
import { LtrValue, Stack, Text } from "@/components/ui";
import { readPublicBookingAction } from "@/features/booking/actions";
import { getPublicBookingCopy } from "@/features/booking/copy";
import {
  bookingAccessStorageKey,
  type PublicBookingReadResult,
} from "@/features/booking/types";
import type { AppLocale } from "@/lib/i18n";

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
          {copy.monetaryLabel}: {data.monetary.totalAmount} {data.monetary.currency}
        </Text>
      ) : null}
      {data.hold ? (
        <Text>
          {copy.holdLabel}: {data.hold.seatCount} · {data.hold.status}
        </Text>
      ) : null}
      <ul className="list-inside list-disc">
        {data.passengers.map((passenger) => (
          <li key={passenger.passengerId}>
            {passenger.givenName} {passenger.familyName} · {passenger.category}
          </li>
        ))}
      </ul>
    </Stack>
  );
}
