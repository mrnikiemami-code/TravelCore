/** HOTIDX-T011 — Public shell on hotel index. */
import assert from "node:assert/strict";
import { pagePath, read } from "./hotidx-common.mjs";

const page = read(pagePath(["hotels"]));
assert.match(page, /PublicShell/);

console.log("hotidx-public-shell-checks: PASS");
