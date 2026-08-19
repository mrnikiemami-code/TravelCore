/**
 * TC-PRODDEL-GATE — aggregate Product Delivery acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const PRODDEL_CHECKS = [
  "proddel-home-discovery-checks.mjs",
  "proddel-home-seo-checks.mjs",
  "proddel-home-link-hygiene-checks.mjs",
  "proddel-tour-listing-checks.mjs",
  "proddel-destination-landing-checks.mjs",
  "proddel-tour-detail-checks.mjs",
  "proddel-place-pages-checks.mjs",
  "proddel-content-pages-checks.mjs",
  "proddel-transactional-noindex-checks.mjs",
  "proddel-entry-routes-checks.mjs",
  "proddel-landing-pages-checks.mjs",
  "proddel-public-shell-checks.mjs",
  "proddel-locale-root-checks.mjs",
  "proddel-uival-dev-routes-checks.mjs",
  "proddel-production-deployment-deferral-checks.mjs",
];

const EVIDENCE_DOCS = [
  "PRODDEL-T001-home-discovery-delivery-evidence.md",
  "PRODDEL-T002-home-seo-delivery-evidence.md",
  "PRODDEL-T003-home-link-hygiene-delivery-evidence.md",
  "PRODDEL-T004-tour-listing-delivery-evidence.md",
  "PRODDEL-T005-destination-landing-delivery-evidence.md",
  "PRODDEL-T006-tour-detail-delivery-evidence.md",
  "PRODDEL-T007-place-pages-delivery-evidence.md",
  "PRODDEL-T008-content-pages-delivery-evidence.md",
  "PRODDEL-T009-transactional-noindex-delivery-evidence.md",
  "PRODDEL-T010-entry-routes-delivery-evidence.md",
  "PRODDEL-T011-landing-pages-delivery-evidence.md",
  "PRODDEL-T012-public-shell-delivery-evidence.md",
  "PRODDEL-T013-locale-root-delivery-evidence.md",
  "PRODDEL-T014-uival-dev-routes-delivery-evidence.md",
  "PRODDEL-T015-production-deployment-deferral-evidence.md",
  "PRODDEL-GATE-acceptance-evidence.md",
];

for (const script of PRODDEL_CHECKS) {
  assert.ok(
    fs.existsSync(path.join(webRoot, "scripts", script)),
    `missing proddel script: ${script}`,
  );
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(
    fs.existsSync(path.join(repoRoot, "docs/plans", doc)),
    `missing evidence doc: ${doc}`,
  );
}

console.log("proddel-gate-checks: PASS");
console.log(`  proddel scripts (${PRODDEL_CHECKS.length}): ok`);
console.log(`  evidence docs (${EVIDENCE_DOCS.length}): ok`);
