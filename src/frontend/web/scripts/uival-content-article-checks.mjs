/**
 * UIVAL-T008 Content Article validation checks.
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
  const dev = path.join(srcRoot, "app", "[locale]", "dev", "content-article", "page.tsx");
  assert.ok(fs.existsSync(dev));
  assert.match(read(dev), /ContentDetailView/);
  assert.match(read(dev), /kind !== "Article"/);

  const prod = path.join(srcRoot, "app", "[locale]", "articles", "[slug]", "page.tsx");
  assert.match(read(prod), /loadContentDetailPage/);

  const view = path.join(srcRoot, "features", "content-detail", "content-detail-view.tsx");
  assert.match(read(view), /blocks\.map/);

  const fa = read(path.join(srcRoot, "lib", "fixtures", "content-article", "fa.ts"));
  assert.match(fa, /kind: "Article"/);
  assert.match(fa, /kind: "Cta"/);

  console.log("uival-content-article-checks: PASS");
}

main();
