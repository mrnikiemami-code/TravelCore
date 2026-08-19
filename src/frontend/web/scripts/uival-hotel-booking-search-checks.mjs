/** UIVAL-T013 Hotel Booking Search checks */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const srcRoot = path.join(path.resolve(path.dirname(fileURLToPath(import.meta.url)), ".."), "src");
const read = (f) => fs.readFileSync(f, "utf8");

assert.ok(fs.existsSync(path.join(srcRoot, "app", "[locale]", "dev", "hotel-booking-search", "page.tsx")));
assert.match(read(path.join(srcRoot, "app", "[locale]", "places", "[slug]", "book", "page.tsx")), /PublicHotelBookingPrepareForm/);
console.log("uival-hotel-booking-search-checks: PASS");
