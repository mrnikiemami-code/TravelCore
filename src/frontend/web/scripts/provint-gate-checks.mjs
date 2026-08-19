/**
 * TC-PROVINT-GATE — aggregate Provider Integration Readiness acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const PROVINT_CHECKS = [
  "provint-payment-readiness-checks.mjs",
  "provint-payment-zero-gateway-checks.mjs",
  "provint-hotel-availability-checklist-checks.mjs",
  "provint-hotel-rate-checklist-checks.mjs",
  "provint-hotel-reservation-checklist-checks.mjs",
  "provint-flight-source-checklist-checks.mjs",
  "provint-none-production-source-checks.mjs",
  "provint-config-secrets-posture-checks.mjs",
  "provint-evolution-boundary-checks.mjs",
  "provint-hotel-catalog-resolver-checks.mjs",
  "provint-vendor-sdk-ban-checks.mjs",
  "provint-test-fake-adapter-checks.mjs",
  "provint-cross-module-none-checks.mjs",
  "provint-named-vendor-deferral-checks.mjs",
  "provint-live-credential-deferral-checks.mjs",
];

const EVIDENCE_DOCS = [
  "PROVINT-T001-payment-readiness-evidence.md",
  "PROVINT-T002-payment-zero-gateway-evidence.md",
  "PROVINT-T003-hotel-availability-checklist-evidence.md",
  "PROVINT-T004-hotel-rate-checklist-evidence.md",
  "PROVINT-T005-hotel-reservation-checklist-evidence.md",
  "PROVINT-T006-flight-source-checklist-evidence.md",
  "PROVINT-T007-none-production-source-evidence.md",
  "PROVINT-T008-config-secrets-posture-evidence.md",
  "PROVINT-T009-evolution-boundary-evidence.md",
  "PROVINT-T010-hotel-catalog-resolver-evidence.md",
  "PROVINT-T011-vendor-sdk-ban-evidence.md",
  "PROVINT-T012-test-fake-adapter-evidence.md",
  "PROVINT-T013-cross-module-none-evidence.md",
  "PROVINT-T014-named-vendor-selection-deferral-evidence.md",
  "PROVINT-T015-live-credential-wiring-deferral-evidence.md",
  "PROVINT-GATE-acceptance-evidence.md",
];

const CHECKLIST_DOCS = [
  "P21-hotel-source-adapter-checklist.md",
  "P22-flight-source-adapter-checklist.md",
  "PROVINT-T008-provider-configuration-secrets-posture.md",
];

for (const script of PROVINT_CHECKS) {
  assert.ok(fs.existsSync(path.join(webRoot, "scripts", script)), `missing: ${script}`);
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(fs.existsSync(path.join(repoRoot, "docs/plans", doc)), `missing: ${doc}`);
}

for (const doc of CHECKLIST_DOCS) {
  assert.ok(fs.existsSync(path.join(repoRoot, "docs/plans", doc)), `missing checklist: ${doc}`);
}

console.log("provint-gate-checks: PASS");
console.log(`  scripts (${PROVINT_CHECKS.length}): ok`);
console.log(`  evidence (${EVIDENCE_DOCS.length}): ok`);
