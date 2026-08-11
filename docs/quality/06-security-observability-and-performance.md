# Security, Observability, and Performance

منبع: [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md)

---

## 1. Security From the Beginning

Security is an engineering quality concern from day one — not deferred until production penetration testing. A final pen-test cannot compensate for insecure architecture.

### Baseline risks (task-applicable)

authentication · authorization · input validation · secret handling · PII exposure · injection · XSS · CSRF where relevant · SSRF where relevant · file upload · rate abuse · dependency vulnerabilities · provider credential handling

---

## 2. Secret Policy

**Never commit secrets to Git:** DB passwords · API keys · JWT/signing secrets · provider credentials · production connection strings.

Secrets in Git history = serious incident.

### Test secrets

Use test configuration · ephemeral credentials · local/dev secrets · environment variables · appropriate secret stores. Never commit real production credentials.

---

## 3. Test Data / PII

Synthetic/test data only. No production customer data. No real PII copied into fixtures.

---

## 4. Logging Quality

Useful operational context **without** leaking secrets or unnecessary PII.

**Do not log:** passwords · tokens · CVV · authorization headers · provider secrets.

Do not swallow exceptions silently. Expected domain/application failures use semantic handling. Unexpected failures remain diagnosable. Do not expose stack traces to public clients in production.

---

## 5. Observability Direction

Foundation should eventually support: structured logs · correlation IDs · request tracing · metrics where valuable · health checks · error reporting.

Do **not** require verbose logging in every method.

### Health

Distinguish process alive vs critical dependency readiness. Exact liveness/readiness in P01/deployment.

---

## 6. External Providers

Layered testing (see testing strategy). Ordinary CI must not depend on production provider networks. Fixtures avoid secrets/PII. Adapter isolation · timeout/cancel · idempotency · error mapping · degradation.

---

## 7. Performance Quality

Risk-based — not every method needs benchmarks.

High-risk areas may include: search · large listings · page composition · pricing · provider aggregation · bulk imports · image-heavy public pages.

Functional correctness with **obvious severe** performance regression may fail acceptance. Evidence guides optimization; avoid premature micro-optimization.

### Database query quality

Consider query count · N+1 · indexes · projection size · pagination · cardinality. Do not load entire aggregates when a narrow projection suffices.

### Frontend performance

Hydration · client JS · LCP/INP/CLS · images · third-party · SSR — maturity/risk based.

---

## 8. Dependency Policy

New dependencies require justification. Do not add packages for trivial code the platform can own.

Before significant dependency: maintenance · license · security history · ecosystem maturity · bundle/runtime cost · framework compatibility · exit cost.

**No package-by-habit** («every project uses it» / «Cursor prefers it»).

Upgrades intentional; do not mix large unrelated upgrades into feature tasks unless required. Security-critical upgrades may be exceptions.

Frontend lockfiles must stay consistent with manifests — do not hand-edit lockfile semantics to hide mismatch.

---

## 9. Future Scanning Direction

dependency vulnerability · secret scanning · static security analysis · container/image scanning when containers exist.

Exact vendors/tools deferred. Branch protection / CI security gates deferred to later governance.

---

## 10. Anti-Patterns

security deferred to pen-test only · secrets in fixtures · production PII in tests · logging tokens · ignoring dependency risk · CI pinned to live production providers
