/**
 * SEOVAL-T005 — Canonical URL contract and page mapping checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { readSrc, pagePath, read, webRoot } from "./seoval-common.mjs";

const metadataContract = readSrc("lib/seo/metadata-contract.ts");
assert.match(metadataContract, /canonicalHref/);

const composeTest = path.join(webRoot, "scripts/tests/metadata-compose.test.ts");
assert.ok(fs.existsSync(composeTest), "metadata-compose.test.ts missing");

const CANONICAL_PAGES = [
  pagePath(["destinations", "[slug]"]),
  pagePath(["tours", "[slug]"]),
  pagePath(["places", "[slug]"]),
  pagePath(["articles", "[slug]"]),
];

for (const file of CANONICAL_PAGES) {
  const src = read(file);
  assert.match(src, /composed\.canonicalHref/, `${file}: canonical mapping`);
  assert.match(src, /alternates/, `${file}: alternates block`);
}

console.log("seoval-canonical-checks: PASS");
console.log("  metadata contract + unit test: ok");
console.log(`  canonical page mapping (${CANONICAL_PAGES.length}): ok`);
