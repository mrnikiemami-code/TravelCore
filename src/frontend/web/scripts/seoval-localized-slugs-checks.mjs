/**
 * SEOVAL-T004 — Localized slug ownership and dynamic route checks.
 */
import assert from "node:assert/strict";
import { pagePath, read } from "./seoval-common.mjs";

const SLUG_PAGES = [
  {
    segments: ["destinations", "[slug]"],
    seoMarker: "public path publication lives in SEO",
    ownerMarker: "Destination remains content SoR",
  },
  {
    segments: ["tours", "[slug]"],
    seoMarker: "SEO owns route binding",
    ownerMarker: "Tour translation owns current slug",
  },
  {
    segments: ["places", "[slug]"],
    seoMarker: "SEO owns route binding",
    ownerMarker: "Place owns current slug",
  },
  {
    segments: ["articles", "[slug]"],
    seoMarker: "SEO owns route binding",
    ownerMarker: "Content owns current slug",
  },
  {
    segments: ["landing-pages", "[slug]"],
    seoMarker: "SEO owns route binding",
    ownerMarker: "Content owns current slug",
  },
];

for (const { segments, seoMarker, ownerMarker } of SLUG_PAGES) {
  const file = pagePath(segments);
  const src = read(file);
  assert.match(src, /slug/, `dynamic slug route: ${segments.join("/")}`);
  assert.ok(src.includes(seoMarker), `${segments.join("/")} SEO ownership comment`);
  assert.ok(src.includes(ownerMarker), `${segments.join("/")} slug ownership comment`);
}

console.log("seoval-localized-slugs-checks: PASS");
console.log(`  slug pages validated (${SLUG_PAGES.length}): ok`);
