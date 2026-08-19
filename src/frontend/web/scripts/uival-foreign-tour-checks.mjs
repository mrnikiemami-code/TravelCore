/**
 * UIVAL-T002 Foreign Package Tour Detail archetype validation checks.
 * Run via: npm run test:quality
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const webRoot = path.resolve(__dirname, "..");
const srcRoot = path.join(webRoot, "src");

function read(file) {
  return fs.readFileSync(file, "utf8");
}

function main() {
  const devRoute = path.join(
    srcRoot,
    "app",
    "[locale]",
    "dev",
    "foreign-tour",
    "page.tsx",
  );
  assert.ok(fs.existsSync(devRoute), "UIVAL foreign tour dev route missing");
  const devPage = read(devRoute);
  assert.doesNotMatch(devPage, /['"]use client['"]/);
  assert.match(devPage, /ForeignTourDetailView/);
  assert.match(devPage, /loadForeignTourDetailFixture/);
  assert.match(devPage, /robots:\s*\{\s*index:\s*false/);

  const view = path.join(
    srcRoot,
    "features",
    "foreign-tour-detail",
    "foreign-tour-detail-view.tsx",
  );
  assert.ok(fs.existsSync(view));
  const viewSrc = read(view);
  assert.doesNotMatch(viewSrc, /['"]use client['"]/);
  assert.match(viewSrc, /MediaImage/);
  assert.match(viewSrc, /MixedCurrencyPrice/);
  assert.match(viewSrc, /LtrValue/);
  assert.match(viewSrc, /BookingCtaIsland/);
  assert.match(viewSrc, /commercialStatus/);

  const ctaIsland = path.join(
    srcRoot,
    "features",
    "foreign-tour-detail",
    "booking-cta-island.tsx",
  );
  assert.ok(fs.existsSync(ctaIsland));
  assert.match(read(ctaIsland).slice(0, 30), /['"]use client['"]/);
  assert.match(read(ctaIsland), /sticky bottom-0/);
  assert.match(read(ctaIsland), /min-h-touch/);

  assert.ok(
    fs.existsSync(path.join(srcRoot, "types", "pages", "foreign-tour-detail.ts")),
  );
  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "fixtures", "foreign-tour-detail", "fa.ts")),
  );
  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "fixtures", "foreign-tour-detail", "en.ts")),
  );

  const faFixture = read(
    path.join(srcRoot, "lib", "fixtures", "foreign-tour-detail", "fa.ts"),
  );
  assert.match(faFixture, /fixture-istanbul-package/);
  assert.match(faFixture, /IKA/);
  assert.match(faFixture, /USD/);

  console.log("uival-foreign-tour-checks: PASS");
  console.log("  /dev/foreign-tour route + ForeignTourDetailView: ok");
  console.log("  fixture fa/en + sticky CTA island: ok");
}

main();
