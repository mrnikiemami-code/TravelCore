/**
 * PRODDEL-T003 — Production home excludes dev-only discovery links.
 */
import assert from "node:assert/strict";
import { pagePath, read, readSrc } from "./proddel-common.mjs";

const home = read(pagePath([]));
assert.doesNotMatch(home, /includeDevLinks/);
assert.match(home, /<HomeDiscoveryView locale=\{locale\} \/>/);

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /includeDevLinks/);
assert.match(view, /devValidationLinks/);
assert.match(view, /includeDevLinks = false/);

console.log("proddel-home-link-hygiene-checks: PASS");
