/**
 * LAUNCHOPS-T006 — Public SEO sitemap/robots endpoints.
 */
import assert from "node:assert/strict";
import { readRepo } from "./launchops-common.mjs";

const endpoints = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpoints, /\/sitemap\.xml/);
assert.match(endpoints, /\/robots\.txt/);

console.log("launchops-public-seo-endpoints-checks: PASS");
