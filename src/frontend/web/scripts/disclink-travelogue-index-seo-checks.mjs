/**
 * DISCLINK-T002 — Travelogue index SEO metadata.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./disclink-common.mjs";

const page = read(pagePath(["travelogues"]));
assert.match(page, /generateMetadata/);
assert.match(page, /loadComposedSeoMetadata/);
assert.match(page, /path: "travelogues"/);

console.log("disclink-travelogue-index-seo-checks: PASS");
