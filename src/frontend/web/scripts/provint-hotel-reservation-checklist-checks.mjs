/**
 * PROVINT-T005 — Hotel reservation source adapter checklist.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, readRepo } from "./provint-common.mjs";

const checklist = path.join(repoRoot, "docs/plans/P21-hotel-source-adapter-checklist.md");
assert.match(fs.readFileSync(checklist, "utf8"), /IHotelReservationSource/);
assert.match(fs.readFileSync(checklist, "utf8"), /ReservationCreate/);

assert.match(readRepo(
  "src/backend/Modules/HotelBooking/TravelCore.Modules.HotelBooking.Contracts/IHotelReservationSource.cs",
), /interface IHotelReservationSource/);

console.log("provint-hotel-reservation-checklist-checks: PASS");
