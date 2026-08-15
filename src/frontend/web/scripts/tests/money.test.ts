import assert from "node:assert/strict";
import { describe, it } from "node:test";
import {
  irrAmountToTomanDisplay,
  resolveMoneyDisplay,
} from "../../src/lib/formatting/money.ts";

describe("money display invariants (T016)", () => {
  it("converts IRR to Toman display by dividing by 10 (not FX)", () => {
    assert.equal(irrAmountToTomanDisplay("119900000"), "11990000");
  });

  it("keeps IRR label when irrDisplayUnit is IRR", () => {
    const resolved = resolveMoneyDisplay(
      { amount: "1000", currencyCode: "IRR" },
      "en",
      "IRR",
    );
    assert.equal(resolved.currencyCode, "IRR");
    assert.match(resolved.unitLabel, /IRR|ریال/);
  });

  it("uses Toman label only when explicitly requested", () => {
    const resolved = resolveMoneyDisplay(
      { amount: "1000", currencyCode: "IRR" },
      "fa",
      "Toman",
    );
    assert.equal(resolved.currencyCode, null);
    assert.equal(resolved.unitLabel, "تومان");
    // 1000 IRR → 100 Toman; locale may use Eastern digits
    assert.equal(irrAmountToTomanDisplay("1000"), "100");
    assert.ok(resolved.amountText.length > 0);
  });

  it("does not change USD amount based on locale", () => {
    const fa = resolveMoneyDisplay(
      { amount: "1290", currencyCode: "USD" },
      "fa",
      "Toman",
    );
    const en = resolveMoneyDisplay(
      { amount: "1290", currencyCode: "USD" },
      "en",
      "IRR",
    );
    assert.equal(fa.currencyCode, "USD");
    assert.equal(en.currencyCode, "USD");
    assert.equal(fa.unitLabel, "USD");
    assert.equal(en.unitLabel, "USD");
  });
});
