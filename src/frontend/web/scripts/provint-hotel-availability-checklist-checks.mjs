/**
 * PROVINT-T003 — Hotel availability source adapter checklist.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, readRepo } from "./provint-common.mjs";

const checklist = path.join(repoRoot, "docs/plans/P21-hotel-source-adapter-checklist.md");
assert.ok(fs.existsSync(checklist));
assert.match(fs.readFileSync(checklist, "utf8"), /IHotelAvailabilitySource/);
assert.match(fs.readFileSync(checklist, "utf8"), /AvailabilityCheck/);

assert.match(readRepo(
  "src/backend/Modules/HotelBooking/TravelCore.Modules.HotelBooking.Contracts/IHotelAvailabilitySource.cs",
), /interface IHotelAvailabilitySource/);

console.log("provint-hotel-availability-checklist-checks: PASS");
