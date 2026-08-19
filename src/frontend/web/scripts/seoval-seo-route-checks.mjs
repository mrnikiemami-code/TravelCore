/**
 * SEOVAL-T003 — SeoRoute domain and API checks.
 */
import assert from "node:assert/strict";
import { existsRepo, readRepo } from "./seoval-common.mjs";

assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Domain/SeoRoute.cs",
  ),
  "SeoRoute.cs missing",
);
assert.ok(
  existsRepo("tests/Unit/TravelCore.Modules.Seo.UnitTests/SeoRouteTests.cs"),
  "SeoRouteTests.cs missing",
);
assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Contracts/SeoRouteContracts.cs",
  ),
  "SeoRouteContracts.cs missing",
);

const routeDomain = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Domain/SeoRoute.cs",
);
assert.match(routeDomain, /SeoResourceType/);
assert.match(routeDomain, /Create\(/);

const endpoints = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpoints, /\/routes\/by-resource/);

console.log("seoval-seo-route-checks: PASS");
console.log("  SeoRoute domain + contracts: ok");
console.log("  route resolution endpoint + unit tests: ok");
