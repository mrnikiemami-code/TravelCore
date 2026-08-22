using NodaTime;
using TravelCore.Modules.CommercialFinance.Contracts;
using TravelCore.Modules.CommercialFinance.Domain;
using MoneyValue = TravelCore.Money.Money;
using Xunit;

namespace TravelCore.Modules.CommercialFinance.UnitTests;

public sealed class CommercialObligationLifecycleRulesTests
{
    [Theory]
    [InlineData(CommercialObligationLifecycleState.Created, CommercialObligationLifecycleState.Pending, true)]
    [InlineData(CommercialObligationLifecycleState.Created, CommercialObligationLifecycleState.Cancelled, true)]
    [InlineData(CommercialObligationLifecycleState.Pending, CommercialObligationLifecycleState.Approved, true)]
    [InlineData(CommercialObligationLifecycleState.Approved, CommercialObligationLifecycleState.Settled, true)]
    [InlineData(CommercialObligationLifecycleState.Settled, CommercialObligationLifecycleState.Reversed, true)]
    [InlineData(CommercialObligationLifecycleState.Created, CommercialObligationLifecycleState.Settled, false)]
    [InlineData(CommercialObligationLifecycleState.Cancelled, CommercialObligationLifecycleState.Pending, false)]
    [InlineData(CommercialObligationLifecycleState.Reversed, CommercialObligationLifecycleState.Settled, false)]
    public void CanTransition_Matches_P39_Lifecycle(
        CommercialObligationLifecycleState from,
        CommercialObligationLifecycleState to,
        bool expected)
    {
        Assert.Equal(expected, CommercialObligationLifecycleRules.CanTransition(from, to));
    }

    [Fact]
    public void EnsureCanTransition_Throws_For_Invalid_Path()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CommercialObligationLifecycleRules.EnsureCanTransition(
                CommercialObligationLifecycleState.Created,
                CommercialObligationLifecycleState.Settled));
    }

    [Fact]
    public void Obligation_TransitionTo_Updates_State()
    {
        var now = Instant.FromUtc(2026, 8, 22, 9, 0);
        var obligation = CommercialObligation.Create(
            CommercialObligationId.New(),
            CommercialFinanceAgencyProfileId.From(Guid.Parse("018f0000-0000-7000-8000-000000000001")),
            agencyOfferId: null,
            CommercialFinanceBookingId.From(Guid.Parse("018f0000-0000-7000-8000-000000000002")),
            paymentId: null,
            sourceEventKey: "payment-succeeded:booking-1",
            amountSnapshot: null,
            evidenceSnapshotHash: null,
            now);

        obligation.TransitionTo(CommercialObligationLifecycleState.Pending, now.Plus(Duration.FromMinutes(1)));
        Assert.Equal(CommercialObligationLifecycleState.Pending, obligation.LifecycleState);
    }
}

public sealed class CommercialFinanceMoneyInvariantTests
{
    [Fact]
    public void Obligation_Allows_Null_Amount_Snapshot()
    {
        var now = Instant.FromUtc(2026, 8, 22, 9, 0);
        var obligation = CommercialObligation.Create(
            CommercialObligationId.New(),
            CommercialFinanceAgencyProfileId.From(Guid.Parse("018f0000-0000-7000-8000-000000000003")),
            agencyOfferId: null,
            CommercialFinanceBookingId.From(Guid.Parse("018f0000-0000-7000-8000-000000000004")),
            paymentId: null,
            sourceEventKey: "evt-no-amount",
            amountSnapshot: null,
            evidenceSnapshotHash: null,
            now);

        Assert.Null(obligation.AmountSnapshot);
    }

    [Fact]
    public void Obligation_Rejects_Negative_Amount_Snapshot()
    {
        var now = Instant.FromUtc(2026, 8, 22, 9, 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => CommercialObligation.Create(
            CommercialObligationId.New(),
            CommercialFinanceAgencyProfileId.From(Guid.Parse("018f0000-0000-7000-8000-000000000005")),
            agencyOfferId: null,
            CommercialFinanceBookingId.From(Guid.Parse("018f0000-0000-7000-8000-000000000006")),
            paymentId: null,
            sourceEventKey: "evt-negative",
            amountSnapshot: new MoneyValue(-1m, "IRR"),
            evidenceSnapshotHash: null,
            now));
    }

    [Fact]
    public void PayoutInstruction_Allows_Optional_Money_Snapshot()
    {
        var now = Instant.FromUtc(2026, 8, 22, 9, 0);
        var instruction = PayoutInstruction.CreateDraft(
            PayoutInstructionId.New(),
            SettlementRecordId.New(),
            payoutAmountSnapshot: new MoneyValue(100m, "AED"),
            approvalRequired: true,
            now);

        Assert.NotNull(instruction.PayoutAmountSnapshot);
        Assert.Equal("AED", instruction.PayoutAmountSnapshot.Currency.Value);
    }
}

public sealed class CommercialFinanceIdempotencyTests
{
    [Fact]
    public void EventConsumptionRecord_Normalizes_Source_Event_Key()
    {
        var now = Instant.FromUtc(2026, 8, 22, 9, 0);
        var record = CommercialFinanceEventConsumptionRecord.Create(
            CommercialFinanceEventSourceKind.PaymentSucceeded,
            "  payment-succeeded:abc  ",
            CommercialObligationId.New(),
            now);

        Assert.Equal("payment-succeeded:abc", record.SourceEventKey);
    }

    [Fact]
    public void Obligation_Source_Event_Key_Is_Unique_Invariant()
    {
        Assert.False(CommercialFinanceIdempotencyBoundary.AutomaticPaymentEventHandlersImplemented);
        Assert.True(CommercialFinanceIdempotencyBoundary.EventConsumptionPersistenceImplemented);
        Assert.Equal(
            "One obligation-side consumption record per source event correlation key",
            CommercialFinanceIdempotencyBoundary.StrictSourceEventConsumption);
    }

    [Fact]
    public void Duplicate_Source_Event_Key_Normalization_Is_Stable()
    {
        var key = CommercialObligation.NormalizeSourceEventKey("payment-succeeded:dup");
        Assert.Equal(key, CommercialObligation.NormalizeSourceEventKey(" payment-succeeded:dup "));
    }
}

public sealed class CommercialFinanceMarketPolicyTests
{
    [Fact]
    public void MarketPolicy_Includes_Iran_And_Uae()
    {
        Assert.Equal(CommercialFinanceMarketPolicy.Iran, (CommercialFinanceMarketPolicy)1);
        Assert.Equal(CommercialFinanceMarketPolicy.Uae, (CommercialFinanceMarketPolicy)2);
        Assert.Equal(2, Enum.GetValues<CommercialFinanceMarketPolicy>().Length);
    }
}
