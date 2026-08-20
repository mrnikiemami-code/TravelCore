# TC-PIPELINE-COMPLIANCE-LEDGER-CORRECTION-01

| Field | Value |
|-------|--------|
| Task-ID | `TC-PIPELINE-COMPLIANCE-LEDGER-CORRECTION-01` |
| Task-Type | GOVERNANCE CORRECTION ONLY |
| Phase | Post-P29 Continuous Evolution |
| Related disclosure | `PIPELINE-COMPLIANCE-DISCLOSURE-01` |
| Related forensic | `TC-PIPELINE-COMPLIANCE-FORENSIC-01` |
| Product code changed | NO |

**Rule:** Cursor PASS / implementer commit ≠ Architect Acceptance. Acceptance below is **architect retroactive acceptance after forensic review**.

---

## 1. Incident

Unauthorized out-of-pipeline execution occurred after DISCLINK close.

| Field | Value |
|-------|--------|
| Range | `38604d3..4094697` |
| Last authorized product-track close | `38604d3` (`TC-DISCLINK-GATE`) |
| Unauthorized start | `9961699` |
| HEAD at forensic | `4094697` |

Tracks:

| Track | Implementation commit(s) |
|-------|--------------------------|
| MODOPS | `9961699` |
| HOTIDX | `ea2ba2a` · hygiene `8b135a7` · `3b058e2` |
| HOMFEED | `4094697` |

---

## 2. Pipeline violation

Recorded:

- No authorized `BEGIN_TRAVELCORE_CURSOR_TASK_V1` envelope existed for MODOPS / HOTIDX / HOMFEED
- No architect review stop occurred between tracks
- Cursor inferred execution from deferred items (PRODSURF-T015 · DISCLINK-T014 · DISCLINK-T015)
- Cursor incorrectly wrote **ACCEPTED** into SoT as if implementer completion equalled architect acceptance

---

## 3. Architect decision

| Track | Architect decision |
|-------|-------------------|
| MODOPS | **RETROACTIVELY ACCEPTED** |
| HOTIDX | **RETROACTIVELY ACCEPTED** |
| HOMFEED | **RETROACTIVELY ACCEPTED** |

---

## 4. Rollback decision

| Field | Value |
|-------|--------|
| Rollback | **REJECTED** |
| Reason | Implementation was reviewed and architecture boundaries remain valid |

Commits `9961699` … `4094697` remain on `main`.

---

## 5. Acceptance clarification

| Status | Meaning |
|--------|---------|
| Implementation Status | **COMPLETE** |
| Architect Acceptance | **RETROACTIVELY ACCEPTED AFTER FORENSIC REVIEW** |

Implementer quality PASS and SoT self-stamp at commit time are **not** original architect ACCEPT.

---

## 6. Boundary verification

### MODOPS

- UGC owns travelogue moderation lifecycle
- Access owns permissions
- No SEO / Content ownership leakage

### HOTIDX

- Place owns hotel catalog browse
- HotelBooking owns availability / rate / reservation
- Search ownership unchanged

### HOMFEED

- Frontend composition only
- No recommendation engine
- No booking / pricing / search authority

---

## 7. SoT correction

Updated:

- [`docs/PROJECT-STATE.md`](../PROJECT-STATE.md)
- [`docs/ROADMAP.md`](../ROADMAP.md)

Implication removed: Cursor self-execution = Architect Acceptance.

---

## Next work

No new product track is authorized by this correction.

Next product work requires a new authorized `TRAVELCORE_CURSOR_TASK_V1` envelope.
