/**
 * UIVAL-T010 Visa validation checks.
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
  const dev = path.join(srcRoot, "app", "[locale]", "dev", "visa", "page.tsx");
  assert.ok(fs.existsSync(dev));
  assert.match(read(dev), /VisaDetailView/);

  const prod = path.join(srcRoot, "app", "[locale]", "visas", "[code]", "page.tsx");
  assert.match(read(prod), /VisaDetailView/);

  const view = path.join(srcRoot, "features", "visa-detail", "visa-detail-view.tsx");
  assert.match(read(view), /requirementSets/);
  assert.match(read(view), /MoneyText/);

  assert.ok(fs.existsSync(path.join(srcRoot, "lib", "fixtures", "visa-detail", "fa.ts")));

  console.log("uival-visa-checks: PASS");
}

main();
