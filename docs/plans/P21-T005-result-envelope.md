# TC-P21-T005 Result Envelope

```text
BEGIN_TRAVELCORE_CURSOR_RESULT_V1

Protocol-Version: 1
Task-ID: TC-P21-T005
Phase: P21
Status: PASS

Repository:
C:/Users/User/TravelCore
https://github.com/mrnikami-code/TravelCore.git

Branch: main
Baseline: 9f38ef6
Implementation-Commit: 8cc1b28
Starting-HEAD: 9f38ef6
Working-Tree: CLEAN

Scope Delivered:
- HotelBookingStatus Pending/Confirmed/Cancelled; Create starts Pending; ConfirmedAt only after confirmation
- ConfirmFromAuthoritativeSupplierReservation only (no public Confirm/SetConfirmed)
- HotelSupplierReservation + Attempt + DB-backed idempotency; one reservation per HotelBooking covering complete RoomReservation set
- IHotelReservationSource + IHotelReservationSourceResolver + ReservationSourceKey (server-controlled)
- HotelSupplierReservationService.InitiateAsync / RecheckAsync; MarkInitiated + SaveChanges before network
- NetworkTimeout/Unknown remain Initiated (not Failed); unresolved Created/Initiated blocks another attempt
- Authoritative Failed allows a new attempt; Confirmed forbids another attempt
- Partial room confirmation persists RoomSetMismatch and cannot Confirm
- Mismatches persist HotelBookingReconciliationIssue; monetary snapshot is not rewritten; booking stays Pending
- Named Hotel Supplier = NONE; Production Hotel Reservation Source = NONE; no fake production source in DI
- P20 Payment/Refund untouched; no public /api/hotel-booking*; MapEndpoints empty; frontend untouched
- P21-R5 recorded RESOLVED; P21-R6 through P21-R8 remain OPEN; T006 not executed

Key Artifacts:
- src/backend/Modules/HotelBooking/**
- tests/Unit/TravelCore.Modules.HotelBooking.UnitTests/HotelSupplierReservationTests.cs
- tests/Integration/TravelCore.Persistence.IntegrationTests/HotelSupplierReservationPersistenceTests.cs
- tests/Architecture/TravelCore.ArchitectureTests/HotelBookingBoundaryGuardrailTests.cs
- docs/plans/P21-implementation-plan.md
- docs/PROJECT-STATE.md
- docs/ROADMAP.md
- docs/plans/P21-T005-task-envelope.md

Exact-Validation:
dotnet build TravelCore.sln: PASS (0 errors)
HotelBooking.UnitTests: 60 passed
ArchitectureTests: 298 passed
Persistence.IntegrationTests: 96 passed
Host.IntegrationTests: 57 passed
frontend touched: NO
git diff --check: PASS

Required Result Evidence:
- HotelBookingStatus exact values: Pending=1, Confirmed=2, Cancelled=3
- initial HotelBooking status: Pending
- SupplierReservation type: HotelSupplierReservation
- SupplierReservationStatus exact values: Pending=1, Confirmed=2, Cancelled=3 (no Failed)
- SupplierReservationAttemptStatus exact values: Created=1, Initiated=2, Confirmed=3, Failed=4
- one logical reservation per HotelBooking: YES (ux_hotel_supplier_reservations_hotel_booking_id)
- one reservation covers complete multi-room set: YES
- partial room confirmation behavior: RoomSetMismatch persisted; attempt stays Initiated; HotelBooking stays Pending
- Named Hotel Supplier: NONE
- Production Hotel Reservation Source: NONE
- supplier SDK: NONE
- network timeout behavior: TimeoutException / TaskCanceledException / Outcome.Timeout leave attempt Initiated; not Failed
- unresolved attempt retry behavior: blocked (domain + ux_hotel_supplier_reservation_attempts_one_unresolved for status IN (1,2))
- definitive failed attempt retry behavior: new attempt allowed under the same HotelSupplierReservation
- concurrent attempt result: unique-index conflict / InvalidOperationException; no second unresolved attempt
- same idempotency-key result: returns existing reservation (PK hotel_supplier_reservation_id + idempotency_key)
- authoritative confirmation source: IHotelReservationSource Complete/Confirmed evidence + matching snapshots
- unverified callback behavior: UnverifiedSupplierCallback != Confirmed; no webhook/callback confirm path
- callback replay result: RecheckAsync on already-Confirmed is no-op; unique source_key+source_reservation_reference
- cross-booking correlation result: ux_hotel_supplier_reservations_source_ref unique where reference is not null
- monetary mismatch result: HotelBookingReconciliationIssue MonetaryMismatch; snapshot unchanged; not Confirmed
- currency mismatch result: CurrencyMismatch; not Confirmed
- cancellation-terms mismatch result: CancellationTermsMismatch; not Confirmed
- stay/hotel mismatch result: StayMismatch / HotelMismatch; not Confirmed
- HotelBooking confirmation owner: HotelBooking.ConfirmFromAuthoritativeSupplierReservation
- generic Confirm surface: NO
- user cancellation execution: NO
- Confirmed cancellation: NO (Cancelled exists on enum/reservation; user cancel / R7 not implemented)
- Payment integration/change: NO
- Refund/Partial Refund change: NO
- public HotelBooking API/UI: NO
- peer-schema FK: NO
- shared DbContext: NO
- peer Infrastructure dependency: NO
- P21-R5: RESOLVED
- P21-R6 through P21-R8: OPEN
- TC-P21-T006: NOT EXECUTED

Persistence tables:
- hotel_booking.hotel_bookings.status (smallint NOT NULL DEFAULT 1) + confirmed_at
- hotel_booking.hotel_supplier_reservations
- hotel_booking.hotel_supplier_reservation_attempts
- hotel_booking.hotel_supplier_reservation_idempotency
- hotel_booking.hotel_booking_reconciliation_issues

Migration:
- 20260818145918_AddHotelSupplierReservation

Next-State:
AWAITING_ARCHITECT_REVIEW

END_TRAVELCORE_CURSOR_RESULT_V1
```
