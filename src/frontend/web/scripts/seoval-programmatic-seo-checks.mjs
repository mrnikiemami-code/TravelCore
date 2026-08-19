/**
 * SEOVAL-T014 — Controlled programmatic SEO (landing pages) checks.
 */
import assert from "node:assert/strict";
import { existsRepo, pagePath, read, readRepo } from "./seoval-common.mjs";

assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Domain/SeoProgrammaticLandingBoundary.cs",
  ),
  "SeoProgrammaticLandingBoundary.cs missing",
);
assert.ok(
  existsRepo(
    "tests/Unit/TravelCore.Modules.Seo.UnitTests/SeoProgrammaticLandingAndRouteQualityBoundaryTests.cs",
  ),
  "Programmatic landing boundary tests missing",
);

const landingPage = read(pagePath(["landing-pages", "[slug]"]));
assert.match(landingPage, /generateMetadata/);
assert.match(landingPage, /loadComposedSeoMetadata/);
assert.match(landingPage, /"LandingPage"/);

const endpoints = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpoints, /\/publication\/landing-page/);

console.log("seoval-programmatic-seo-checks: PASS");
console.log("  programmatic landing boundary + tests: ok");
console.log("  landing-pages route + publication: ok");
