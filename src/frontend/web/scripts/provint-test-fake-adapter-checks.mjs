/**
 * PROVINT-T012 — Test-only fake adapter conventions.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, readRepo } from "./provint-common.mjs";

const fake = path.join(
  repoRoot,
  "tests/Unit/TravelCore.Modules.Payment.UnitTests/Fakes/FakePaymentProviderGateway.cs",
);
assert.ok(fs.existsSync(fake));
assert.match(fs.readFileSync(fake, "utf8"), /FakePaymentProviderGateway/);

const moduleSrc = readRepo(
  "src/backend/Modules/Payment/TravelCore.Modules.Payment.Infrastructure/PaymentModule.cs",
);
assert.doesNotMatch(moduleSrc, /FakePaymentProviderGateway/);

console.log("provint-test-fake-adapter-checks: PASS");
