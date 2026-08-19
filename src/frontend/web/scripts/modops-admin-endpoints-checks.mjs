/**
 * MODOPS-T004 — Admin UGC moderation HTTP endpoints.
 */
import assert from "node:assert/strict";
import { readRepo } from "./modops-common.mjs";

const endpoints = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Infrastructure/Endpoints/UgcAdminEndpoints.cs",
);
assert.match(endpoints, /MapUgcAdminEndpoints/);
assert.match(endpoints, /\/api\/ugc\/moderation/);
assert.match(endpoints, /\/pending/);
assert.match(endpoints, /\/approve/);
assert.match(endpoints, /\/reject/);
assert.match(endpoints, /\/publish/);
assert.match(endpoints, /RequireAuthorization/);

console.log("modops-admin-endpoints-checks: PASS");
