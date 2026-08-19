/**
 * TC-MODOPS-GATE — aggregate Moderation Operations acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const MODOPS_CHECKS = [
  "modops-access-permissions-checks.mjs",
  "modops-moderation-contracts-checks.mjs",
  "modops-moderation-service-checks.mjs",
  "modops-admin-endpoints-checks.mjs",
  "modops-module-wiring-checks.mjs",
  "modops-admin-route-checks.mjs",
  "modops-moderation-island-checks.mjs",
  "modops-catalog-hub-link-checks.mjs",
  "modops-admin-noindex-checks.mjs",
  "modops-access-policy-guard-checks.mjs",
  "modops-ugc-content-boundary-checks.mjs",
  "modops-lifecycle-guard-checks.mjs",
  "modops-travelogue-scope-deferral-checks.mjs",
  "modops-report-moderation-deferral-checks.mjs",
  "modops-bulk-moderation-deferral-checks.mjs",
];

const EVIDENCE_DOCS = [
  "MODOPS-T001-access-permissions-evidence.md",
  "MODOPS-T002-moderation-contracts-evidence.md",
  "MODOPS-T003-moderation-service-evidence.md",
  "MODOPS-T004-admin-endpoints-evidence.md",
  "MODOPS-T005-module-wiring-evidence.md",
  "MODOPS-T006-admin-route-evidence.md",
  "MODOPS-T007-moderation-island-evidence.md",
  "MODOPS-T008-catalog-hub-link-evidence.md",
  "MODOPS-T009-admin-noindex-evidence.md",
  "MODOPS-T010-access-policy-guard-evidence.md",
  "MODOPS-T011-ugc-content-boundary-evidence.md",
  "MODOPS-T012-lifecycle-guard-evidence.md",
  "MODOPS-T013-travelogue-scope-deferral-evidence.md",
  "MODOPS-T014-report-moderation-deferral-evidence.md",
  "MODOPS-T015-bulk-moderation-deferral-evidence.md",
  "MODOPS-GATE-acceptance-evidence.md",
];

for (const script of MODOPS_CHECKS) {
  assert.ok(fs.existsSync(path.join(webRoot, "scripts", script)), `missing: ${script}`);
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(fs.existsSync(path.join(repoRoot, "docs/plans", doc)), `missing: ${doc}`);
}

console.log("modops-gate-checks: PASS");
console.log(`  scripts (${MODOPS_CHECKS.length}): ok`);
console.log(`  evidence (${EVIDENCE_DOCS.length}): ok`);
