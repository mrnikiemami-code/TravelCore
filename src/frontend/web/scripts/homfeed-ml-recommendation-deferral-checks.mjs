/** HOMFEED-T015 — ML recommendation engine deferral guard. */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./homfeed-common.mjs";

const evidence = path.join(
  repoRoot,
  "docs/plans/HOMFEED-T015-ml-recommendation-deferral-evidence.md",
);
assert.ok(fs.existsSync(evidence));

console.log("homfeed-ml-recommendation-deferral-checks: PASS");
