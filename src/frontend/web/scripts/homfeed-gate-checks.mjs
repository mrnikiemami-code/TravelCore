/**
 * TC-HOMFEED-GATE — aggregate Home Discovery Composition acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const HOMFEED_CHECKS = [
  "homfeed-composition-types-checks.mjs",
  "homfeed-composition-loader-checks.mjs",
  "homfeed-travelogue-preview-checks.mjs",
  "homfeed-hotel-preview-checks.mjs",
  "homfeed-home-page-wiring-checks.mjs",
  "homfeed-index-links-checks.mjs",
  "homfeed-detail-links-checks.mjs",
  "homfeed-entry-links-retained-checks.mjs",
  "homfeed-public-shell-checks.mjs",
  "homfeed-not-personalized-checks.mjs",
  "homfeed-not-search-engine-checks.mjs",
  "homfeed-no-dev-links-checks.mjs",
  "homfeed-ugc-place-boundary-checks.mjs",
  "homfeed-locale-prefixed-links-checks.mjs",
  "homfeed-ml-recommendation-deferral-checks.mjs",
];

const EVIDENCE_DOCS = [
  "HOMFEED-T001-composition-types-evidence.md",
  "HOMFEED-T002-composition-loader-evidence.md",
  "HOMFEED-T003-travelogue-preview-evidence.md",
  "HOMFEED-T004-hotel-preview-evidence.md",
  "HOMFEED-T005-home-page-wiring-evidence.md",
  "HOMFEED-T006-index-links-evidence.md",
  "HOMFEED-T007-detail-links-evidence.md",
  "HOMFEED-T008-entry-links-retained-evidence.md",
  "HOMFEED-T009-public-shell-evidence.md",
  "HOMFEED-T010-not-personalized-evidence.md",
  "HOMFEED-T011-not-search-engine-evidence.md",
  "HOMFEED-T012-no-dev-links-evidence.md",
  "HOMFEED-T013-ugc-place-boundary-evidence.md",
  "HOMFEED-T014-locale-prefixed-links-evidence.md",
  "HOMFEED-T015-ml-recommendation-deferral-evidence.md",
  "HOMFEED-GATE-acceptance-evidence.md",
];

for (const script of HOMFEED_CHECKS) {
  assert.ok(fs.existsSync(path.join(webRoot, "scripts", script)), `missing: ${script}`);
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(fs.existsSync(path.join(repoRoot, "docs/plans", doc)), `missing: ${doc}`);
}

console.log("homfeed-gate-checks: PASS");
console.log(`  scripts (${HOMFEED_CHECKS.length}): ok`);
console.log(`  evidence (${EVIDENCE_DOCS.length}): ok`);
