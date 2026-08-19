/**
 * PRODSURF-T001 — UGC public travelogue GetById API.
 */
import assert from "node:assert/strict";
import { readRepo } from "./prodsurf-common.mjs";

const contracts = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Contracts/UgcPublicReadContracts.cs",
);
assert.match(contracts, /GetByIdAsync/);

const endpoints = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Infrastructure/Endpoints/UgcPublicEndpoints.cs",
);
assert.match(endpoints, /\/travelogues\/\{travelogueId:guid\}/);

const query = readRepo(
  "src/backend/Modules/Ugc/TravelCore.Modules.Ugc.Infrastructure/Services/UgcPublicQuery.cs",
);
assert.match(query, /GetByIdAsync/);

console.log("prodsurf-travelogue-api-checks: PASS");
