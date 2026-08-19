/**
 * SEOVAL-T007 — Redirect engine and resolution endpoint checks.
 */
import assert from "node:assert/strict";
import { existsRepo, readRepo } from "./seoval-common.mjs";

assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Domain/SeoRedirectEngine.cs",
  ),
  "SeoRedirectEngine.cs missing",
);
assert.ok(
  existsRepo(
    "tests/Unit/TravelCore.Modules.Seo.UnitTests/SeoRedirectEngineTests.cs",
  ),
  "SeoRedirectEngineTests.cs missing",
);
assert.ok(
  existsRepo(
    "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Services/SeoRedirectApplicationService.cs",
  ),
  "SeoRedirectApplicationService missing",
);

const engine = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Domain/SeoRedirectEngine.cs",
);
assert.match(engine, /Resolve\(/);

const endpoints = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpoints, /\/resolve\//);
assert.match(endpoints, /\/canonical\//);

console.log("seoval-redirects-checks: PASS");
console.log("  redirect engine + unit tests: ok");
console.log("  resolve/canonical endpoints: ok");
