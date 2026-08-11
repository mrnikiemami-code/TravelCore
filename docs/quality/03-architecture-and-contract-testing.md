# Architecture and Contract Testing

منبع: [`../architecture/14-engineering-quality-constitution.md`](../architecture/14-engineering-quality-constitution.md)  
ADR: [`../adr/0012-automated-architecture-guardrails.md`](../adr/0012-automated-architecture-guardrails.md) (Proposed)

---

## 1. Purpose

Automatically enforce high-value **structural** rules where mechanically practical.

Architecture tests are **safety rails**, not a replacement for architecture review. A green suite cannot prove all architectural decisions are correct.

---

## 2. Priority Guardrails (Candidates)

Future architecture tests should aim to detect:

| Guardrail | Intent |
|-----------|--------|
| Domain ↛ Infrastructure | Domain purity |
| Domain ↛ ASP.NET Core / EF Core / UI frameworks | No framework leakage into Domain |
| No cross-module persistence dependencies | e.g. Tour ↛ Place.Persistence / Pricing.Persistence |
| No cross-module DbContext access | Accepted module boundary |
| No cross-module EF entity navigation | No hidden joins across ownership |
| Presentation ↛ Domain dependency inversion break | Presentation is not Domain |
| Search / SEO / Notification not required for core business correctness | Platform capabilities stay derived/supportive |
| No reference to another module's internal implementation | Public contracts only |
| Frontend route code ↛ server persistence | Boundary |
| No giant TravelCoreDbContext | Schema-per-module direction |
| No NameFa/NameEn/NameAr entity pattern where detectable | i18n data architecture |

Exact project/package graphs established in P01+.

---

## 3. Architecture Drift Examples

Detect/prevent:

- new global DbContext  
- cross-module EF navigation  
- language-specific Name columns  
- raw provider ID as primary domain ID  
- inventing `TOMAN` as currency code contrary to Money ADR  
- business rules authoritative only in frontend  
- SEO route logic duplicated across business modules  
- all pages converted to Client Components  
- page composition via cross-module DbContext  

Mechanically detectable forms should fail the Architecture quality gate.

---

## 4. Module Dependency Testing

Tour may consume **approved contracts**, but must not directly depend on another module's persistence implementation. Forbidden dependency direction should fail CI/architecture gate once tooling exists.

---

## 5. Contract Testing

Where contract breakage risk exists:

- frontend ↔ backend public APIs  
- inter-module application contracts  
- external provider adapters  

Protect **semantic** compatibility. Snapshots may help structured contracts / rendering / serialization but:

- review every large snapshot change  
- snapshot change ≠ automatic correctness  
- do not use snapshots alone as sole contract validation  

---

## 6. API Compatibility Direction

Public/external API changes must consider compatibility. Breaking risks include:

renaming/removing fields · semantic meaning change · nullability change · enum/code meaning change · URL behavior change.

Exact versioning strategy deferred. Unexpected breaking change without Task/architect intent → FAIL.

---

## 7. Accepted-Document Integrity

Accepted architecture documents are protected.

If an implementation task modifies an accepted architecture doc:

1. Classify: SAFE EXTENSION vs ACCEPTED ARCHITECTURE MODIFICATION vs UNRELATED  
2. Safe traceability/editorial extension may be reported  
3. Changed architectural rule → architect review and potentially ADR  

Do not silently rewrite accepted decisions. Cursor cannot convert Proposed → Accepted without architect instruction.

---

## 8. Relationship to Review

| Mechanism | Role |
|-----------|------|
| Architecture tests | Catch detectable structural drift |
| Hermes / peer review | Independent judgment on risk |
| Chief Architect | Accept ADRs; resolve conflicts |

Passing tests + wrong decision still requires architect process.
