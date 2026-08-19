/**
 * PRODDEL-T008 — Content pages production route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const page = read(pagePath(["articles", "[slug]"]));
assert.match(page, /ContentDetailView/);
assert.match(page, /loadContentDetailPage/);
assert.match(page, /loadComposedSeoMetadata/);

console.log("proddel-content-pages-checks: PASS");
