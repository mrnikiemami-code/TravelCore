/**
 * PROVINT-T007 — Flight/Hotel NONE production source posture.
 */
import assert from "node:assert/strict";
import { readRepo } from "./provint-common.mjs";

const hotel = readRepo(
  "src/backend/Modules/HotelBooking/TravelCore.Modules.HotelBooking.Contracts/HotelSourceReadinessContracts.cs",
);
assert.match(hotel, /NamedHotelSupplier = "NONE"/);
assert.match(hotel, /ProductionAvailabilitySource = "NONE"/);

const flight = readRepo(
  "src/backend/Modules/Flight/TravelCore.Modules.Flight.Contracts/FlightOwnershipBoundary.cs",
);
assert.match(flight, /NamedFlightSupplier = "NONE"/);
assert.match(flight, /ProductionSearchSource = "NONE"/);
assert.match(flight, /SupplierSdkImplemented = false/);

console.log("provint-none-production-source-checks: PASS");
