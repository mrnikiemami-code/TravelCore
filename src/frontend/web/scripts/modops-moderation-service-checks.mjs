/**
 * MODOPS-T003 — UGC moderation service (pending queue + mutations).
 */
import assert from "node:assert/strict";
import { readRepo } from "./modops-common.mjs";

const service = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Infrastructure/Services/UgcModerationService.cs",
);
assert.match(service, /class UgcModerationService : IUgcModerationService/);
assert.match(service, /ListPendingTraveloguesAsync/);
assert.match(service, /ApproveTravelogueAsync/);
assert.match(service, /RejectTravelogueAsync/);
assert.match(service, /PublishTravelogueAsync/);
assert.match(service, /ModerationStatus\.Pending/);

console.log("modops-moderation-service-checks: PASS");
