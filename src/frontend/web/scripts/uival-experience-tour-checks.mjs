/**
 * UIVAL-T003 Experience Tour Detail archetype validation checks.
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
    "experience-tour",
    "page.tsx",
  );
  assert.ok(fs.existsSync(devRoute), "UIVAL experience tour dev route missing");
  const devPage = read(devRoute);
  assert.doesNotMatch(devPage, /['"]use client['"]/);
  assert.match(devPage, /TourDetailView/);
  assert.match(devPage, /loadExperienceTourDetailFixture/);
  assert.match(devPage, /kind !== "Experience"/);
  assert.match(devPage, /robots:\s*\{\s*index:\s*false/);

  const sections = path.join(
    srcRoot,
    "features",
    "public-experience",
    "experience-detail-sections.tsx",
  );
  assert.ok(fs.existsSync(sections));
  const sectionsSrc = read(sections);
  assert.doesNotMatch(sectionsSrc, /['"]use client['"]/);
  assert.match(sectionsSrc, /itineraryDays/);
  assert.match(sectionsSrc, /difficulty/);
  assert.match(sectionsSrc, /equipment/);
  assert.match(sectionsSrc, /LtrValue/);

  const tourView = path.join(
    srcRoot,
    "features",
    "tour-detail",
    "tour-detail-view.tsx",
  );
  assert.match(read(tourView), /ExperienceTourDetailSections/);
  assert.match(read(tourView), /vm\.kind === "Experience"/);

  const sticky = path.join(
    srcRoot,
    "features",
    "public-experience",
    "detail-sticky-actions.tsx",
  );
  assert.ok(fs.existsSync(sticky));
  assert.doesNotMatch(read(sticky).slice(0, 30), /['"]use client['"]/);
  assert.match(read(sticky), /fixed inset-x-0 bottom-0/);
  assert.match(read(sticky), /min-h-touch/);

  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "fixtures", "experience-tour-detail", "fa.ts")),
  );
  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "fixtures", "experience-tour-detail", "en.ts")),
  );

  const faFixture = read(
    path.join(srcRoot, "lib", "fixtures", "experience-tour-detail", "fa.ts"),
  );
  assert.match(faFixture, /kind: "Experience"/);
  assert.match(faFixture, /itineraryDays/);
  assert.match(faFixture, /fixture-daryache-experience/);
  assert.match(faFixture, /Moderate/);

  const loader = read(
    path.join(srcRoot, "lib", "fixtures", "experience-tour-detail", "index.ts"),
  );
  assert.match(loader, /loadExperienceTourDetailFixture/);

  console.log("uival-experience-tour-checks: PASS");
  console.log("  /dev/experience-tour + Experience sections: ok");
  console.log("  fixture fa/en + sticky actions: ok");
}

main();
