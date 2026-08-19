/**
 * PROVINT-T009 — Post-P29-R3 evolution provider boundary.
 */
import assert from "node:assert/strict";
import { readRepo } from "./provint-common.mjs";

const boundary = readRepo(
  "src/backend/Platform/Evolution/TravelCore.Evolution/EvolutionProviderExpansionBoundary.cs",
);
assert.match(boundary, /ProviderExpansionBoundaryImplemented = true/);
assert.match(boundary, /ProviderRegistryProductImplemented = false/);
assert.match(boundary, /NoGlobalProviderRegistryMegaTable/);

console.log("provint-evolution-boundary-checks: PASS");
