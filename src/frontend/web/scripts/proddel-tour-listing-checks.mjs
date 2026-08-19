/**
 * PRODDEL-T004 — Tour listing production route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const listing = read(pagePath(["tours"]));
assert.match(listing, /generateMetadata/);
assert.match(listing, /loadComposedSeoMetadata/);
assert.match(listing, /PublicTourListingView|listing-view/);

console.log("proddel-tour-listing-checks: PASS");
