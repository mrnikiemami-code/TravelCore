/**
 * SEOVAL-T013 — Content (Article) page metadata integration checks.
 */
import assert from "node:assert/strict";
import { pagePath, read, readRepo } from "./seoval-common.mjs";

const articlePage = read(pagePath(["articles", "[slug]"]));
assert.match(articlePage, /generateMetadata/);
assert.match(articlePage, /loadComposedSeoMetadata/);
assert.match(articlePage, /loadContentDetailPage/);
assert.match(articlePage, /"Article"/);
assert.match(articlePage, /vm\.publicPath/);

const endpoints = readRepo(
  "src/backend/Modules/Seo/TravelCore.Modules.Seo.Infrastructure/Endpoints/SeoEndpoints.cs",
);
assert.match(endpoints, /\/publication\/article/);

console.log("seoval-content-pages-checks: PASS");
console.log("  article page SEO metadata: ok");
console.log("  article publication endpoint: ok");
