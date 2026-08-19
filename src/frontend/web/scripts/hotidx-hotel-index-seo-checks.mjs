/** HOTIDX-T006 — Hotel index SEO metadata. */
import assert from "node:assert/strict";
import { pagePath, read } from "./hotidx-common.mjs";

const page = read(pagePath(["hotels"]));
assert.match(page, /generateMetadata/);
assert.match(page, /loadComposedSeoMetadata/);
assert.match(page, /path: "hotels"/);

console.log("hotidx-hotel-index-seo-checks: PASS");
