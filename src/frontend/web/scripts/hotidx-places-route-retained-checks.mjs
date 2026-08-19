/** HOTIDX-T014 — Places route retained alongside hotels. */
import assert from "node:assert/strict";
import { existsSrc } from "./hotidx-common.mjs";

assert.ok(existsSrc("app/[locale]/places/[slug]/page.tsx"));
assert.ok(existsSrc("app/[locale]/hotels/[slug]/page.tsx"));

console.log("hotidx-places-route-retained-checks: PASS");
