/**
 * Shared paths/helpers for SEOVAL deterministic checks.
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

export const webRoot = path.resolve(__dirname, "..");
export const repoRoot = path.resolve(webRoot, "../../..");
export const srcRoot = path.join(webRoot, "src");
export const seoModuleRoot = path.join(
  repoRoot,
  "src",
  "backend",
  "Modules",
  "Seo",
);
export const seoUnitTestsRoot = path.join(
  repoRoot,
  "tests",
  "Unit",
  "TravelCore.Modules.Seo.UnitTests",
);

export function read(file) {
  return fs.readFileSync(file, "utf8");
}

export function readSrc(rel) {
  return read(path.join(srcRoot, rel));
}

export function readRepo(rel) {
  return read(path.join(repoRoot, rel));
}

export function existsRepo(rel) {
  return fs.existsSync(path.join(repoRoot, rel));
}

export function existsSrc(rel) {
  return fs.existsSync(path.join(srcRoot, rel));
}

export function pagePath(segments) {
  return path.join(srcRoot, "app", "[locale]", ...segments, "page.tsx");
}
