/** HOTIDX-T003 — Public hotel browse HTTP endpoint. */
import assert from "node:assert/strict";
import { readRepo } from "./hotidx-common.mjs";

const endpoints = readRepo(
  "src/backend/Modules/Place/TravelCore.Modules.Place.Infrastructure/Endpoints/PlacePublicEndpoints.cs",
);
assert.match(endpoints, /\/api\/place\/public/);
assert.match(endpoints, /\/hotels/);
assert.match(endpoints, /IPlacePublicHotelBrowseQuery/);

console.log("hotidx-public-endpoint-checks: PASS");
