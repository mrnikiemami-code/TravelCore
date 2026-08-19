/**
 * MODOPS-T008 — Admin catalog hub links to UGC moderation.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./modops-common.mjs";

const hub = read(pagePath(["admin", "catalog"]));
assert.match(hub, /admin\/ugc\/moderation/);

console.log("modops-catalog-hub-link-checks: PASS");
