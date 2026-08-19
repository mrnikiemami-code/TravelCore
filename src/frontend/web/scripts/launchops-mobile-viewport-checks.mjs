/**
 * LAUNCHOPS-T013 — Mobile viewport on root layout.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { srcRoot } from "./launchops-common.mjs";

const layoutCandidates = [
  path.join(srcRoot, "app", "layout.tsx"),
  path.join(srcRoot, "app", "[locale]", "layout.tsx"),
];

let foundViewport = false;
for (const file of layoutCandidates) {
  if (!fs.existsSync(file)) continue;
  const src = fs.readFileSync(file, "utf8");
  if (/viewport|min-h-full|mobile/i.test(src)) {
    foundViewport = true;
  }
}

assert.ok(foundViewport, "mobile-friendly layout signals missing");

const localeLayout = fs.readFileSync(
  path.join(srcRoot, "app", "[locale]", "layout.tsx"),
  "utf8",
);
assert.match(localeLayout, /min-h-full/);

console.log("launchops-mobile-viewport-checks: PASS");
