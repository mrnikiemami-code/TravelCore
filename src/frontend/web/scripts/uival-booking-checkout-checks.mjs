/** UIVAL-T011 Booking/Checkout checks */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const srcRoot = path.join(path.resolve(path.dirname(fileURLToPath(import.meta.url)), ".."), "src");
const read = (f) => fs.readFileSync(f, "utf8");

const dev = path.join(srcRoot, "app", "[locale]", "dev", "booking-checkout", "page.tsx");
assert.ok(fs.existsSync(dev));
assert.match(read(dev), /PublicBookingPrepareForm/);
assert.match(read(path.join(srcRoot, "app", "[locale]", "bookings", "[bookingId]", "page.tsx")), /PublicBookingStatusView/);
assert.match(read(path.join(srcRoot, "features", "booking", "prepare-form.tsx")).slice(0, 30), /use client/);
console.log("uival-booking-checkout-checks: PASS");
