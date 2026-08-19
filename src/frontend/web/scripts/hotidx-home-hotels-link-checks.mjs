/** HOTIDX-T010 — Home discovery hotels entry link. */
import assert from "node:assert/strict";
import { readSrc } from "./hotidx-common.mjs";

const home = readSrc("features/home-discovery/home-discovery-view.tsx");
assert.match(home, /\/hotels/);
assert.match(home, /Hotel catalog browse/);

console.log("hotidx-home-hotels-link-checks: PASS");
