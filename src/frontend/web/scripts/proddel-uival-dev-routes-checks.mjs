/**
 * PRODDEL-T014 — UIVAL dev routes retained with noindex.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { srcRoot } from "./proddel-common.mjs";

const devRoot = path.join(srcRoot, "app", "[locale]", "dev");
const routes = fs.readdirSync(devRoot, { withFileTypes: true })
  .filter((e) => e.isDirectory())
  .map((e) => e.name);

assert.ok(routes.length >= 15, "expected UIVAL dev routes");

for (const route of routes) {
  const page = path.join(devRoot, route, "page.tsx");
  const src = fs.readFileSync(page, "utf8");
  assert.match(
    src,
    /robots:\s*\{\s*index:\s*false|index:\s*false,\s*follow|metadata.*index:\s*false/s,
    `dev route noindex: ${route}`,
  );
}

console.log("proddel-uival-dev-routes-checks: PASS");
console.log(`  dev routes (${routes.length}): ok`);
