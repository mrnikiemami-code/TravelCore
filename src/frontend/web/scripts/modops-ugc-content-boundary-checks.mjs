/**
 * MODOPS-T011 — UGC moderation is not Content CMS coupling.
 */
import assert from "node:assert/strict";
import { readRepo, readSrc } from "./modops-common.mjs";

const service = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Infrastructure/Services/UgcModerationService.cs",
);
assert.match(service, /Not Content CMS/i);
assert.doesNotMatch(service, /ContentItem/);

const actions = readSrc("features/admin-ugc-moderation/actions.ts");
assert.match(actions, /\/api\/ugc\/moderation\//);
assert.doesNotMatch(actions, /\/api\/content\//);

const copy = readSrc("features/admin-ugc-moderation/copy.ts");
assert.match(copy, /not the Content CMS/i);

console.log("modops-ugc-content-boundary-checks: PASS");
