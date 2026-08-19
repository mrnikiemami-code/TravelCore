# LAUNCHOPS-T004 — Search Console Operations Runbook

**Status:** Operations guidance (not CI-automated)  
**Related:** SEOVAL-T015 deferral · PRODDEL-T015 deferral

---

## Pre-launch

1. Verify production domain serves `/api/seo/robots.txt` and `/api/seo/sitemap.xml`.
2. Confirm IndexPolicy for public pages before expecting index coverage.
3. Add property in Google Search Console (domain or URL-prefix).

## Post-launch

1. Submit sitemap URL: `https://<host>/api/seo/sitemap.xml`
2. Monitor Coverage / Page indexing reports weekly.
3. Fix crawl errors via SEO redirect/canonical contracts — not frontend hacks.

## Out of scope for repository CI

- Live Search Console API integration
- Automated index monitoring

These remain **production operations** tasks.
