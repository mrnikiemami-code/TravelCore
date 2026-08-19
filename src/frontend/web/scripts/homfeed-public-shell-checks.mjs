/** HOMFEED-T009 — Public shell on production home. */
import assert from "node:assert/strict";
import { pagePath, read } from "./homfeed-common.mjs";

const page = read(pagePath([]));
assert.match(page, /PublicShell/);

console.log("homfeed-public-shell-checks: PASS");
