import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { languagesFromHreflang } from "../../src/lib/seo/hreflang-contract.ts";

describe("hreflang metadata consumer (TC-P05-T006)", () => {
  it("maps FA/EN published pair to languages", () => {
    const languages = languagesFromHreflang({
      resourceType: "Destination",
      resourceId: "0198a000-0000-7000-8000-000000000501",
      alternates: [
        {
          locale: "en",
          path: "destinations/istanbul",
          href: "/en/destinations/istanbul",
        },
        {
          locale: "fa",
          path: "destinations/استانبول",
          href: "/fa/destinations/استانبول",
        },
      ],
    });

    assert.equal(languages.en, "/en/destinations/istanbul");
    assert.equal(languages.fa, "/fa/destinations/استانبول");
  });

  it("omits missing locales and does not fabricate", () => {
    const languages = languagesFromHreflang({
      resourceType: "Destination",
      resourceId: "0198a000-0000-7000-8000-000000000502",
      alternates: [
        {
          locale: "en",
          path: "destinations/istanbul",
          href: "/en/destinations/istanbul",
        },
      ],
    });

    assert.equal(Object.keys(languages).length, 1);
    assert.equal(languages.en, "/en/destinations/istanbul");
    assert.equal(languages.fa, undefined);
  });

  it("returns empty when bindings absent", () => {
    assert.deepEqual(languagesFromHreflang(null), {});
    assert.deepEqual(languagesFromHreflang(undefined), {});
    assert.deepEqual(
      languagesFromHreflang({
        resourceType: "Destination",
        resourceId: "x",
        alternates: [],
      }),
      {},
    );
  });
});
