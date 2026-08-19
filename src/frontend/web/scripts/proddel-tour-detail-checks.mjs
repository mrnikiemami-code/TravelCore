/**
 * PRODDEL-T006 — Tour detail production route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const page = read(pagePath(["tours", "[slug]"]));
assert.match(page, /TourDetailView/);
assert.match(page, /loadTourDetailPage/);
assert.match(page, /loadComposedSeoMetadata/);

console.log("proddel-tour-detail-checks: PASS");
