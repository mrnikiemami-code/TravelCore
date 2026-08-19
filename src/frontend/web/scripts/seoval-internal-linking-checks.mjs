/**
 * SEOVAL-T010 — Internal linking graph boundary and related-content checks.
 */
import assert from "node:assert/strict";
import { existsRepo, readSrc } from "./seoval-common.mjs";

assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Domain/SeoInternalLinkGraphBoundary.cs",
  ),
  "SeoInternalLinkGraphBoundary.cs missing",
);
assert.ok(
  existsRepo(
    "tests/Unit/TravelCore.Modules.Seo.UnitTests/SeoInternalLinkGraphBoundaryTests.cs",
  ),
  "SeoInternalLinkGraphBoundaryTests.cs missing",
);

const tourLoader = readSrc("features/tour-detail/load-tour-detail.ts");
assert.match(tourLoader, /loadRelatedToursByProduct/);
assert.match(tourLoader, /loadRelatedContentByDestinations/);

const relatedTours = readSrc("features/public-experience/load-related-tours.ts");
assert.match(relatedTours, /related-published/);

const relatedContent = readSrc("features/public-experience/load-related-content.ts");
assert.match(relatedContent, /related-published/);

console.log("seoval-internal-linking-checks: PASS");
console.log("  SEO internal link graph boundary: ok");
console.log("  related tours/content loaders: ok");
