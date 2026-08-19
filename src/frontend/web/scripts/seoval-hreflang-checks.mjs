/**
 * SEOVAL-T006 — hreflang contract and integration checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { existsRepo, pagePath, read, readRepo, readSrc, webRoot } from "./seoval-common.mjs";

assert.ok(existsRepo("src/frontend/web/src/lib/seo/hreflang-contract.ts"));
assert.ok(existsRepo("src/frontend/web/src/lib/seo/load-hreflang.ts"));

const hreflangContract = readSrc("lib/seo/hreflang-contract.ts");
assert.match(hreflangContract, /SeoHreflangAlternate/);

const metadataContract = readSrc("lib/seo/metadata-contract.ts");
assert.match(metadataContract, /languagesFromComposed/);
assert.match(metadataContract, /hreflangAlternates/);

const hreflangTest = path.join(webRoot, "scripts/tests/hreflang.test.ts");
assert.ok(fs.existsSync(hreflangTest), "hreflang.test.ts missing");

assert.ok(
  existsRepo("tests/Unit/TravelCore.Modules.Seo.UnitTests/SeoHreflangEngineTests.cs"),
  "SeoHreflangEngineTests.cs missing",
);

const endpointsSrc = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpointsSrc, /\/hreflang\//);

const destPage = read(pagePath(["destinations", "[slug]"]));
assert.match(destPage, /languagesFromComposed/);

console.log("seoval-hreflang-checks: PASS");
console.log("  frontend hreflang contracts + test: ok");
console.log("  backend hreflang engine + endpoints: ok");
