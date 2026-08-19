/**
 * SEOVAL-T009 — Structured data (JSON-LD) contract checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { existsRepo, pagePath, read, readRepo, readSrc, webRoot } from "./seoval-common.mjs";

assert.ok(existsRepo("src/frontend/web/src/lib/seo/structured-data-contract.ts"));
assert.ok(existsRepo("src/frontend/web/src/lib/seo/load-breadcrumb-jsonld.ts"));

const structuredContract = readSrc("lib/seo/structured-data-contract.ts");
assert.match(structuredContract, /serializeBreadcrumbJsonLd/);

const breadcrumbTest = path.join(webRoot, "scripts/tests/breadcrumb-jsonld.test.ts");
assert.ok(fs.existsSync(breadcrumbTest), "breadcrumb-jsonld.test.ts missing");

assert.ok(
  existsRepo(
    "tests/Unit/TravelCore.Modules.Seo.UnitTests/SeoStructuredDataEngineTests.cs",
  ),
  "SeoStructuredDataEngineTests.cs missing",
);

const endpoints = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpoints, /\/structured-data\/breadcrumb/);

const JSONLD_PAGES = [
  pagePath(["destinations", "[slug]"]),
  pagePath(["tours", "[slug]"]),
  pagePath(["articles", "[slug]"]),
];

for (const file of JSONLD_PAGES) {
  const src = read(file);
  assert.match(src, /loadSeoBreadcrumbJsonLd/, `${file}: breadcrumb loader`);
  assert.match(src, /serializeBreadcrumbJsonLd/, `${file}: JSON-LD serializer`);
}

console.log("seoval-structured-data-checks: PASS");
console.log("  structured-data contracts + unit tests: ok");
console.log(`  page JSON-LD integration (${JSONLD_PAGES.length}): ok`);
