# TC-P20-T009 Task Envelope (architect, live)

Captured from the same ChatGPT tab after TC-P20-T008 RESULT.

`	ext
TC-P20-T008 = ACCEPTED
Implementation Commit: f11041a
Result/docs HEAD: 7aab5b6
HEAD == origin/main
Working Tree: CLEAN
P20-R1 through P20-R8 = RESOLVED
`

Executable task:

`	ext
BEGIN_TRAVELCORE_CURSOR_TASK_V1

Protocol-Version:
1

Task-ID:
TC-P20-T009

Phase:
P20

Title:
Payment phase hardening, regression coverage, security evidence, and Gate readiness

Baseline:
7aab5b6

Purpose:
Perform the final P20 hardening pass across all accepted Payment decisions
P20-R1 through P20-R8.

This task must NOT introduce a new Payment capability or architecture decision.

Its purpose is to:

- verify all accepted invariants together
- strengthen architecture/security/privacy/concurrency/durability regression tests
- close evidence gaps
- synchronize the P20 implementation ledger
- prepare an authoritative evidence pack for TC-P20-GATE

Do NOT execute TC-P20-GATE.

Do NOT implement a real payment provider.

Required:

1. Repository preflight

Run:

git rev-parse --show-toplevel
git fetch origin
git branch --show-current
git rev-parse HEAD
git rev-parse origin/main
git status --short

Require:

branch = main
HEAD == origin/main
Working Tree = CLEAN

Expected baseline:

7aab5b6

2. Read authoritative P20 plan/state

Inspect:

docs/plans/P20-implementation-plan.md
docs/PROJECT-STATE.md
docs/ROADMAP.md
P20 provider adapter checklist
P20 result/evidence artifacts
accepted architecture/ADR/module-boundary docs

Verify:

P20-R1 through P20-R8 = RESOLVED

Do not invent R9.

3. Execution ledger

Synchronize and verify exact status:

TC-P20-PLAN = ACCEPTED
TC-P20-T001 = ACCEPTED
TC-P20-T002 = ACCEPTED
TC-P20-T003 = ACCEPTED
TC-P20-T004 = ACCEPTED
TC-P20-T005 = ACCEPTED
TC-P20-T006 = ACCEPTED
TC-P20-T007 = ACCEPTED
TC-P20-T008 = ACCEPTED
TC-P20-T009 = IMPLEMENTED / AWAITING_ARCHITECT_REVIEW

TC-P20-GATE = NOT EXECUTED

4. R1 hardening

Verify:

Payment is independent module
schema = payment
initial target = Booking
no generic TargetType/TargetId universalization
no shared DbContext
no peer-schema FK
no peer Infrastructure dependency

5. R2 hardening

Verify exact:

PaymentStatus:
- Pending
- Succeeded

PaymentAttemptStatus:
- Created
- Initiated
- Succeeded
- Failed

Verify:

Payment != PaymentAttempt
Failed PaymentAttempt != Failed Payment
at most one legitimate successful attempt
no new Attempt after Payment Succeeded

6. R3 hardening

Verify:

BrowserReturn != PaymentSuccess
UnverifiedCallback != PaymentSuccess
ClientSuccessFlag != PaymentSuccess
ProviderRedirect != PaymentSuccess

Authoritative provider verification remains required.

7. Provider neutrality

Verify:

Named Provider = NONE
Real Provider SDK = NONE
Production Fake Provider = NONE

8. R4 hardening

Verify:

One Booking -> one logical Payment

Retry -> PaymentAttempt

Database-backed uniqueness/idempotency remains authoritative.

No process-local lock/idempotency structure is the correctness authority.

9. Payment uniqueness

Regression-test concurrent logical Payment creation for one Booking.

Expected:

one Payment
same effective PaymentId

10. Active attempt uniqueness

Regression-test:

at most one Created/Initiated active PaymentAttempt per Payment.

11. Ambiguous provider outcome

Verify:

NetworkTimeout
!=
Failed

Unknown/Ambiguous
!=
PaymentAttempt.Failed

Unsafe retry remains blocked.

12. Callback replay

Repeated verified success remains idempotent.

No duplicate logical Payment success.

13. Contradictory evidence

Terminal state must not silently flip.

Verify durable Payment reconciliation issue/equivalent remains visible.

14. R5 obligation hardening

Verify:

BookingMonetarySnapshot
->
trusted Booking payment obligation
->
PaymentExecutionSnapshot

No live Pricing recalculation.

15. PaymentExecutionSnapshot

Verify immutable:

amount
currency

source is Booking authoritative obligation.

16. Attempt amount consistency

All PaymentAttempts under one Payment use same PaymentExecutionSnapshot.

17. Payment preparation

Verify:

same obligation = idempotent
different obligation = conflict
initiation without snapshot = rejected

18. Amount tampering

Regression-test public and provider boundaries.

Client amount cannot become authority.

19. Currency tampering

Same for CurrencyCode.

20. Provider amount mismatch

Verified provider success with wrong amount:

must not succeed Payment.

21. Provider currency mismatch

Same requirement.

22. Missing provider amount/currency

Verify current accepted semantics remain explicit and safe.

Do not weaken T005 verification behavior.

23. Payment success durability

Verify:

Payment -> Succeeded
+
PaymentSucceededIntegrationEvent outbox

commit atomically.

24. Outbox crash window

Prove no accepted path can commit Payment Succeeded without durable downstream
success trigger.

25. Payment success event

Verify:

PaymentSucceededIntegrationEvent

contains no Booking passenger/contact PII.

26. Booking consumer

Verify Booking owns Payment-success consumer.

Payment does not mutate Booking persistence.

27. Booking revalidation

Verify Booking re-reads authoritative Payment evidence and rechecks:

- Pending Booking
- monetary snapshot
- amount/currency
- passenger/contact prerequisites
- Active unexpired CapacityHold

28. Confirmation transaction

Verify atomically:

Active -> Consumed

Pending -> Confirmed

29. No unrestricted Confirm

Verify only accepted authoritative Payment-success path can perform confirmation.

No generic Confirm()/SetConfirmed()/boolean-paid path.

30. Duplicate Payment-success delivery

Regression-test:

Booking confirms once
Hold consumes once
Inbox/effect remains idempotent.

31. Delayed delivery

Verify Payment can remain Succeeded while Booking processing is delayed.

32. Expired hold delayed delivery

Expected:

Payment = Succeeded
Booking != Confirmed
Hold = Expired
BookingConfirmationRecoveryIssue exists

33. Released hold delayed delivery

Expected:

Booking does not confirm
no capacity resurrection
recovery issue exists

34. Cancelled Booking delayed delivery

Expected:

Booking stays Cancelled
no reopen
recovery issue exists

35. Success-vs-cancel race

Verify:

Cancelled -> Confirmed

cannot occur.

36. Success-vs-expiry race

Forbidden final state:

Booking Confirmed
+
Hold Expired

for same effective transition.

37. R6 Refund hardening

Verify:

Refund != Payment

RefundStatus:
- Pending
- Succeeded

RefundAttemptStatus:
- Created
- Initiated
- Succeeded
- Failed

38. Payment truth after Refund

Verify:

PaymentStatus remains Succeeded

39. Refund amount

Verify:

Refund amount
=
PaymentExecutionSnapshot amount

40. Refund currency

Verify:

Refund CurrencyCode
=
PaymentExecutionSnapshot CurrencyCode

41. Partial Refund

Confirm:

NOT IMPLEMENTED

42. Multiple logical refunds

Verify baseline:

one Payment -> at most one logical Refund

43. Refund retries

Failed RefundAttempt allows explicit retry.

Unresolved RefundAttempt blocks unsafe retry.

44. Refund ambiguity

Verify:

NetworkTimeout
!=
RefundAttempt.Failed

45. Refund provider verification

Unverified refund evidence cannot mark Refund succeeded.

46. Refund amount mismatch

Regression-test wrong refund amount rejected.

47. Refund currency mismatch

Regression-test wrong refund currency rejected.

48. Compensation durability

Verify Booking business rejection atomically persists:

BookingConfirmationRecoveryIssue
+
BookingPaymentCompensationRequiredIntegrationEvent outbox

49. Technical failure distinction

Transient technical Booking handler failure must not emit compensation-required
business evidence.

50. Payment compensation inbox

Verify repeated compensation event creates one logical Refund.

51. RefundSucceeded durability

Verify:

Refund -> Succeeded
+
RefundSucceededIntegrationEvent outbox

commit atomically.

52. RefundSucceeded event

Verify no passenger/contact PII.

53. Booking RefundSucceeded consumer

Verify Booking owns consumer and Payment does not mutate Booking.

54. Pending Booking compensation finalization

Verify:

Pending + Active Hold
+
RefundSucceeded

=>

Booking Cancelled
Hold Released

55. Expired hold compensation finalization

Expected:

Booking Cancelled
Hold remains Expired

56. Released hold compensation finalization

Expected:

Booking Cancelled
Hold remains Released

57. Already Cancelled Booking

RefundSucceeded is idempotent.

58. Confirmed Booking

Verify:

Confirmed -> Cancelled

is NOT implemented.

59. Consumed hold

Verify:

Consumed -> Released

is NOT implemented.

60. General cancellation policy

Verify cancellation penalty/schedule/general refund policy remains deferred.

61. R7 public authorization regression

Exact public routes must remain:

GET /api/booking/public/{bookingId}/payment

POST /api/booking/public/{bookingId}/payment/initiation

62. Private frontend routes

Verify:

/[locale]/bookings/[bookingId]/payment

/[locale]/bookings/[bookingId]/payment/return

63. Anonymous credential

Verify reuse of:

X-TravelCore-Booking-Access-Token

64. Token exposure

Verify raw Booking token does NOT enter:

- URL
- query string
- logs
- localStorage

Preserve accepted sessionStorage behavior if still current.

65. Missing token

Expected:

404

66. Wrong token

Expected:

404

67. Unknown Booking

Expected:

404

68. Cross-user

Expected:

404

69. BookingId only

Must not authorize Payment.

70. PaymentId only

Must not authorize Payment.

71. Public amount tampering

Verify ignored/rejected and non-authoritative.

72. Public currency tampering

Same.

73. Public success tampering

Same.

74. Public provider tampering

Verify provider selection remains server-controlled.

75. No production provider

Public initiation still returns truthful unavailable behavior.

Expected current posture:

503

76. Payment already Succeeded

Public re-initiation must not contact provider or create Attempt.

77. Unresolved Attempt

Public re-initiation must not create unsafe retry.

78. Failed Attempt

Explicit retry remains allowed under accepted idempotency rules.

79. Browser return

Verify return route cannot mark Payment success.

80. Callback route

Verify technical callback remains separate:

POST /api/payment/providers/{providerKey}/callback

81. No public Payment list

Verify absent.

82. No generic public Payment-by-id authority

Verify absent unless Booking authorization still gates it.

Preferred existing posture:
absent.

83. No public Refund API

Verify absent.

84. No card collection

Verify frontend contains no:

PAN
card number
CVV/CVC
PIN
banking password fields

85. Private transactional SEO posture

Verify Payment/return pages:

noindex

86. Search boundary

Verify Payment/Refund transactional data not projected to Search.

87. R7 frontend regression

Verify:

FA
EN
AR

continue to compile/render according to accepted locale architecture.

88. RTL/LTR/bidi

Verify Payment views remain direction-neutral/bidi-safe.

89. Money display

Preserve:

Toman != CurrencyCode

No domain mutation for display.

90. Accessibility

Review Payment flow for:

- semantic headings
- accessible status text
- keyboard action
- focus visibility
- associated labels where forms exist
- loading state
- errors not color-only

91. Mobile-first

Inspect narrow viewport composition/code for accidental desktop-only layout.

92. Server Component First

Verify unnecessary client boundaries have not spread.

93. R8 provider capability hardening

Verify exact capabilities remain:

- RedirectInitiation
- CallbackVerification
- PaymentStatusQuery
- RefundInitiation
- RefundVerification
- RefundStatusQuery

94. Capability exactness

Do not add speculative capabilities in T009.

95. Zero-provider host

Verify host starts with zero production providers.

96. Duplicate ProviderKey

Verify configuration rejects duplicate provider key.

97. Disabled provider

Verify cannot initiate.

98. Unknown provider

Verify safe rejection.

99. Unsupported Payment query

Verify no provider call/mutation occurs.

100. Unsupported Refund initiation

Verify no Refund execution occurs.

101. Unsupported Refund query

Verify no provider call/mutation occurs.

102. No failover

Confirm automatic provider failover absent.

103. No smart routing

Confirm absent.

104. Operational read

Verify internal-only:

IPaymentOperationalQuery

or current equivalent.

105. No public ops endpoint

Verify no anonymous/public support route.

106. Booking token cannot access ops

Verify.

107. Operational read privacy

Verify no:

- passenger PII
- Booking access token
- provider secrets
- raw callback payload

108. Operational Payment visibility

Verify safe read contains appropriate:

Payment
Attempts
Refund
RefundAttempts
reconciliation summary

109. No mega-status

Verify no synthetic combined financial/Booking workflow status has been introduced.

110. Manual mutation guardrail

Verify absent:

SetStatus
ForceSuccess
MarkPaid
MarkRefunded
ForceConfirm
SetConfirmed

111. Operational recheck

If available, verify outcome source remains:

authoritative provider query

112. Operator cannot choose result

Recheck caller must not provide Succeeded/Failed result.

113. Cross-Payment callback correlation

Regression-test callback for Payment A cannot mutate Payment B.

114. Cross-Refund correlation

Refund evidence A cannot mutate Refund B.

115. Payment vs Refund correlation

Collection evidence must not mark Refund succeeded.

Refund evidence must not mark PaymentAttempt succeeded.

116. Provider-scoped reference uniqueness

Re-verify constraints.

117. Callback replay security

Re-verify.

118. Unknown provider callback

Expected:

safe failure / current 404 behavior
no mutation.

119. Secrets posture

Search relevant Payment implementation/config/logging for accidental:

merchant key
API secret
callback secret
raw token
PAN/CVV

No secrets committed.

120. Raw provider payload

Confirm no unnecessary raw callback/provider JSON persistence was added.

121. Outbox/inbox inventory

Document exact durable tables/mechanisms currently used for P20 flows.

At minimum identify:

Payment success outbox
Booking Payment-success inbox
Booking compensation outbox
Payment compensation inbox
Refund-success outbox
Booking Refund-success inbox

122. Delivery semantics evidence

Verify:

distributed delivery = at-least-once
local effects = idempotent/effectively-once

No exactly-once claim.

123. Process restart correctness

Confirm all critical duplicate/delivery correctness is database-backed.

124. No in-memory authority

Search for correctness-sensitive:

static Dictionary
ConcurrentDictionary
SemaphoreSlim
lock

within Payment orchestration.

If present for performance only, prove it is not correctness authority.

125. Cross-schema persistence hardening

Inspect migrations/schema for:

no peer-schema FK

126. Cross-schema SQL hardening

Verify no direct Booking/Payment cross-schema SQL shortcuts were introduced.

127. Shared DbContext

Verify none.

128. Module dependency hardening

Verify:

Payment.Infrastructure !-> Booking.Infrastructure

Booking.Infrastructure !-> Payment.Infrastructure

Payment.Infrastructure !-> Pricing.Infrastructure

129. Pricing boundary

Verify Payment does not calculate:

tax
fee
discount
FX
quote
total

130. Settlement boundary

Confirm not implemented.

131. Accounting boundary

Confirm not implemented.

132. Agency settlement

Confirm not implemented.

133. Wallet

Confirm not implemented.

134. Fraud engine

Confirm not implemented.

135. Chargeback/dispute

Confirm not implemented.

136. Recurring/subscription payment

Confirm not implemented.

137. Provider onboarding checklist

Verify documentation exists and covers:

- credential/secrets
- currencies
- callback verification
- amount units
- query support
- refund support
- timeout ambiguity
- sandbox vs production

138. Production provider status

Evidence pack must explicitly say:

Production Provider:
NONE / NOT CONFIGURED

139. Payment phase security threat matrix

Create or update a P20 T009 evidence artifact covering at minimum:

- Booking ownership bypass
- amount tampering
- currency tampering
- forged callback
- callback replay
- duplicate Payment
- duplicate Attempt
- ambiguous provider timeout
- double charge
- Payment success crash window
- Booking confirmation race
- expired capacity after payment
- successful payment + failed Booking confirmation
- duplicate compensation
- duplicate Refund
- forged Refund evidence
- Refund amount/currency mismatch
- delayed compensation delivery
- token leakage
- open redirect
- card-data collection
- provider secret leakage

140. Failure-mode matrix

Document final behavior for at least:

provider initiation timeout

provider callback missing

provider success duplicated

Payment succeeds / Booking consumer delayed

Payment succeeds / hold expired

Payment succeeds / Booking cancelled

Booking recovery event delayed

Refund initiation ambiguous

Refund success duplicated

Refund success / Booking consumer delayed

provider lacks Refund capability

zero production providers configured

141. Evidence pack artifact

Create:

docs/plans/P20-T009-hardening-and-evidence-pack.md

142. Evidence pack content

Include:

- accepted P20 decision ledger
- exact status enums
- route inventory
- module ownership inventory
- persistence/outbox/inbox inventory
- security evidence
- concurrency evidence
- privacy evidence
- provider capability inventory
- deferred/out-of-scope list
- exact validation results

143. Deferred list

Explicitly preserve at minimum:

Real production provider integration = DEFERRED

Confirmed Booking cancellation = DEFERRED

Consumed capacity reversal = DEFERRED

Partial Refund = DEFERRED

General cancellation/refund policy = DEFERRED

Chargeback/dispute = DEFERRED

Accounting ledger = OUT/DEFERRED

Bank settlement = OUT/DEFERRED

Agency settlement = OUT/DEFERRED

Wallet = OUT/DEFERRED

Fraud/risk engine = OUT/DEFERRED

Recurring/subscription billing = OUT/DEFERRED

Smart provider routing/failover = DEFERRED

144. No new product capability

T009 is evidence/hardening only.

Any defect correction must remain strictly within accepted P20-R1 through R8
semantics.

145. Regression correction policy

If a test reveals violation of an already accepted invariant:

fix that invariant minimally.

Do not make a new architecture decision.

146. Architecture tests

Add a P20 phase-boundary guardrail suite if useful.

It should prove the locked boundaries rather than duplicate every unit test.

147. Security reflection/static guardrails

Where useful, assert absence of forbidden public surface/types/methods.

148. Persistence inspection

Inspect current Payment and Booking migrations/tables.

Record actual relevant P20 tables.

Do not guess.

149. Routes inspection

Record exact actual backend/frontend routes in evidence artifact.

150. Source-of-Truth synchronization

Update authoritative state to record:

TC-P20-T008 = ACCEPTED

TC-P20-T009 = implemented / awaiting architect review

P20-R1 through P20-R8 = RESOLVED

P20 remains:
IN_PROGRESS

TC-P20-GATE remains:
NOT EXECUTED

Do NOT mark P20 COMPLETE.

151. Gate readiness

Evidence artifact must explicitly state whether:

P20 is READY FOR GATE

Only say YES if all accepted invariants and full validation pass.

Allowed:

- hardening tests
- security tests
- concurrency regression tests
- persistence inspection
- privacy/a11y/bidi review/fixes
- evidence documentation
- SoT synchronization
- minimal fixes required to restore accepted P20 invariants

Forbidden:

- new P20-R decision
- real provider
- provider SDK
- provider credentials
- confirmed Booking cancellation
- consumed capacity reversal
- partial Refund
- general cancellation policy
- accounting
- settlement
- agency settlement
- wallet
- fraud
- chargeback
- subscription billing
- smart provider routing
- manual financial state mutation
- public admin Payment surface
- future phase implementation
- unrelated refactoring
- dependency upgrades
- TC-P20-GATE execution

Done:

- all P20-R1 through R8 invariants are verified together
- public authorization and privacy regressions pass
- Payment/Refund amount and currency integrity pass
- callback/refund evidence forgery cannot fabricate success
- duplicate/retry/concurrency protections pass
- Payment success outbox durability passes
- Booking confirmation inbox/idempotency passes
- financial recovery/Refund durability passes
- zero-provider production posture passes
- operational reads remain read-only/internal
- no forbidden lifecycle mutation surfaces exist
- no peer-schema FK/shared DbContext/peer Infrastructure dependency exists
- no real provider exists
- exact evidence pack exists
- deferred list is preserved
- P20 is ready for Gate
- P20 is NOT marked COMPLETE
- TC-P20-GATE is NOT executed

Validation:

Run:

dotnet build TravelCore.sln

dotnet test tests/Unit/TravelCore.Modules.Payment.UnitTests/TravelCore.Modules.Payment.UnitTests.csproj

dotnet test tests/Unit/TravelCore.Modules.Booking.UnitTests/TravelCore.Modules.Booking.UnitTests.csproj

dotnet test tests/Architecture/TravelCore.ArchitectureTests/TravelCore.ArchitectureTests.csproj

dotnet test tests/Integration/TravelCore.Persistence.IntegrationTests/TravelCore.Persistence.IntegrationTests.csproj

dotnet test tests/Integration/TravelCore.Host.IntegrationTests/TravelCore.Host.IntegrationTests.csproj

Frontend:

npm run typecheck
npm run lint
npm run build

git diff --check

Required Result Evidence:

Report exact:

- Payment Unit test count
- Booking Unit test count
- Architecture test count
- Persistence Integration test count
- Host Integration test count
- frontend typecheck
- frontend lint
- frontend production build
- git diff --check

Also report:

- P20-R1 through P20-R8 = RESOLVED
- PaymentStatus exact values
- PaymentAttemptStatus exact values
- RefundStatus exact values
- RefundAttemptStatus exact values
- BookingStatus exact values
- CapacityHoldStatus exact values
- one Booking -> one Payment evidence
- one active PaymentAttempt evidence
- ambiguous Payment retry evidence
- callback replay evidence
- provider amount mismatch evidence
- provider currency mismatch evidence
- Payment success outbox atomicity evidence
- Booking success consumer idempotency evidence
- expired hold after Payment evidence
- cancelled Booking after Payment evidence
- one Payment -> one Refund evidence
- ambiguous Refund retry evidence
- Refund amount mismatch evidence
- Refund currency mismatch evidence
- compensation outbox/inbox evidence
- RefundSucceeded outbox/inbox evidence
- Confirmed Booking cancellation: NO
- Consumed hold reversal: NO
- Partial Refund: NO
- public Refund API: NO
- card collection: NO
- raw Booking token URL exposure: NO
- public Payment list: NO
- Production Provider: NONE
- Real Provider SDK: NO
- provider capability exact values
- operational read surface
- operational mutation surface: NONE
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- distributed transaction: NO
- Accounting/Settlement/Agency Settlement/Wallet/Fraud/Chargeback/Subscriptions:
  NOT IMPLEMENTED
- evidence artifact path
- P20 READY FOR GATE: YES/NO
- TC-P20-GATE: NOT EXECUTED

Repository safety:

- discover repository root using:
  git rev-parse --show-toplevel
- git fetch origin
- require branch main
- require HEAD == origin/main
- require clean working tree before work

Forbidden repository operations:

- force push
- accepted-history rewrite
- reset discarding accepted work
- duplicate cherry-picks

Commit:

After successful validation:

- commit with TC-P20-T009 in commit message
- push main to origin/main using normal fast-forward push
- re-fetch origin
- verify HEAD == origin/main
- verify Working Tree CLEAN

Expected Baseline:
7aab5b6

Auto-Execute:

After PASS:

- return TC-P20-T009 RESULT to architect
- do NOT execute TC-P20-GATE until T009 is architect ACCEPTED
- remain in PIPELINE

END_TRAVELCORE_CURSOR_TASK_V1
`
