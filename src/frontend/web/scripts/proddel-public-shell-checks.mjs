/**
 * PRODDEL-T012 — Public shell consistency on production home.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./proddel-common.mjs";

const home = read(pagePath([]));
assert.match(home, /PublicShell/);
assert.match(home, /header=/);
assert.match(home, /footer=/);
assert.doesNotMatch(home, /AdminShell/);

console.log("proddel-public-shell-checks: PASS");
