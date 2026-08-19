/** HOTIDX-T001 — Public hotel browse contracts. */
import assert from "node:assert/strict";
import { readRepo } from "./hotidx-common.mjs";

const contracts = readRepo(
  "src/backend/Modules/Place/TravelCore.Modules.Place.Contracts/PlacePublicReadContracts.cs",
);
assert.match(contracts, /PublicHotelBrowseItem/);
assert.match(contracts, /IPlacePublicHotelBrowseQuery/);
assert.match(contracts, /MaxPublicHotels/);

console.log("hotidx-public-contracts-checks: PASS");
