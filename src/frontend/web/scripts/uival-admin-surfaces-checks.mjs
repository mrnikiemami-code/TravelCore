/** UIVAL-T014 Admin surfaces checks */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const srcRoot = path.join(path.resolve(path.dirname(fileURLToPath(import.meta.url)), ".."), "src");
const read = (f) => fs.readFileSync(f, "utf8");

const showcase = path.join(srcRoot, "features", "admin-surfaces", "admin-surfaces-showcase.tsx");
assert.match(read(showcase), /admin\/catalog\/tours/);
assert.match(read(showcase), /admin\/media/);
assert.ok(fs.existsSync(path.join(srcRoot, "app", "[locale]", "dev", "admin-surfaces", "page.tsx")));
console.log("uival-admin-surfaces-checks: PASS");
