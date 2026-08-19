/**
 * DISCLINK-T014 — Hotel catalog browse index deferral guard.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot, pagePath } from "./disclink-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/DISCLINK-T014-hotel-catalog-index-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));
assert.ok(!fs.existsSync(pagePath(["hotels"])), "hotels index page must remain deferred");

console.log("disclink-hotel-catalog-index-deferral-checks: PASS");
