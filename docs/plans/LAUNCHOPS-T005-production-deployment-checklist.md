# LAUNCHOPS-T005 — Production Deployment Checklist

**Status:** Operations checklist (manual verification)

---

## Build & quality

- [ ] `npm run quality` PASS in `src/frontend/web`
- [ ] `dotnet build TravelCore.sln` PASS
- [ ] Architecture + module unit tests PASS

## Infrastructure

- [ ] PostgreSQL migrations applied per module
- [ ] Environment secrets configured (no secrets in repo)
- [ ] Health: `/health/live` and `/health/ready` respond

## SEO / public web

- [ ] Locale-prefixed routes reachable (`/fa`, `/en`, `/ar`)
- [ ] Root `/` redirects via Accept-Language negotiation to locale prefix
- [ ] `robots.txt` and `sitemap.xml` reachable via SEO API

## Deferred to ops (not blocking repo acceptance)

- Live Search Console verification
- Full browser E2E farm
- CDN / WAF vendor configuration
