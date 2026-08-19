/**
 * MODOPS-T010 — Admin endpoints use Access.Ugc.Moderation policies.
 */
import assert from "node:assert/strict";
import { readRepo } from "./modops-common.mjs";

const endpoints = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Infrastructure/Endpoints/UgcAdminEndpoints.cs",
);
assert.match(endpoints, /Access\.Ugc\.Moderation\.Read/);
assert.match(endpoints, /Access\.Ugc\.Moderation\.Moderate/);
assert.match(endpoints, /RequireAuthorization\(ModerationReadPolicy\)/);
assert.match(endpoints, /RequireAuthorization\(ModerationModeratePolicy\)/);

console.log("modops-access-policy-guard-checks: PASS");
