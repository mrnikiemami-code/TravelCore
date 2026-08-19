/**
 * UIVAL-T004 Tour Listing/Search presentation validation checks.
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
    "tour-listing",
    "page.tsx",
  );
  assert.ok(fs.existsSync(devRoute));
  const devPage = read(devRoute);
  assert.doesNotMatch(devPage, /['"]use client['"]/);
  assert.match(devPage, /PublicTourListingView/);
  assert.match(devPage, /parseListingFilterCriteria/);
  assert.match(devPage, /loadTourListingFixtureSelection/);
  assert.match(devPage, /robots:\s*\{\s*index:\s*false/);

  const prodRoute = path.join(srcRoot, "app", "[locale]", "tours", "page.tsx");
  assert.ok(fs.existsSync(prodRoute));
  assert.match(read(prodRoute), /PublicTourListingView/);
  assert.match(read(prodRoute), /parseListingFilterCriteria/);

  const listingView = path.join(
    srcRoot,
    "features",
    "public-experience",
    "listing-view.tsx",
  );
  assert.doesNotMatch(read(listingView), /['"]use client['"]/);
  assert.match(read(listingView), /ListingFilters/);
  assert.match(read(listingView), /ListingSelection/);
  assert.match(read(listingView), /not a search engine/i);

  const filters = path.join(
    srcRoot,
    "features",
    "public-experience",
    "listing-filters.tsx",
  );
  assert.doesNotMatch(read(filters), /['"]use client['"]/);
  assert.match(read(filters), /method="get"/);
  assert.match(read(filters), /min-h-touch/);

  const filterPresentation = path.join(
    srcRoot,
    "features",
    "public-experience",
    "filter-presentation.ts",
  );
  assert.match(read(filterPresentation), /parseListingFilterCriteria/);
  assert.match(read(filterPresentation), /listingFilterHref/);

  const selection = path.join(
    srcRoot,
    "features",
    "public-experience",
    "listing-selection.tsx",
  );
  assert.match(read(selection), /Presentation sort/);
  assert.match(read(selection), /localeCompare/);

  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "fixtures", "tour-listing", "index.ts")),
  );
  const fixture = read(
    path.join(srcRoot, "lib", "fixtures", "tour-listing", "index.ts"),
  );
  assert.match(fixture, /Experience/);
  assert.match(fixture, /Package/);

  console.log("uival-tour-listing-checks: PASS");
  console.log("  /dev/tour-listing + /tours listing shell: ok");
  console.log("  presentation filters + selection sort: ok");
}

main();
