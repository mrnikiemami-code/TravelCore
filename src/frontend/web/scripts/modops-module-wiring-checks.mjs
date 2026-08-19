/**
 * MODOPS-T005 — UgcModule composition wiring for moderation.
 */
import assert from "node:assert/strict";
import { readRepo } from "./modops-common.mjs";

const moduleSrc = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Infrastructure/UgcModule.cs",
);
assert.match(moduleSrc, /AddScoped<IUgcModerationService, UgcModerationService>/);
assert.match(moduleSrc, /MapUgcAdminEndpoints/);

console.log("modops-module-wiring-checks: PASS");
