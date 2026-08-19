/**
 * UIVAL-T007 Home / Discovery validation checks.
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
  const devRoute = path.join(srcRoot, "app", "[locale]", "dev", "home-discovery", "page.tsx");
  assert.ok(fs.existsSync(devRoute));
  assert.match(read(devRoute), /HomeDiscoveryView/);

  const homeRoute = path.join(srcRoot, "app", "[locale]", "page.tsx");
  assert.ok(fs.existsSync(homeRoute));
  assert.match(read(homeRoute), /PublicShell/);

  const view = path.join(srcRoot, "features", "home-discovery", "home-discovery-view.tsx");
  const viewSrc = read(view);
  assert.doesNotMatch(viewSrc, /['"]use client['"]/);
  assert.match(viewSrc, /\/tours/);
  assert.match(viewSrc, /\/plan/);
  assert.match(viewSrc, /min-h-touch/);

  console.log("uival-home-discovery-checks: PASS");
}

main();
