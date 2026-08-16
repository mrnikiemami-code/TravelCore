/**
 * Deterministic P02 quality checks (T016) — no browser farm.
 * Run via: npm run test:quality
 */
import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const webRoot = path.resolve(__dirname, "..");
const srcRoot = path.join(webRoot, "src");

const ALLOWED_USE_CLIENT = new Set([
  path.normalize("features/foreign-tour-detail/booking-cta-island.tsx"),
  path.normalize("features/admin-identity-party/identity-party-workflow-island.tsx"),
  path.normalize(
    "features/admin-destination-hierarchy/destination-hierarchy-workflow-island.tsx",
  ),
  path.normalize(
    "features/admin-destination-seo/destination-seo-posture-panel.tsx",
  ),
  path.normalize(
    "features/admin-reference-data/reference-data-browse-island.tsx",
  ),
  path.normalize("features/admin-media/media-workflow-island.tsx"),
  path.normalize("features/admin-place/place-workflow-island.tsx"),
  path.normalize("app/[locale]/error.tsx"),
]);

function walk(dir, out = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      if (entry.name === "node_modules" || entry.name === ".next") continue;
      walk(full, out);
    } else if (/\.(tsx|ts|jsx|js)$/.test(entry.name)) {
      out.push(full);
    }
  }
  return out;
}

function relSrc(file) {
  return path.normalize(path.relative(srcRoot, file));
}

function read(file) {
  return fs.readFileSync(file, "utf8");
}

