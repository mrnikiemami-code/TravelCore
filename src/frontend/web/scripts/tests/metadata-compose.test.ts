import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { languagesFromHreflang } from "../../src/lib/seo/hreflang-contract.ts";
import { robotsFromIndexability } from "../../src/lib/seo/indexability-contract.ts";

describe("composed metadata consumer (TC-P05-T007)", () => {
  it("maps robots from SEO indexability (R2 default noindex)", () => {
    const robots = robotsFromIndexability({
      locale: "en",
      path: "destinations/istanbul",
      effectiveIndex: "NoIndex",
      effectiveFollow: "Follow",
      robotsDirective: "noindex, follow",
      configuredIndex: null,
      configuredFollow: null,
      isIndexable: false,
      reasons: ["missing-policy"],
    });
    assert.equal(robots.index, false);
    assert.equal(robots.follow, true);
  });

  it("maps hreflang languages without fabricating", () => {
    const languages = languagesFromHreflang({
      resourceType: "Destination",
      resourceId: "x",
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

  it("honors Index when SEO evaluation says Index", () => {
    const robots = robotsFromIndexability({
      locale: "en",
      path: "destinations/istanbul",
      effectiveIndex: "Index",
      effectiveFollow: "Follow",
      robotsDirective: "index, follow",
      configuredIndex: "Index",
      configuredFollow: "Follow",
      isIndexable: true,
      reasons: [],
    });
    assert.equal(robots.index, true);
    assert.equal(robots.follow, true);
  });
});
