/**
 * DISCLINK-T014 — Hotel catalog browse index deferral guard (historical at DISCLINK gate).
 * HOTIDX may supersede at product level; evidence documents original deferral.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./disclink-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/DISCLINK-T014-hotel-catalog-index-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("disclink-hotel-catalog-index-deferral-checks: PASS");
