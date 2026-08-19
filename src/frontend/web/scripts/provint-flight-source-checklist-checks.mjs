/**
 * PROVINT-T006 — Flight source adapter checklist.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, readRepo } from "./provint-common.mjs";

const checklist = path.join(repoRoot, "docs/plans/P22-flight-source-adapter-checklist.md");
assert.ok(fs.existsSync(checklist));
assert.match(fs.readFileSync(checklist, "utf8"), /IFlightSearchSource/);
assert.match(fs.readFileSync(checklist, "utf8"), /IFlightCancellationSource/);

assert.match(readRepo(
  "src/backend/Modules/Flight/TravelCore.Modules.Flight.Contracts/FlightOwnershipBoundary.cs",
), /IFlightSearchSource/);

console.log("provint-flight-source-checklist-checks: PASS");
