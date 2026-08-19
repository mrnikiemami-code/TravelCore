/**
 * UIVAL-T005 Destination Landing validation checks.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const srcRoot = path.join(path.resolve(__dirname, ".."), "src");

function read(file) {
  return fs.readFileSync(file, "utf8");
}

function main() {
  const devRoute = path.join(
    srcRoot,
    "app",
    "[locale]",
    "dev",
    "destination-landing",
    "page.tsx",
  );
  assert.ok(fs.existsSync(devRoute));
  const devPage = read(devRoute);
  assert.match(devPage, /DestinationLandingView/);
  assert.match(devPage, /loadDestinationLandingFixture/);
  assert.match(devPage, /robots:\s*\{\s*index:\s*false/);

  const prodRoute = path.join(
    srcRoot,
    "app",
    "[locale]",
    "destinations",
    "[slug]",
    "page.tsx",
  );
  assert.match(read(prodRoute), /DestinationLandingView/);
  assert.match(read(prodRoute), /generateMetadata/);

  const view = path.join(
    srcRoot,
    "features",
    "destination-landing",
    "destination-landing-view.tsx",
  );
  assert.doesNotMatch(read(view), /['"]use client['"]/);
  assert.match(read(view), /breadcrumb/);
  assert.match(read(view), /LtrValue/);
  assert.match(read(view), /min-h-touch/);

  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "fixtures", "destination-landing", "fa.ts")),
  );

  console.log("uival-destination-landing-checks: PASS");
}

main();
