/**
 * TC-DISCLINK-GATE — aggregate Discovery Linking acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const DISCLINK_CHECKS = [
  "disclink-travelogue-index-checks.mjs",
  "disclink-travelogue-index-seo-checks.mjs",
  "disclink-ugc-travelogue-links-checks.mjs",
  "disclink-hotel-book-path-checks.mjs",
  "disclink-home-travelogues-link-checks.mjs",
  "disclink-home-link-hygiene-checks.mjs",
  "disclink-public-shell-checks.mjs",
  "disclink-travelogue-loader-checks.mjs",
  "disclink-discovery-not-search-checks.mjs",
  "disclink-locale-prefixed-links-checks.mjs",
  "disclink-places-route-retained-checks.mjs",
  "disclink-ugc-boundary-checks.mjs",
  "disclink-internal-linking-checks.mjs",
  "disclink-hotel-catalog-index-deferral-checks.mjs",
  "disclink-personalized-feed-deferral-checks.mjs",
];

const EVIDENCE_DOCS = [
  "DISCLINK-T001-travelogue-index-evidence.md",
  "DISCLINK-T002-travelogue-index-seo-evidence.md",
  "DISCLINK-T003-ugc-travelogue-links-evidence.md",
  "DISCLINK-T004-hotel-book-path-evidence.md",
  "DISCLINK-T005-home-travelogues-link-evidence.md",
  "DISCLINK-T006-home-link-hygiene-evidence.md",
  "DISCLINK-T007-public-shell-evidence.md",
  "DISCLINK-T008-travelogue-loader-evidence.md",
  "DISCLINK-T009-discovery-not-search-evidence.md",
  "DISCLINK-T010-locale-prefixed-links-evidence.md",
  "DISCLINK-T011-places-route-retained-evidence.md",
  "DISCLINK-T012-ugc-boundary-evidence.md",
  "DISCLINK-T013-internal-linking-evidence.md",
  "DISCLINK-T014-hotel-catalog-index-deferral-evidence.md",
  "DISCLINK-T015-personalized-feed-deferral-evidence.md",
  "DISCLINK-GATE-acceptance-evidence.md",
];

for (const script of DISCLINK_CHECKS) {
  assert.ok(fs.existsSync(path.join(webRoot, "scripts", script)), `missing: ${script}`);
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(fs.existsSync(path.join(repoRoot, "docs/plans", doc)), `missing: ${doc}`);
}

console.log("disclink-gate-checks: PASS");
console.log(`  scripts (${DISCLINK_CHECKS.length}): ok`);
console.log(`  evidence (${EVIDENCE_DOCS.length}): ok`);
