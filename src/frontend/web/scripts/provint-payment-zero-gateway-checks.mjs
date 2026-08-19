/**
 * PROVINT-T002 — Payment module registers zero production gateways.
 */
import assert from "node:assert/strict";
import { readRepo } from "./provint-common.mjs";

const moduleSrc = readRepo(
  "src/backend/Modules/Payment/TravelCore.Modules.Payment.Infrastructure/PaymentModule.cs",
);
assert.doesNotMatch(moduleSrc, /AddSingleton<IPaymentProviderGateway/);
assert.match(moduleSrc, /IPaymentProviderResolver/);

console.log("provint-payment-zero-gateway-checks: PASS");
