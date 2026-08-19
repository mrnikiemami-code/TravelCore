/**
 * MODOPS-T002 — UGC moderation contracts (queue DTO + service port).
 */
import assert from "node:assert/strict";
import { readRepo } from "./modops-common.mjs";

const contracts = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Contracts/UgcModerationContracts.cs",
);
assert.match(contracts, /interface IUgcModerationService/);
assert.match(contracts, /record ModerationQueueTravelogueItem/);
assert.match(contracts, /ListPendingTraveloguesAsync/);
assert.match(contracts, /ApproveTravelogueAsync/);
assert.match(contracts, /RejectTravelogueAsync/);
assert.match(contracts, /PublishTravelogueAsync/);

console.log("modops-moderation-contracts-checks: PASS");
