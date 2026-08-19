/**
 * PROVINT-T001 — Payment provider readiness checklist.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, readRepo } from "./provint-common.mjs";

const checklist = path.join(repoRoot, "docs/plans/P20-provider-adapter-checklist.md");
assert.ok(fs.existsSync(checklist));
assert.match(fs.readFileSync(checklist, "utf8"), /READY FOR ADAPTERS/);

const boundary = readRepo(
  "src/backend/Modules/Payment/TravelCore.Modules.Payment.Contracts/PaymentProviderTrustBoundary.cs",
);
assert.match(boundary, /NamedProviderSelected = "NONE"/);
assert.match(boundary, /ProviderInfrastructurePosture = "READY FOR ADAPTERS"/);

console.log("provint-payment-readiness-checks: PASS");
