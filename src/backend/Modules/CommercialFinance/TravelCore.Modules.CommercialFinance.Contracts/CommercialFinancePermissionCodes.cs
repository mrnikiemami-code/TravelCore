namespace TravelCore.Modules.CommercialFinance.Contracts;

/// <summary>Access permission codes for Commercial Finance admin operations (TC-P39-T006).</summary>
public static class CommercialFinancePermissionCodes
{
    public const string AgreementsRead = "commercial.finance.agreements.read";
    public const string AgreementsWrite = "commercial.finance.agreements.write";
    public const string ObligationsRead = "commercial.finance.obligations.read";
    public const string SettlementsRead = "commercial.finance.settlements.read";
    public const string SettlementsApprove = "commercial.finance.settlements.approve";
    public const string PayoutsRead = "commercial.finance.payouts.read";
    public const string PayoutsApprove = "commercial.finance.payouts.approve";
}
