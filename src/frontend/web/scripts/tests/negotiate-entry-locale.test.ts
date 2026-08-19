import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { negotiateEntryLocale } from "../../src/lib/i18n/negotiate-entry-locale.ts";

describe("root entry locale negotiation (LAUNCHOPS-T001)", () => {
  it("defaults to fa when Accept-Language is missing", () => {
    assert.equal(negotiateEntryLocale(null), "fa");
    assert.equal(negotiateEntryLocale(undefined), "fa");
    assert.equal(negotiateEntryLocale(""), "fa");
  });

  it("prefers highest-q supported locale tag", () => {
    assert.equal(negotiateEntryLocale("en-US,en;q=0.9,fa;q=0.8"), "en");
    assert.equal(negotiateEntryLocale("ar-SA,ar;q=0.9,en;q=0.5"), "ar");
  });

  it("matches base language subtags", () => {
    assert.equal(negotiateEntryLocale("en-GB"), "en");
    assert.equal(negotiateEntryLocale("fa-IR"), "fa");
  });

  it("falls back to product default for unsupported languages", () => {
    assert.equal(negotiateEntryLocale("de-DE,de;q=0.9"), "fa");
  });
});
