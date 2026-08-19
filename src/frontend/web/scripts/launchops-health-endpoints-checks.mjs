/**
 * LAUNCHOPS-T003 — Backend health endpoints posture.
 */
import assert from "node:assert/strict";
import { existsRepo, readRepo } from "./launchops-common.mjs";

assert.ok(existsRepo("src/backend/Platform/Health/TravelCore.Health/README.md"));
assert.ok(existsRepo("src/backend/TravelCore.Api/Program.cs"));

const program = readRepo("src/backend/TravelCore.Api/Program.cs");
assert.match(program, /MapTravelCoreHealth/);
assert.match(program, /AddTravelCoreHealth/);

console.log("launchops-health-endpoints-checks: PASS");
