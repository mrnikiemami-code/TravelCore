/**
 * MODOPS-T012 — Approved ≠ Published lifecycle guard (approve before publish).
 */
import assert from "node:assert/strict";
import { readRepo } from "./modops-common.mjs";

const tests = readRepo("tests/Unit/TravelCore.Modules.Ugc.UnitTests/UgcModerationServiceTests.cs");
assert.match(tests, /Approve_Then_Publish/);
assert.match(tests, /Publish_Before_Approve_Throws/);
assert.match(tests, /"Approved"/);
assert.match(tests, /"Published"/);

console.log("modops-lifecycle-guard-checks: PASS");
