/**
 * LAUNCHOPS-T009 — Hardening / health boundary checks.
 */
import assert from "node:assert/strict";
import { existsRepo, readRepo } from "./launchops-common.mjs";

assert.ok(
  existsRepo(
    "src/backend/Platform/Hardening/TravelCore.Hardening/HardeningHealthObservabilityInteractionBoundary.cs",
  ),
);

const boundary = readRepo(
  "src/backend/Platform/Hardening/TravelCore.Hardening/HardeningHealthObservabilityInteractionBoundary.cs",
);
assert.match(boundary, /HealthOwnsMinimalOperationalChecks/);
assert.match(boundary, /HardeningDoesNotReplaceHealth/);

console.log("launchops-hardening-health-boundary-checks: PASS");
