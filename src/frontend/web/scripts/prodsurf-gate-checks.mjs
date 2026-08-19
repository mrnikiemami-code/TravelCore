/**
 * TC-PRODSURF-GATE — aggregate Product Surface Completion acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const PRODSURF_CHECKS = [
  "prodsurf-travelogue-api-checks.mjs",
  "prodsurf-travelogue-route-checks.mjs",
  "prodsurf-travelogue-seo-checks.mjs",
  "prodsurf-hotel-catalog-route-checks.mjs",
  "prodsurf-hotel-seo-checks.mjs",
  "prodsurf-hotel-book-route-checks.mjs",
  "prodsurf-transactional-noindex-checks.mjs",
  "prodsurf-uival-dev-routes-checks.mjs",
  "prodsurf-public-shell-checks.mjs",
  "prodsurf-ugc-content-boundary-checks.mjs",
  "prodsurf-place-ownership-checks.mjs",
  "prodsurf-travelogue-view-checks.mjs",
  "prodsurf-travelogue-loader-checks.mjs",
  "prodsurf-real-providers-deferral-checks.mjs",
  "prodsurf-moderation-workflow-deferral-checks.mjs",
];

const EVIDENCE_DOCS = [
  "PRODSURF-T001-travelogue-api-evidence.md",
  "PRODSURF-T002-travelogue-route-evidence.md",
  "PRODSURF-T003-travelogue-seo-evidence.md",
  "PRODSURF-T004-hotel-catalog-route-evidence.md",
  "PRODSURF-T005-hotel-seo-evidence.md",
  "PRODSURF-T006-hotel-book-route-evidence.md",
  "PRODSURF-T007-transactional-noindex-evidence.md",
  "PRODSURF-T008-uival-dev-routes-evidence.md",
  "PRODSURF-T009-public-shell-evidence.md",
  "PRODSURF-T010-ugc-content-boundary-evidence.md",
  "PRODSURF-T011-place-ownership-evidence.md",
  "PRODSURF-T012-travelogue-view-evidence.md",
  "PRODSURF-T013-travelogue-loader-evidence.md",
  "PRODSURF-T014-real-providers-deferral-evidence.md",
  "PRODSURF-T015-moderation-workflow-deferral-evidence.md",
  "PRODSURF-GATE-acceptance-evidence.md",
];

for (const script of PRODSURF_CHECKS) {
  assert.ok(fs.existsSync(path.join(webRoot, "scripts", script)), `missing: ${script}`);
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(fs.existsSync(path.join(repoRoot, "docs/plans", doc)), `missing: ${doc}`);
}

console.log("prodsurf-gate-checks: PASS");
console.log(`  scripts (${PRODSURF_CHECKS.length}): ok`);
console.log(`  evidence (${EVIDENCE_DOCS.length}): ok`);
