/**
 * UIVAL-T001 foundation primitive validation checks — deterministic, no browser.
 * Run via: npm run test:quality
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const webRoot = path.resolve(__dirname, "..");
const srcRoot = path.join(webRoot, "src");

const REQUIRED_UI_EXPORTS = [
  "Container",
  "Stack",
  "Inline",
  "Surface",
  "Text",
  "BidiText",
  "LtrValue",
  "MoneyText",
  "MixedCurrencyPrice",
  "SkipLink",
  "FieldMessage",
  "VisuallyHidden",
  "RouteStatePanel",
  "RouteLoadingSkeleton",
  "NotFoundView",
  "MediaImage",
];

function read(file) {
  return fs.readFileSync(file, "utf8");
}

function walkUi(dir, out = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) continue;
    if (/\.(tsx|ts)$/.test(entry.name) && entry.name !== "index.ts") {
      out.push(full);
    }
  }
  return out;
}

function main() {
  const uiIndex = read(path.join(srcRoot, "components", "ui", "index.ts"));
  for (const name of REQUIRED_UI_EXPORTS) {
    assert.match(uiIndex, new RegExp(`\\b${name}\\b`), `Missing ui export: ${name}`);
  }

  const uiFiles = walkUi(path.join(srcRoot, "components", "ui"));
  for (const file of uiFiles) {
    const rel = path.relative(srcRoot, file);
    const content = read(file);
    assert.doesNotMatch(
      content.slice(0, 200),
      /['"]use client['"]/,
      `components/ui must stay Server Components: ${rel}`,
    );
  }

  const devRoute = path.join(
    srcRoot,
    "app",
    "[locale]",
    "dev",
    "foundation",
    "page.tsx",
  );
  assert.ok(fs.existsSync(devRoute), "UIVAL dev foundation route missing");
  const devPage = read(devRoute);
  assert.doesNotMatch(devPage, /['"]use client['"]/);
  assert.match(devPage, /FoundationPrimitivesShowcase/);
  assert.match(devPage, /robots:\s*\{\s*index:\s*false/);

  const showcase = path.join(
    srcRoot,
    "features",
    "foundation-validation",
    "foundation-primitives-showcase.tsx",
  );
  assert.ok(fs.existsSync(showcase), "FoundationPrimitivesShowcase missing");
  const showcaseSrc = read(showcase);
  assert.doesNotMatch(showcaseSrc, /['"]use client['"]/);
  assert.match(showcaseSrc, /MoneyText/);
  assert.match(showcaseSrc, /MixedCurrencyPrice/);
  assert.match(showcaseSrc, /FieldMessage/);
  assert.match(showcaseSrc, /RouteStatePanel/);
  assert.match(showcaseSrc, /RouteLoadingSkeleton/);
  assert.match(showcaseSrc, /MediaImage/);
  assert.match(showcaseSrc, /LtrValue/);

  const tokens = read(path.join(srcRoot, "styles", "tokens.css"));
  assert.match(tokens, /--tc-color-surface/);
  assert.match(tokens, /--tc-font-size-body/);
  assert.match(tokens, /--tc-space-4/);

  console.log("uival-foundation-checks: PASS");
  console.log(`  ui exports (${REQUIRED_UI_EXPORTS.length}): ok`);
  console.log("  no use client in components/ui: ok");
  console.log("  /dev/foundation route + showcase: ok");
}

main();
