/**
 * UIVAL-T009 Travelogue validation checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const srcRoot = path.join(path.resolve(path.dirname(fileURLToPath(import.meta.url)), ".."), "src");

function read(f) {
  return fs.readFileSync(f, "utf8");
}

function main() {
  const dev = path.join(srcRoot, "app", "[locale]", "dev", "travelogue", "page.tsx");
  assert.ok(fs.existsSync(dev));
  assert.match(read(dev), /TravelogueDetailView/);

  const view = path.join(srcRoot, "features", "travelogue-detail", "travelogue-detail-view.tsx");
  const viewSrc = read(view);
  assert.doesNotMatch(viewSrc, /['"]use client['"]/);
  assert.match(viewSrc, /not editorial Article/i);

  console.log("uival-travelogue-checks: PASS");
}

main();
