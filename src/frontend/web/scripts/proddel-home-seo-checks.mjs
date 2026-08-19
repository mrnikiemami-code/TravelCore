/**
 * PRODDEL-T002 — Home SEO metadata via compose contract.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const home = read(pagePath([]));
assert.match(home, /generateMetadata/);
assert.match(home, /loadComposedSeoMetadata/);
assert.match(home, /languagesFromComposed/);
assert.match(home, /robotsFromComposed/);

console.log("proddel-home-seo-checks: PASS");
