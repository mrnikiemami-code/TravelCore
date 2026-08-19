/** UIVAL-T012 Flight Search checks */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const srcRoot = path.join(path.resolve(path.dirname(fileURLToPath(import.meta.url)), ".."), "src");
const read = (f) => fs.readFileSync(f, "utf8");

assert.ok(fs.existsSync(path.join(srcRoot, "app", "[locale]", "dev", "flight-search", "page.tsx")));
assert.match(read(path.join(srcRoot, "app", "[locale]", "flights", "page.tsx")), /PublicFlightSearchForm/);
console.log("uival-flight-search-checks: PASS");
