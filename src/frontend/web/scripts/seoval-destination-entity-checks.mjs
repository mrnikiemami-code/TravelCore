/**
 * SEOVAL-T002 — Destination entity SEO integration checks.
 */
import assert from "node:assert/strict";
import {
  existsRepo,
  read,
  readRepo,
  pagePath,
} from "./seoval-common.mjs";

assert.ok(
  existsRepo(
    "src/frontend/web/src/features/destination-landing/load-destination-landing.ts",
  ),
  "loadDestinationLandingPage missing",
);

const destPage = read(pagePath(["destinations", "[slug]"]));
assert.match(destPage, /loadDestinationLandingPage/);
assert.match(destPage, /loadComposedSeoMetadata/);
assert.match(destPage, /languagesFromComposed/);
assert.match(destPage, /robotsFromComposed/);
assert.match(destPage, /loadSeoBreadcrumbJsonLd/);
assert.match(destPage, /public path publication lives in SEO/);

assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Services/SeoAdminDestinationPostureService.cs",
  ),
  "SeoAdminDestinationPostureService missing",
);

const endpoints = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpoints, /\/publication\/destination/);
assert.match(endpoints, /\/admin\/destination-posture/);

console.log("seoval-destination-entity-checks: PASS");
console.log("  destination landing loader + page: ok");
console.log("  SEO publication + posture endpoints: ok");
