/**
 * TC-LAUNCHOPS-GATE — aggregate Launch Operations acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const LAUNCHOPS_CHECKS = [
  "launchops-root-locale-negotiation-checks.mjs",
  "launchops-locale-override-guard-checks.mjs",
  "launchops-health-endpoints-checks.mjs",
  "launchops-search-console-runbook-checks.mjs",
  "launchops-deployment-checklist-checks.mjs",
  "launchops-public-seo-endpoints-checks.mjs",
  "launchops-recovery-readiness-checks.mjs",
  "launchops-pipeline-protocol-checks.mjs",
  "launchops-hardening-health-boundary-checks.mjs",
  "launchops-observability-deferral-checks.mjs",
  "launchops-secrets-deployment-deferral-checks.mjs",
  "launchops-a11y-skip-link-checks.mjs",
  "launchops-mobile-viewport-checks.mjs",
  "launchops-e2e-deferral-checks.mjs",
  "launchops-live-gsc-deferral-checks.mjs",
];

const EVIDENCE_DOCS = [
  "LAUNCHOPS-T001-root-locale-negotiation-evidence.md",
  "LAUNCHOPS-T002-locale-override-guard-evidence.md",
  "LAUNCHOPS-T003-health-endpoints-evidence.md",
  "LAUNCHOPS-T004-search-console-ops-runbook.md",
  "LAUNCHOPS-T005-production-deployment-checklist.md",
  "LAUNCHOPS-T006-public-seo-endpoints-evidence.md",
  "LAUNCHOPS-T007-recovery-readiness-evidence.md",
  "LAUNCHOPS-T008-pipeline-protocol-evidence.md",
  "LAUNCHOPS-T009-hardening-health-boundary-evidence.md",
  "LAUNCHOPS-T010-observability-deferral-evidence.md",
  "LAUNCHOPS-T011-secrets-deployment-deferral-evidence.md",
  "LAUNCHOPS-T012-a11y-skip-link-evidence.md",
  "LAUNCHOPS-T013-mobile-viewport-evidence.md",
  "LAUNCHOPS-T014-e2e-live-crawl-deferral-evidence.md",
  "LAUNCHOPS-T015-live-gsc-verification-deferral-evidence.md",
  "LAUNCHOPS-GATE-acceptance-evidence.md",
];

for (const script of LAUNCHOPS_CHECKS) {
  assert.ok(fs.existsSync(path.join(webRoot, "scripts", script)), `missing: ${script}`);
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(fs.existsSync(path.join(repoRoot, "docs/plans", doc)), `missing: ${doc}`);
}

console.log("launchops-gate-checks: PASS");
console.log(`  scripts (${LAUNCHOPS_CHECKS.length}): ok`);
console.log(`  evidence (${EVIDENCE_DOCS.length}): ok`);
