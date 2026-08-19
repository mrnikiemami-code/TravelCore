/** HOTIDX-T004 — PlaceModule public browse wiring. */
import assert from "node:assert/strict";
import { readRepo } from "./hotidx-common.mjs";

const moduleSrc = readRepo(
  "src/backend/Modules/Place/TravelCore.Modules.Place.Infrastructure/PlaceModule.cs",
);
assert.match(moduleSrc, /IPlacePublicHotelBrowseQuery, PlacePublicQuery/);
assert.match(moduleSrc, /MapPlacePublicEndpoints/);

console.log("hotidx-module-wiring-checks: PASS");
