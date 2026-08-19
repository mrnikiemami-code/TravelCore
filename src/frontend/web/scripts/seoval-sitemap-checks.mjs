/**
 * SEOVAL-T008 — Sitemap engine and public endpoint checks.
 */
import assert from "node:assert/strict";
import { existsRepo, readRepo } from "./seoval-common.mjs";

assert.ok(
  existsRepo(
    "tests/Unit/TravelCore.Modules.Seo.UnitTests/SeoSitemapEngineTests.cs",
  ),
  "SeoSitemapEngineTests.cs missing",
);
assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Contracts/SeoSitemapContracts.cs",
  ),
  "SeoSitemapContracts.cs missing",
);
assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Services/SeoSitemapApplicationService.cs",
  ),
  "SeoSitemapApplicationService missing",
);

const endpoints = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpoints, /\/sitemap\.xml/);
assert.match(endpoints, /\/robots\.txt/);

console.log("seoval-sitemap-checks: PASS");
console.log("  sitemap engine + contracts: ok");
console.log("  sitemap.xml + robots.txt endpoints: ok");
