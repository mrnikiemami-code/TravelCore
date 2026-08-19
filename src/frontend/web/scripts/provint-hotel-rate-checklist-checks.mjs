/**
 * PROVINT-T004 — Hotel rate source adapter checklist.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, readRepo } from "./provint-common.mjs";

const checklist = path.join(repoRoot, "docs/plans/P21-hotel-source-adapter-checklist.md");
assert.match(fs.readFileSync(checklist, "utf8"), /IHotelRateOfferSource/);
assert.match(fs.readFileSync(checklist, "utf8"), /RateQuote/);

assert.match(readRepo(
  "src/backend/Modules/HotelBooking/TravelCore.Modules.HotelBooking.Contracts/IHotelRateOfferSource.cs",
), /interface IHotelRateOfferSource/);

console.log("provint-hotel-rate-checklist-checks: PASS");
