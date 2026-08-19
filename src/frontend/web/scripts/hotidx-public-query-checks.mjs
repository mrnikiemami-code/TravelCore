/** HOTIDX-T002 — Place public hotel browse query. */
import assert from "node:assert/strict";
import { readRepo } from "./hotidx-common.mjs";

const query = readRepo(
  "src/backend/Modules/Place/TravelCore.Modules.Place.Infrastructure/Services/PlacePublicQuery.cs",
);
assert.match(query, /ListByLocaleAsync/);
assert.match(query, /PlaceCatalogStatus\.Active/);
assert.match(query, /PlaceKind\.Hotel/);

console.log("hotidx-public-query-checks: PASS");
