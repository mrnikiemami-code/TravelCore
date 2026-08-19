/**
 * MODOPS-T007 — Moderation workflow island + server actions.
 */
import assert from "node:assert/strict";
import { readSrc } from "./modops-common.mjs";

const island = readSrc("features/admin-ugc-moderation/ugc-moderation-workflow-island.tsx");
assert.match(island, /UgcModerationWorkflowIsland/);
assert.match(island, /listPendingTraveloguesAction/);
assert.match(island, /approveTravelogueAction/);
assert.match(island, /rejectTravelogueAction/);
assert.match(island, /publishTravelogueAction/);

const actions = readSrc("features/admin-ugc-moderation/actions.ts");
assert.match(actions, /listPendingTraveloguesAction/);
assert.match(actions, /approveTravelogueAction/);
assert.match(actions, /rejectTravelogueAction/);
assert.match(actions, /publishTravelogueAction/);
assert.match(actions, /\/api\/ugc\/moderation\/travelogues\/pending/);

console.log("modops-moderation-island-checks: PASS");
