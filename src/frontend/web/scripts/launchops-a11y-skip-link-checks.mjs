/**
 * LAUNCHOPS-T012 — Skip link a11y on locale layout.
 */
import assert from "node:assert/strict";
import { readSrc } from "./launchops-common.mjs";

const layout = readSrc("app/[locale]/layout.tsx");
assert.match(layout, /SkipLink/);
assert.match(layout, /main-content|hrefId/);

console.log("launchops-a11y-skip-link-checks: PASS");
