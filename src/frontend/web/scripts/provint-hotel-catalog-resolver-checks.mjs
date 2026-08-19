/**
 * PROVINT-T010 — Hotel source catalog + resolver registration.
 */
import assert from "node:assert/strict";
import { readRepo } from "./provint-common.mjs";

assert.match(readRepo(
  "src/backend/Modules/HotelBooking/TravelCore.Modules.HotelBooking.Contracts/HotelSourceReadinessContracts.cs",
), /interface IHotelSourceCatalog/);

const moduleSrc = readRepo(
  "src/backend/Modules/HotelBooking/TravelCore.Modules.HotelBooking.Infrastructure/HotelBookingModule.cs",
);
assert.match(moduleSrc, /IHotelAvailabilitySourceResolver/);
assert.match(moduleSrc, /IHotelRateOfferSourceResolver/);
assert.match(moduleSrc, /IHotelReservationSourceResolver/);

console.log("provint-hotel-catalog-resolver-checks: PASS");
