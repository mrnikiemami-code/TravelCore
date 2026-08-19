/**
 * PROVINT-T008 — Provider configuration / secrets posture doc.
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { repoRoot } from "./provint-common.mjs";

const doc = path.join(
  repoRoot,
  "docs/plans/PROVINT-T008-provider-configuration-secrets-posture.md",
);
assert.ok(fs.existsSync(doc));
assert.match(fs.readFileSync(doc, "utf8"), /Secrets never in repository/i);

console.log("provint-config-secrets-posture-checks: PASS");
