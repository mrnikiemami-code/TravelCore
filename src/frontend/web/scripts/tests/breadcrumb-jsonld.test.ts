import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { serializeBreadcrumbJsonLd } from "../../src/lib/seo/structured-data-contract.ts";

describe("breadcrumb JSON-LD consumer (TC-P05-T008)", () => {
  it("serializes truthful BreadcrumbList and omits null item", () => {
    const json = serializeBreadcrumbJsonLd({
      "@context": "https://schema.org",
      "@type": "BreadcrumbList",
      itemListElement: [
        {
          "@type": "ListItem",
          position: 1,
          name: "Turkey",
          item: "/en/destinations/turkey",
        },
        {
          "@type": "ListItem",
          position: 2,
          name: "NoSlug",
          item: null,
        },
      ],
    });

    assert.ok(json);
    const parsed = JSON.parse(json);
    assert.equal(parsed["@type"], "BreadcrumbList");
    assert.equal(parsed.itemListElement[0].item, "/en/destinations/turkey");
    assert.equal(parsed.itemListElement[1].item, undefined);
    assert.doesNotMatch(json, /AggregateRating|Offer|Tour/);
  });

  it("returns null when empty", () => {
    assert.equal(serializeBreadcrumbJsonLd(null), null);
    assert.equal(
      serializeBreadcrumbJsonLd({
        "@context": "https://schema.org",
        "@type": "BreadcrumbList",
        itemListElement: [],
      }),
      null,
    );
  });
});
