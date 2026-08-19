/**
 * TC-SEOVAL-GATE — aggregate SEO Validation acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const SEOVAL_CHECKS = [
  "seoval-url-locale-checks.mjs",
  "seoval-destination-entity-checks.mjs",
  "seoval-seo-route-checks.mjs",
  "seoval-localized-slugs-checks.mjs",
  "seoval-canonical-checks.mjs",
  "seoval-hreflang-checks.mjs",
  "seoval-redirects-checks.mjs",
  "seoval-sitemap-checks.mjs",
  "seoval-structured-data-checks.mjs",
  "seoval-internal-linking-checks.mjs",
  "seoval-tour-landing-checks.mjs",
  "seoval-place-pages-checks.mjs",
  "seoval-content-pages-checks.mjs",
  "seoval-programmatic-seo-checks.mjs",
  "seoval-search-console-checks.mjs",
];

const EVIDENCE_DOCS = [
  "SEOVAL-T001-url-locale-constitution-validation-evidence.md",
  "SEOVAL-T002-destination-entity-validation-evidence.md",
  "SEOVAL-T003-seo-route-validation-evidence.md",
  "SEOVAL-T004-localized-slugs-validation-evidence.md",
  "SEOVAL-T005-canonical-validation-evidence.md",
  "SEOVAL-T006-hreflang-validation-evidence.md",
  "SEOVAL-T007-redirects-validation-evidence.md",
  "SEOVAL-T008-sitemap-validation-evidence.md",
  "SEOVAL-T009-structured-data-validation-evidence.md",
  "SEOVAL-T010-internal-linking-validation-evidence.md",
  "SEOVAL-T011-tour-landing-validation-evidence.md",
  "SEOVAL-T012-place-pages-validation-evidence.md",
  "SEOVAL-T013-content-pages-validation-evidence.md",
  "SEOVAL-T014-programmatic-seo-validation-evidence.md",
  "SEOVAL-T015-search-console-deferral-evidence.md",
  "SEOVAL-GATE-acceptance-evidence.md",
];

for (const script of SEOVAL_CHECKS) {
  assert.ok(
    fs.existsSync(path.join(webRoot, "scripts", script)),
    `missing seoval script: ${script}`,
  );
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(
    fs.existsSync(path.join(repoRoot, "docs/plans", doc)),
    `missing evidence doc: ${doc}`,
  );
}

assert.ok(
  fs.existsSync(path.join(repoRoot, "docs/plans/SEOVAL-implementation-plan.md")),
  "SEOVAL implementation plan missing",
);

console.log("seoval-gate-checks: PASS");
console.log(`  seoval scripts (${SEOVAL_CHECKS.length}): ok`);
console.log(`  evidence docs (${EVIDENCE_DOCS.length}): ok`);
