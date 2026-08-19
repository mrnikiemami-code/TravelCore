/**
 * UIVAL-T006 Hotel Detail (Place catalog) validation checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const srcRoot = path.join(path.resolve(path.dirname(fileURLToPath(import.meta.url)), ".."), "src");

function read(file) {
  return fs.readFileSync(file, "utf8");
}

function main() {
  const devRoute = path.join(srcRoot, "app", "[locale]", "dev", "hotel-detail", "page.tsx");
  assert.ok(fs.existsSync(devRoute));
  assert.match(read(devRoute), /PlaceDetailView/);
  assert.match(read(devRoute), /loadHotelDetailFixture/);
  assert.match(read(devRoute), /kind !== "Hotel"/);

  const prodRoute = path.join(srcRoot, "app", "[locale]", "places", "[slug]", "page.tsx");
  assert.match(read(prodRoute), /PlaceDetailView/);

  const view = path.join(srcRoot, "features", "place-detail", "place-detail-view.tsx");
  const viewSrc = read(view);
  assert.doesNotMatch(viewSrc, /['"]use client['"]/);
  assert.match(viewSrc, /hotelStarRating/);
  assert.match(viewSrc, /kind === "Hotel"/);
  assert.match(viewSrc, /\/book/);
  assert.match(viewSrc, /LtrValue/);

  const faFixture = read(path.join(srcRoot, "lib", "fixtures", "hotel-detail", "fa.ts"));
  assert.match(faFixture, /kind: "Hotel"/);
  assert.match(faFixture, /hotelStarRating: 4/);

  console.log("uival-hotel-detail-checks: PASS");
}

main();
