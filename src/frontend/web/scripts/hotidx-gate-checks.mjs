/**
 * TC-HOTIDX-GATE — aggregate Hotel Catalog Browse Index acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const HOTIDX_CHECKS = [
  "hotidx-public-contracts-checks.mjs",
  "hotidx-public-query-checks.mjs",
  "hotidx-public-endpoint-checks.mjs",
  "hotidx-module-wiring-checks.mjs",
  "hotidx-hotel-index-route-checks.mjs",
  "hotidx-hotel-index-seo-checks.mjs",
  "hotidx-hotel-loader-checks.mjs",
  "hotidx-hotel-discovery-view-checks.mjs",
  "hotidx-hotel-detail-links-checks.mjs",
  "hotidx-home-hotels-link-checks.mjs",
  "hotidx-public-shell-checks.mjs",
  "hotidx-discovery-not-search-checks.mjs",
  "hotidx-locale-prefixed-links-checks.mjs",
  "hotidx-places-route-retained-checks.mjs",
  "hotidx-personalized-feed-deferral-checks.mjs",
];

const EVIDENCE_DOCS = [
  "HOTIDX-T001-public-contracts-evidence.md",
  "HOTIDX-T002-public-query-evidence.md",
  "HOTIDX-T003-public-endpoint-evidence.md",
  "HOTIDX-T004-module-wiring-evidence.md",
  "HOTIDX-T005-hotel-index-route-evidence.md",
  "HOTIDX-T006-hotel-index-seo-evidence.md",
  "HOTIDX-T007-hotel-loader-evidence.md",
  "HOTIDX-T008-hotel-discovery-view-evidence.md",
  "HOTIDX-T009-hotel-detail-links-evidence.md",
  "HOTIDX-T010-home-hotels-link-evidence.md",
  "HOTIDX-T011-public-shell-evidence.md",
  "HOTIDX-T012-discovery-not-search-evidence.md",
  "HOTIDX-T013-locale-prefixed-links-evidence.md",
  "HOTIDX-T014-places-route-retained-evidence.md",
  "HOTIDX-T015-personalized-hotel-feed-deferral-evidence.md",
  "HOTIDX-GATE-acceptance-evidence.md",
];

for (const script of HOTIDX_CHECKS) {
  assert.ok(fs.existsSync(path.join(webRoot, "scripts", script)), `missing: ${script}`);
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(fs.existsSync(path.join(repoRoot, "docs/plans", doc)), `missing: ${doc}`);
}

console.log("hotidx-gate-checks: PASS");
console.log(`  scripts (${HOTIDX_CHECKS.length}): ok`);
console.log(`  evidence (${EVIDENCE_DOCS.length}): ok`);
