/**
 * DISCLINK-T007 — Public shell on travelogue index.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./disclink-common.mjs";

const page = read(pagePath(["travelogues"]));
assert.match(page, /PublicShell/);

console.log("disclink-public-shell-checks: PASS");
