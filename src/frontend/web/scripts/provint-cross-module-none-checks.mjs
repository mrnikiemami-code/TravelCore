/**
 * PROVINT-T013 — Cross-module production provider NONE guard.
 */
import assert from "node:assert/strict";
import { readRepo } from "./provint-common.mjs";

assert.match(readRepo(
  "src/backend/Modules/Payment/TravelCore.Modules.Payment.Contracts/PaymentProviderTrustBoundary.cs",
), /NamedProviderSelected = "NONE"/);

assert.match(readRepo(
  "src/backend/Modules/HotelBooking/TravelCore.Modules.HotelBooking.Contracts/HotelSourceReadinessContracts.cs",
), /ProductionPaymentProvider = "NONE"/);

assert.match(readRepo(
  "src/backend/Modules/Flight/TravelCore.Modules.Flight.Contracts/FlightOwnershipBoundary.cs",
), /NamedFlightSupplier = "NONE"/);

console.log("provint-cross-module-none-checks: PASS");
