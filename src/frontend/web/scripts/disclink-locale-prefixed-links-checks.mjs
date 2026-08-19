/**
 * DISCLINK-T010 — Locale-prefixed internal travelogue links.
 */
import assert from "node:assert/strict";
import { readSrc } from "./disclink-common.mjs";

const ugc = readSrc("features/public-experience/ugc-composition-list.tsx");
assert.match(ugc, /\$\{locale\}\/travelogues\//);

const discovery = readSrc("features/travelogue-detail/travelogue-discovery-view.tsx");
assert.match(discovery, /\$\{locale\}\/travelogues\//);

console.log("disclink-locale-prefixed-links-checks: PASS");
