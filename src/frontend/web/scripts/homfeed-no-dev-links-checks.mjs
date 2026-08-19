/** HOMFEED-T012 — No dev links on production home. */
import assert from "node:assert/strict";
import { pagePath, read } from "./homfeed-common.mjs";

const page = read(pagePath([]));
assert.doesNotMatch(page, /includeDevLinks/);

console.log("homfeed-no-dev-links-checks: PASS");
