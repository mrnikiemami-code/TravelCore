/** UIVAL-T015 Agency surfaces checks */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const srcRoot = path.join(path.resolve(path.dirname(fileURLToPath(import.meta.url)), ".."), "src");
const read = (f) => fs.readFileSync(f, "utf8");

assert.ok(fs.existsSync(path.join(srcRoot, "app", "[locale]", "dev", "agency-surfaces", "page.tsx")));
assert.match(read(path.join(srcRoot, "app", "[locale]", "agency", "page.tsx")), /Agency Marketplace/);
console.log("uival-agency-surfaces-checks: PASS");
