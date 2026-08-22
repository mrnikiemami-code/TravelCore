using TravelCore.Identifiers;

namespace TravelCore.Modules.CommercialFinance.Domain;

public readonly record struct CommissionAgreementId(Guid Value) : IEquatable<CommissionAgreementId>
{
    public static CommissionAgreementId New() => new(Uuid7.New());

    public static CommissionAgreementId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CommissionAgreementId cannot be empty.", nameof(value));
        }

        return new CommissionAgreementId(value);
    }

    public override string ToString() => Value.ToString("D");
}

public readonly record struct AgencyOfferCommissionOverrideId(Guid Value) : IEquatable<AgencyOfferCommissionOverrideId>
{
    public static AgencyOfferCommissionOverrideId New() => new(Uuid7.New());

    public static AgencyOfferCommissionOverrideId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("AgencyOfferCommissionOverrideId cannot be empty.", nameof(value));
        }

        return new AgencyOfferCommissionOverrideId(value);
    }

    public override string ToString() => Value.ToString("D");
}

public readonly record struct CommercialObligationId(Guid Value) : IEquatable<CommercialObligationId>
{
    public static CommercialObligationId New() => new(Uuid7.New());

    public static CommercialObligationId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CommercialObligationId cannot be empty.", nameof(value));
        }

        return new CommercialObligationId(value);
    }

    public override string ToString() => Value.ToString("D");
}

public readonly record struct SettlementPeriodId(Guid Value) : IEquatable<SettlementPeriodId>
{
    public static SettlementPeriodId New() => new(Uuid7.New());

    public static SettlementPeriodId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SettlementPeriodId cannot be empty.", nameof(value));
        }

        return new SettlementPeriodId(value);
    }

    public override string ToString() => Value.ToString("D");
}

public readonly record struct SettlementRecordId(Guid Value) : IEquatable<SettlementRecordId>
{
    public static SettlementRecordId New() => new(Uuid7.New());

    public static SettlementRecordId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("SettlementRecordId cannot be empty.", nameof(value));
        }

        return new SettlementRecordId(value);
    }

    public override string ToString() => Value.ToString("D");
}

public readonly record struct PayoutInstructionId(Guid Value) : IEquatable<PayoutInstructionId>
{
    public static PayoutInstructionId New() => new(Uuid7.New());

    public static PayoutInstructionId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("PayoutInstructionId cannot be empty.", nameof(value));
        }

        return new PayoutInstructionId(value);
    }

    public override string ToString() => Value.ToString("D");
}

/// <summary>Logical AgencyProfile reference — no agency_marketplace FK.</summary>
public readonly record struct CommercialFinanceAgencyProfileId(Guid Value) : IEquatable<CommercialFinanceAgencyProfileId>
{
    public static CommercialFinanceAgencyProfileId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CommercialFinanceAgencyProfileId cannot be empty.", nameof(value));
        }

        return new CommercialFinanceAgencyProfileId(value);
    }

    public override string ToString() => Value.ToString("D");
}

/// <summary>Logical AgencyOffer reference — no AgencyOffer mutation or cross-schema FK.</summary>
public readonly record struct CommercialFinanceAgencyOfferId(Guid Value) : IEquatable<CommercialFinanceAgencyOfferId>
{
    public static CommercialFinanceAgencyOfferId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CommercialFinanceAgencyOfferId cannot be empty.", nameof(value));
        }

        return new CommercialFinanceAgencyOfferId(value);
    }

    public override string ToString() => Value.ToString("D");
}

/// <summary>Logical Booking reference — no booking schema FK.</summary>
public readonly record struct CommercialFinanceBookingId(Guid Value) : IEquatable<CommercialFinanceBookingId>
{
    public static CommercialFinanceBookingId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CommercialFinanceBookingId cannot be empty.", nameof(value));
        }

        return new CommercialFinanceBookingId(value);
    }

    public override string ToString() => Value.ToString("D");
}

/// <summary>Logical Payment reference — no payment schema FK.</summary>
public readonly record struct CommercialFinancePaymentId(Guid Value) : IEquatable<CommercialFinancePaymentId>
{
    public static CommercialFinancePaymentId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CommercialFinancePaymentId cannot be empty.", nameof(value));
        }

        return new CommercialFinancePaymentId(value);
    }

    public override string ToString() => Value.ToString("D");
}
