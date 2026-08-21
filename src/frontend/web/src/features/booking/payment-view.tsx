"use client";

import { Stack, Surface, Text } from "@/components/ui";
import { getPublicBookingCopy } from "@/features/booking/copy";
import type { AppLocale } from "@/lib/i18n";

/**
 * I4 Option A — honest Payment boundary (TC-P33-T008).
 * Does not initiate provider redirects, invent success, or Confirm bookings.
 * Payment module ownership is preserved; this surface stops without money theater.
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
          <Text role="caption">{copy.notConfirmed}</Text>
        </Stack>
      </Surface>
    </Stack>
  );
}
