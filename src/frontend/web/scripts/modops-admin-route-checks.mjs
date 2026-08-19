/**
 * MODOPS-T006 — Admin UGC moderation route exists.
 */
import assert from "node:assert/strict";
import { existsSrc, pagePath, read } from "./modops-common.mjs";

const page = pagePath(["admin", "ugc", "moderation"]);
assert.ok(existsSrc(`app/[locale]/admin/ugc/moderation/page.tsx`));

const src = read(page);
assert.match(src, /UgcModerationWorkflowIsland/);
assert.match(src, /AdminShell/);

console.log("modops-admin-route-checks: PASS");
