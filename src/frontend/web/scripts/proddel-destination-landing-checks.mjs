/**
 * PRODDEL-T005 — Destination landing production route.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const page = read(pagePath(["destinations", "[slug]"]));
assert.match(page, /DestinationLandingView/);
assert.match(page, /loadComposedSeoMetadata/);
assert.match(page, /loadDestinationLandingPage/);

console.log("proddel-destination-landing-checks: PASS");
