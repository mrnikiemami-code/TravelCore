/** HOTIDX-T013 — Locale-prefixed internal links. */
import assert from "node:assert/strict";
import { readSrc } from "./hotidx-common.mjs";

const view = readSrc("features/hotel-discovery/hotel-discovery-view.tsx");
assert.match(view, /\/\$\{locale\}\/hotels\//);

console.log("hotidx-locale-prefixed-links-checks: PASS");
