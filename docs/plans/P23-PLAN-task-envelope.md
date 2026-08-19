# TC-P23-PLAN Task Envelope

Captured live after architect `TC-P22-GATE = ACCEPTED` on the same ChatGPT conversation. Baseline `2a372ae`. Current HEAD at capture `ed040f0` (GATE docs). Do **not** execute `TC-P23-T001`.

```text
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version: 1
Task-ID: TC-P23-PLAN
Phase: P23
Title: Dynamic Package / Flight + Hotel architecture and implementation plan
Baseline: 2a372ae

Purpose:
Inspect the authoritative repository and produce the implementation plan for:

P23 — Dynamic Package / Flight + Hotel

This is architecture/planning only.

Do NOT implement product code.
Do NOT create a DynamicPackage module/schema yet.
Do NOT modify Flight, HotelBooking, Payment, Pricing, Search, or Booking behavior.
Do NOT start T001.

1. Repository preflight

Discover repository root:

git rev-parse --show-toplevel

Then:

git fetch origin
git branch --show-current
git rev-parse HEAD
git rev-parse origin/main
git status --short

Require:

branch = main
HEAD == origin/main
Working Tree = CLEAN

Expected accepted lineage includes:

TC-P22-GATE
2a372ae


2. Read authoritative SoT

Inspect at minimum:

docs/PROJECT-STATE.md
docs/ROADMAP.md
docs/architecture/**
docs/adr/**
docs/plans/P19*
docs/plans/P20*
docs/plans/P21*
docs/plans/P22*

Also inspect actual implementation of:

Tour
Booking
Pricing
HotelBooking
Flight
Payment
Search
SEO
ReferenceData
Place

Repository is the source of truth.


3. Confirm P23 position

Report exact authoritative:

Phase name
status
declared scope
dependencies
next-phase relationship

Do not infer from memory if SoT differs.


4. Preserve locked architecture

The plan must preserve:

FlightBooking != HotelBooking
Hotel Catalog != HotelBooking
Flight != Tour package transport
Price != Quote != Booking != Payment
Payment owns money movement
Search is not transaction SoT
no shared DbContext
schema-per-module
no peer-schema FK
no distributed transaction
UUIDv7
NodaTime
Money/Currency rules
server-component-first frontend
FA/EN/AR
mobile/accessibility/bidi
controlled SEO/indexation


5. Investigate Dynamic Package meaning

Determine from repository evidence whether P23 should mean:

A. a new commercial Product aggregate,
B. a customer Quote/selection aggregate,
C. a transaction/orchestration aggregate coordinating FlightBooking + HotelBooking,
D. composition of existing Flight and Hotel transactions without a new persistent owner,
E. another repository-supported interpretation.

Do NOT choose before inspecting evidence.

Give a recommendation with tradeoffs.


6. Ownership question

Investigate candidate ownership:

- new DynamicPackage module
- Booking module
- Tour module
- separate orchestration/application layer
- another existing module

Recommend exactly one baseline.

Do not create it yet.


7. Transaction boundary

Determine whether a Dynamic Package needs its own durable transaction identity such as:

DynamicPackageBooking

or whether it should reference/co-ordinate existing:

FlightBooking
HotelBooking

without replacing their ownership.

Explicitly prevent accidental creation of a generic universal Booking abstraction.


8. Composition semantics

Define candidate baseline for:

Flight component
Hotel component

Determine whether baseline requires:

exactly one FlightBooking + exactly one HotelBooking

or another cardinality.

Consider:

round-trip flight
multi-room hotel
connecting flights

Do not introduce MultiCity flight unless P23 genuinely requires it.


9. Search/discovery composition

Investigate how a customer finds a Flight + Hotel combination.

Determine ownership of:

- destination/date intent
- flight search
- hotel availability/rate search
- package candidate combination
- ranking
- transient package result

Preserve:

Search module != live supplier truth.


10. Package candidate vs accepted transaction

Define required separations, likely including:

DynamicPackageSearchResult
PackageQuote / PackageOffer
DynamicPackageBooking

but do not lock names without evidence.

Explicitly decide whether a transient package combination is transaction truth.


11. Price authority

Investigate how package price should work across:

FlightBookingMonetarySnapshot
HotelBookingMonetarySnapshot

Determine whether P23 needs:

- a package monetary snapshot
- simple immutable sum of component obligations
- package discount allocation
- package-owned fees
- another approach

Do NOT generalize Pricing without explicit justification.


12. Currency rule

Determine baseline behavior when Flight and Hotel offers have:

same currency
different currencies

No implicit FX.

Toman remains display-only, not CurrencyCode.


13. Package discount

Investigate whether Dynamic Package baseline includes a genuine package discount.

If no repository/business evidence exists:

recommend DEFERRED.

Do not invent discount economics.


14. Availability consistency

Analyze the core race:

Flight available
+
Hotel available
at different moments.

Determine required revalidation/hold/order strategy before customer commitment.


15. Reservation ordering

Compare at least:

A. Flight reservation first → Hotel hold/reservation
B. Hotel hold first → Flight reservation
C. parallel reservation
D. another evidence-based sequence

Account for existing realities:

Flight uses source-authoritative PNR/reservation.
Hotel uses authoritative availability/hold/final reservation.
Flight PNR can expire.
Hotel hold may expire.
Both may have ambiguous supplier outcomes.

Recommend one baseline and justify it.


16. Atomicity posture

Explicitly state:

There is no distributed transaction across Flight / Hotel / Payment / suppliers.

Define Saga/orchestration/compensation needs if required.

Do not claim exactly-once.


17. Failure matrix

Analyze at minimum:

- Flight succeeds, Hotel fails
- Hotel succeeds, Flight fails
- Flight ambiguous
- Hotel ambiguous
- component offer expires
- one component reprices
- Payment succeeds but one component cannot finalize
- only one component becomes confirmed
- crash between component operations

For each identify likely:

retry
recheck
compensation
reconciliation


18. Payment architecture

Inspect current Payment target model:

TourBooking
HotelBooking
FlightBooking

Determine whether P23 should:

A. add DynamicPackageBooking as a fourth explicit Payment target,
B. create separate Flight + Hotel Payments,
C. another explicit model.

Recommend one.

Do NOT implement it.


19. One-charge customer experience

Evaluate whether baseline should expose one customer Payment for the package.

If yes, define where the combined monetary obligation comes from.

If no, explain UX and consistency tradeoff.

Do not silently assume.


20. Refund dependency

Explicitly analyze interaction with current limitation:

Partial Refund = DEFERRED

Dynamic packages are especially sensitive because one component may fail or cancel
while another remains valid.

Determine whether this limitation blocks any P23 slice.

Do not implement Partial Refund during planning.


21. Cancellation semantics

Analyze baseline cancellation choices:

- cancel whole package only
- component cancellation allowed
- Flight cancellation independent
- Hotel cancellation independent

Identify what is safely implementable given current Refund capabilities.

Recommend baseline.


22. Component status vs package status

Determine whether P23 requires a package lifecycle.

If yes, propose minimal statuses only.

Avoid mega-status that duplicates:

FlightBookingStatus
HotelBookingStatus
PaymentStatus
supplier reservation states


23. Confirmation semantics

Define exact evidence that would allow a package transaction to become Confirmed.

Do not reduce component truth to one boolean.

Consider:

FlightBooking Confirmed
HotelBooking Confirmed
Payment evidence
package-level monetary evidence


24. Compensation semantics

Determine when package orchestration may need:

Flight cancellation
Hotel cancellation
Payment Refund

Preserve each module's ownership.

P23 must orchestrate via Contracts/events/services, not peer Infrastructure access.


25. Public UX

Define candidate public journey, likely:

Search Flight + Hotel
→ compare combinations
→ select package
→ passenger/guest details
→ authoritative revalidation
→ component reservation/hold
→ Payment
→ final confirmation

Do not lock exact endpoints yet unless repository evidence supports them.


26. Existing identifiers reuse

Analyze whether passenger/guest/customer contact information can be safely shared
between component initiation requests.

Do NOT introduce shared mutable Passenger/Guest aggregate.


27. Privacy

Identify minimal PII required.

Do not broaden Flight PII merely because Hotel may have other guest facts.

Preserve bounded-context snapshots.


28. SEO

Determine which P23 pages could be indexable discovery pages versus private
transaction pages.

SEO remains policy owner.


29. Operational support

Determine what internal operational read model is required to understand a package
across:

Flight
HotelBooking
Payment
component reconciliation

No operational force mutations.


30. Supplier posture

Keep current production matrix truthful.

Report existing:

Flight production sources
Hotel production sources
Payment production provider

Do not add named suppliers/providers.


31. AI readiness

Assess only structural readiness:

- structured package facts
- provenance
- component attribution
- locale-aware presentation
- stable contracts

Do NOT introduce:

LLM
RAG
vector DB
embeddings
AI orchestration


32. Proposed P23 decision inventory

Produce P23-R1 through P23-R8.

Recommended coverage:

R1 — ownership/module/transaction boundary
R2 — component composition and package lifecycle
R3 — search/composition/revalidation authority
R4 — package quote/monetary/currency/discount model
R5 — reservation orchestration/idempotency/reconciliation
R6 — Payment ordering/target/confirmation/compensation
R7 — cancellation/refund/partial-refund dependency
R8 — public UX/auth/privacy/operations/SEO

Adjust only if repository evidence clearly requires it.

Leave all OPEN.


33. Proposed task sequence

Produce a compact execution sequence:

TC-P23-T001
...
TC-P23-T009
TC-P23-GATE

Prefer:

T001-T008 = implementation of R1-R8
T009 = hardening/evidence
GATE = closure

Do not execute any.


34. Risk register

Identify high-risk architectural points, especially:

- distributed consistency
- cross-component expiry
- ambiguous external supplier states
- one-charge vs two-charge semantics
- partial-refund limitation
- cancellation compensation
- package repricing
- duplicated passenger/guest PII
- accidental generic Booking abstraction
- accidental Pricing generalization


35. Create planning artifact

Create:

docs/plans/P23-implementation-plan.md

It must contain:

- repository findings
- current architecture dependencies
- ownership candidates
- recommended baseline
- R1-R8 decision inventory
- T001-T009 + Gate sequence
- failure matrix
- payment/refund analysis
- deferred scope
- explicit blockers if any


36. SoT update

Update:

docs/PROJECT-STATE.md
docs/ROADMAP.md

Record only:

P22 Gate ACCEPTED
P22 COMPLETE
P23 planning started/completed as appropriate
TC-P23-PLAN result state

Do NOT mark P23 decisions RESOLVED.
Do NOT mark P23 COMPLETE.


37. Validation

Since this is docs/planning only:

dotnet build TravelCore.sln

Run architecture tests if planning changes touch architecture guardrails.

Run:

git diff --check

Frontend should remain untouched.


38. Required result evidence

Return:

- current HEAD
- HEAD == origin/main
- Working Tree
- P23 exact SoT title/status
- recommended P23 owner
- recommended schema if any
- new persistent package aggregate recommended YES/NO
- exact proposed transaction identity if any
- FlightBooking ownership unchanged YES/NO
- HotelBooking ownership unchanged YES/NO
- component cardinality recommendation
- search/composition authority recommendation
- package quote/offer recommendation
- package monetary snapshot recommendation
- same-currency posture
- mixed-currency posture
- package discount posture
- reservation ordering recommendation
- distributed transaction: NO
- proposed Payment model
- one customer charge recommendation
- Partial Refund dependency/blocker analysis
- package cancellation recommendation
- package confirmation evidence
- PII posture
- public UX candidate
- SEO posture
- operational read posture
- production Flight source matrix
- production Hotel source matrix
- Production Payment Provider
- P23-R1 through R8 status = OPEN
- proposed T001-T009 sequence
- blockers
- planning artifact path
- TC-P23-T001 executed: NO


39. Commit/push

After PASS:

- commit with TC-P23-PLAN in message
- push normal fast-forward to origin/main
- re-fetch
- verify HEAD == origin/main
- verify Working Tree CLEAN


40. Stop

Return TC-P23-PLAN RESULT.

Do NOT execute TC-P23-T001.

END_TRAVELCORE_CURSOR_TASK_V1
```
