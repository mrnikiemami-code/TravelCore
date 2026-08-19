/**
 * PROVINT-T011 — Architecture vendor SDK / package ban guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, webRoot } from "./provint-common.mjs";

const archTest = fs.readFileSync(
  path.join(repoRoot, "tests/Architecture/TravelCore.ArchitectureTests/PaymentBoundaryGuardrailTests.cs"),
  "utf8",
);
assert.match(archTest, /NamedProductionAdapterImplemented/);

const pkg = JSON.parse(fs.readFileSync(path.join(webRoot, "package.json"), "utf8"));
const deps = JSON.stringify({ ...pkg.dependencies, ...pkg.devDependencies });
assert.doesNotMatch(deps, /stripe|adyen|amadeus|sabre|travelport/i);

console.log("provint-vendor-sdk-ban-checks: PASS");
