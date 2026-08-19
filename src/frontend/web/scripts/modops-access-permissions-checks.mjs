/**
 * MODOPS-T001 — UGC moderation Access permissions and authorization policies.
 */
import assert from "node:assert/strict";
import { readRepo } from "./modops-common.mjs";

const catalog = readRepo(
  "src/backend/Modules/Access/TravelCore.Modules.Access.Domain/AccessPermissionCatalog.cs",
);
assert.match(catalog, /ugc\.moderation\.read/);
assert.match(catalog, /ugc\.moderation\.moderate/);

const policies = readRepo(
  "src/backend/Modules/Access/TravelCore.Modules.Access.Infrastructure/Authorization/AccessAuthorizationPolicies.cs",
);
assert.match(policies, /UgcModerationRead = "Access\.Ugc\.Moderation\.Read"/);
assert.match(policies, /UgcModerationModerate = "Access\.Ugc\.Moderation\.Moderate"/);

const moduleSrc = readRepo(
  "src/backend/Modules/Access/TravelCore.Modules.Access.Infrastructure/AccessModule.cs",
);
assert.match(moduleSrc, /AccessAuthorizationPolicies\.UgcModerationRead/);
assert.match(moduleSrc, /AccessAuthorizationPolicies\.UgcModerationModerate/);
assert.match(moduleSrc, /PermissionRequirement\("ugc\.moderation\.read"\)/);
assert.match(moduleSrc, /PermissionRequirement\("ugc\.moderation\.moderate"\)/);

console.log("modops-access-permissions-checks: PASS");