function main() {
  const files = walk(srcRoot);

  // 1) Server Component / Client boundary allowlist
  const clientFiles = files.filter((f) => {
    const head = read(f).slice(0, 200);
    return /['"]use client['"]/.test(head);
  });
  for (const f of clientFiles) {
    const rel = relSrc(f);
    assert.ok(
      ALLOWED_USE_CLIENT.has(rel),
      `Unexpected "use client" file: ${rel}`,
    );
  }
  for (const required of ALLOWED_USE_CLIENT) {
    assert.ok(
      clientFiles.some((f) => relSrc(f) === required),
      `Expected intentional client boundary missing: ${required}`,
    );
  }

  // 2) Locale route invariants
  assert.ok(fs.existsSync(path.join(srcRoot, "app", "[locale]", "page.tsx")));
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "app", "[locale]", "tours", "[productKey]", "page.tsx"),
    ),
  );
  const tourPage = read(
    path.join(srcRoot, "app", "[locale]", "tours", "[productKey]", "page.tsx"),
  );
  assert.match(tourPage, /generateMetadata/);
  assert.doesNotMatch(tourPage, /['"]use client['"]/);

  // 3) FA/EN published fixtures; AR not fabricated as published fixture
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "lib", "fixtures", "foreign-tour-detail", "fa.ts"),
    ),
  );
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "lib", "fixtures", "foreign-tour-detail", "en.ts"),
    ),
  );
  assert.equal(
    fs.existsSync(
      path.join(srcRoot, "lib", "fixtures", "foreign-tour-detail", "ar.ts"),
    ),
    false,
  );
  const loader = read(
    path.join(srcRoot, "lib", "fixtures", "foreign-tour-detail", "index.ts"),
  );
  assert.match(loader, /loadForeignTourDetailFixture/);
  assert.match(loader, /No ForeignTourDetail fixture published for locale/);

  // 4) RTL/LTR: locale layout sets dir from registry
  const layout = read(path.join(srcRoot, "app", "[locale]", "layout.tsx"));
  assert.match(layout, /getHtmlDir/);
  assert.match(layout, /dir=\{dir\}/);
  assert.match(layout, /lang=\{lang\}/);

  // 5) Accessibility baseline artifacts present
  assert.ok(fs.existsSync(path.join(srcRoot, "components", "ui", "skip-link.tsx")));
  assert.ok(
    fs.existsSync(path.join(srcRoot, "components", "ui", "field-message.tsx")),
  );

  // 6) Image foundation present; next.config has no remote wildcard / storage-provider hosts
  assert.ok(fs.existsSync(path.join(srcRoot, "components", "ui", "media-image.tsx")));
  assert.ok(fs.existsSync(path.join(srcRoot, "lib", "media", "media-presentation.ts")));
  const nextConfig = read(path.join(webRoot, "next.config.ts"));
  assert.doesNotMatch(nextConfig, /hostname:\s*['"]\*['"]/);
  assert.doesNotMatch(nextConfig, /remotePatterns:\s*\[[^\]]*['"]\*\*\/*/);
  assert.doesNotMatch(nextConfig, /amazonaws\.com|r2\.cloudflarestorage|minio/i);
  assert.match(nextConfig, /\/api\/media\/\*\*/);

  // 7) Money / Media contracts exist
  assert.ok(fs.existsSync(path.join(srcRoot, "types", "money.ts")));
  assert.ok(fs.existsSync(path.join(srcRoot, "types", "media-image.ts")));
  assert.ok(
    fs.existsSync(path.join(srcRoot, "types", "pages", "foreign-tour-detail.ts")),
  );

  // 8) P03 T010 guided Admin Identity↔Party workflow routes (job-based, not silo CRUD)
  assert.ok(
    fs.existsSync(path.join(srcRoot, "app", "[locale]", "admin", "accounts", "page.tsx")),
  );
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "app", "[locale]", "admin", "accounts", "onboard", "page.tsx"),
    ),
  );
  const onboard = read(
    path.join(srcRoot, "app", "[locale]", "admin", "accounts", "onboard", "page.tsx"),
  );
  assert.doesNotMatch(onboard, /['"]use client['"]/);
  assert.match(onboard, /IdentityPartyWorkflowIsland/);
  assert.match(onboard, /robots/);

  // 9) P04 T008 guided Admin Destination hierarchy workflow (job-based catalog)
  assert.ok(
    fs.existsSync(path.join(srcRoot, "app", "[locale]", "admin", "catalog", "page.tsx")),
  );
  assert.ok(
    fs.existsSync(
      path.join(
        srcRoot,
        "app",
        "[locale]",
        "admin",
        "catalog",
        "destinations",
        "page.tsx",
      ),
    ),
  );
  const destinationsPage = read(
    path.join(
      srcRoot,
      "app",
      "[locale]",
      "admin",
      "catalog",
      "destinations",
      "page.tsx",
    ),
  );
  assert.doesNotMatch(destinationsPage, /['"]use client['"]/);
  assert.match(destinationsPage, /DestinationHierarchyWorkflowIsland/);
  assert.match(destinationsPage, /robots/);

  // 10) P04 T009 public Destination landing (R3 noindex,follow; Server Component)
  const publicDestinationPage = path.join(
    srcRoot,
    "app",
    "[locale]",
    "destinations",
    "[slug]",
    "page.tsx",
  );
  assert.ok(fs.existsSync(publicDestinationPage));
  const publicDest = read(publicDestinationPage);
  assert.doesNotMatch(publicDest, /['"]use client['"]/);
  assert.match(publicDest, /generateMetadata/);
  assert.match(publicDest, /loadComposedSeoMetadata/);
  assert.match(publicDest, /robotsFromComposed/);
  assert.match(publicDest, /loadSeoBreadcrumbJsonLd/);
  assert.match(publicDest, /application\/ld\+json/);
  assert.match(publicDest, /loadDestinationLandingPage/);
  assert.match(publicDest, /PublicShell/);
  // T007: SEO-aware composition required (robots may still noindex via R2 evaluation)
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "types", "pages", "destination-landing.ts"),
    ),
  );
  assert.ok(
    fs.existsSync(
      path.join(
        srcRoot,
        "features",
        "destination-landing",
        "destination-landing-view.tsx",
      ),
    ),
  );
  const landingView = read(
    path.join(
      srcRoot,
      "features",
      "destination-landing",
      "destination-landing-view.tsx",
    ),
  );
  assert.doesNotMatch(landingView, /['"]use client['"]/);
  assert.doesNotMatch(landingView, /\/destinations\/\$\{[^}]*id/);
  // T007: composed metadata may include canonical + languages from SEO
  assert.match(publicDest, /loadComposedSeoMetadata/);
  assert.match(publicDest, /languagesFromComposed|canonicalHref/);
  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "seo", "hreflang-contract.ts")),
  );
  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "seo", "load-hreflang.ts")),
  );
  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "seo", "metadata-contract.ts")),
  );
  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "seo", "load-composed-metadata.ts")),
  );
  assert.ok(
    fs.existsSync(path.join(srcRoot, "lib", "seo", "indexability-contract.ts")),
  );
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "lib", "seo", "structured-data-contract.ts"),
    ),
  );
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "lib", "seo", "load-breadcrumb-jsonld.ts"),
    ),
  );

  // 11) P07 T007 public Place detail (R5 default noindex,follow; Server Component)
  const publicPlacePage = path.join(
    srcRoot,
    "app",
    "[locale]",
    "places",
    "[slug]",
    "page.tsx",
  );
  assert.ok(fs.existsSync(publicPlacePage));
  const publicPlace = read(publicPlacePage);
  assert.doesNotMatch(publicPlace, /['"]use client['"]/);
  assert.match(publicPlace, /generateMetadata/);
  assert.match(publicPlace, /loadComposedSeoMetadata/);
  assert.match(publicPlace, /robotsFromComposed/);
  assert.match(publicPlace, /loadSeoBreadcrumbJsonLd/);
  assert.match(publicPlace, /application\/ld\+json/);
  assert.match(publicPlace, /loadPlaceDetailPage/);
  assert.match(publicPlace, /PublicShell/);
  assert.match(publicPlace, /places\/\$\{/);
  assert.ok(
    fs.existsSync(path.join(srcRoot, "types", "pages", "place-detail.ts")),
  );
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "features", "place-detail", "place-detail-view.tsx"),
    ),
  );
  const placeView = read(
    path.join(srcRoot, "features", "place-detail", "place-detail-view.tsx"),
  );
  assert.doesNotMatch(placeView, /['"]use client['"]/);
    assert.doesNotMatch(placeView, /\/places\/\$\{[^}]*id/);
  assert.doesNotMatch(placeView, /\bstorageKey\b/i);
  assert.doesNotMatch(placeView, /\bHero\b/);

  // 12) Admin catalog places workflow island remains intentional client boundary
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "app", "[locale]", "admin", "catalog", "places", "page.tsx"),
    ),
  );
  // 11) P04 T010 ReferenceData Admin read surface (job catalog, not silo CMS)
  const referencePage = path.join(
    srcRoot,
    "app",
    "[locale]",
    "admin",
    "catalog",
    "reference",
    "page.tsx",
  );
  assert.ok(fs.existsSync(referencePage));
  const reference = read(referencePage);
  assert.doesNotMatch(reference, /['"]use client['"]/);
  assert.match(reference, /ReferenceDataBrowseIsland/);
  assert.match(reference, /robots/);
  assert.ok(
    fs.existsSync(
      path.join(srcRoot, "features", "admin-reference-data", "actions.ts"),
    ),
  );

  console.log("p02-quality-checks: PASS");
  console.log(`  use client allowlist (${ALLOWED_USE_CLIENT.size}): ok`);
  console.log(
    "  locale/tour/fixtures/metadata/a11y/media/money/admin-workflow/destination-public/reference-data: ok",
  );
}

main();
