/** HOMFEED-T014 — Locale-prefixed internal links. */
import assert from "node:assert/strict";
import { readSrc } from "./homfeed-common.mjs";

const view = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(view, /\/\$\{locale\}\/travelogues/);
assert.match(view, /\/\$\{locale\}\/hotels/);

console.log("homfeed-locale-prefixed-links-checks: PASS");
