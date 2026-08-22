namespace TravelCore.Modules.CommercialFinance.Domain;

/// <summary>Market-specific policy slot (P39-T005 §10).</summary>
public enum CommercialFinanceMarketPolicy : short
{
    Iran = 1,
    Uae = 2,
}

public enum CommissionAgreementStatus : short
{
    Draft = 1,
    Active = 2,
    Suspended = 3,
    Retired = 4,
}

public enum AgencyOfferCommissionOverrideStatus : short
{
    Active = 1,
    Suspended = 2,
}

/// <summary>Commercial Obligation lifecycle vocabulary (P39-T003).</summary>
public enum CommercialObligationLifecycleState : short
{
    Created = 1,
    Pending = 2,
    Approved = 3,
    Settled = 4,
    Cancelled = 5,
    Reversed = 6,
}

public enum SettlementPeriodStatus : short
{
    Open = 1,
    Closed = 2,
}

public enum SettlementRecordStatus : short
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Closed = 4,
}

public enum PayoutInstructionStatus : short
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Cancelled = 4,
}

public enum CommercialFinanceEventSourceKind : short
{
    PaymentSucceeded = 1,
    RefundCorrelation = 2,
    ManualAdjustment = 3,
}
