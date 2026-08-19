/**
 * PRODDEL-T009 — Transactional surfaces remain noindex.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const TRANSACTIONAL = [
  ["bookings", "[bookingId]"],
  ["bookings", "[bookingId]", "payment"],
  ["hotel-bookings", "[hotelBookingId]"],
  ["flight-bookings", "[flightBookingId]"],
  ["tours", "[slug]", "book"],
];

for (const segments of TRANSACTIONAL) {
  const src = read(pagePath(segments));
  assert.match(
    src,
    /robots:\s*\{\s*index:\s*false|index:\s*false,\s*follow/,
    `transactional noindex: ${segments.join("/")}`,
  );
}

console.log("proddel-transactional-noindex-checks: PASS");
