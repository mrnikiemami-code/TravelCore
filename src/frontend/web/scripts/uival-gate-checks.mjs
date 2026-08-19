/**
 * TC-UIVAL-GATE — aggregate UI Validation acceptance checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const webRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const repoRoot = path.resolve(webRoot, "../../..");

const UIVAL_CHECKS = [
  "uival-foundation-checks.mjs",
  "uival-foreign-tour-checks.mjs",
  "uival-experience-tour-checks.mjs",
  "uival-tour-listing-checks.mjs",
  "uival-destination-landing-checks.mjs",
  "uival-hotel-detail-checks.mjs",
  "uival-home-discovery-checks.mjs",
  "uival-content-article-checks.mjs",
  "uival-travelogue-checks.mjs",
  "uival-visa-checks.mjs",
  "uival-booking-checkout-checks.mjs",
  "uival-flight-search-checks.mjs",
  "uival-hotel-booking-search-checks.mjs",
  "uival-admin-surfaces-checks.mjs",
  "uival-agency-surfaces-checks.mjs",
];

const DEV_ROUTES = [
  "foundation",
  "foreign-tour",
  "experience-tour",
  "tour-listing",
  "destination-landing",
  "hotel-detail",
  "home-discovery",
  "content-article",
  "travelogue",
  "visa",
  "booking-checkout",
  "flight-search",
  "hotel-booking-search",
  "admin-surfaces",
  "agency-surfaces",
];

const EVIDENCE_DOCS = [
  "UIVAL-T001-foundation-primitives-validation-evidence.md",
  "UIVAL-T002-foreign-tour-detail-validation-evidence.md",
  "UIVAL-T003-experience-tour-detail-validation-evidence.md",
  "UIVAL-T004-tour-listing-search-validation-evidence.md",
  "UIVAL-T005-destination-landing-validation-evidence.md",
  "UIVAL-T006-hotel-detail-validation-evidence.md",
  "UIVAL-T007-home-discovery-validation-evidence.md",
  "UIVAL-T008-content-article-validation-evidence.md",
  "UIVAL-T009-travelogue-validation-evidence.md",
  "UIVAL-T010-visa-validation-evidence.md",
  "UIVAL-T011-booking-checkout-validation-evidence.md",
  "UIVAL-T012-flight-search-validation-evidence.md",
  "UIVAL-T013-hotel-booking-search-validation-evidence.md",
  "UIVAL-T014-admin-surfaces-validation-evidence.md",
  "UIVAL-T015-agency-surfaces-validation-evidence.md",
  "UIVAL-GATE-acceptance-evidence.md",
];

for (const script of UIVAL_CHECKS) {
  assert.ok(
    fs.existsSync(path.join(webRoot, "scripts", script)),
    `missing uival script: ${script}`,
  );
}

for (const route of DEV_ROUTES) {
  assert.ok(
    fs.existsSync(
      path.join(webRoot, "src", "app", "[locale]", "dev", route, "page.tsx"),
    ),
    `missing dev route: /dev/${route}`,
  );
}

for (const doc of EVIDENCE_DOCS) {
  assert.ok(
    fs.existsSync(path.join(repoRoot, "docs", "plans", doc)),
    `missing evidence: ${doc}`,
  );
}

const plan = fs.readFileSync(
  path.join(repoRoot, "docs", "plans", "UIVAL-implementation-plan.md"),
  "utf8",
);
assert.match(plan, /TC-UIVAL-GATE/);
assert.match(plan, /UI Validation Sequence/);

console.log("uival-gate-checks: PASS");
console.log(`  scripts (${UIVAL_CHECKS.length}): ok`);
console.log(`  dev routes (${DEV_ROUTES.length}): ok`);
console.log(`  evidence docs (${EVIDENCE_DOCS.length}): ok`);
