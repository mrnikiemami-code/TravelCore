/**
 * LAUNCHOPS-T007 — Emergency recovery prompt readiness.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./launchops-common.mjs";

const recovery = path.join(
  repoRoot,
  "docs/prompts/START-HERE-IF-CHATGPT-IS-LOST.md",
);
assert.ok(fs.existsSync(recovery));

const state = fs.readFileSync(path.join(repoRoot, "docs/PROJECT-STATE.md"), "utf8");
assert.match(state, /Emergency ChatGPT Recovery/);

console.log("launchops-recovery-readiness-checks: PASS");
