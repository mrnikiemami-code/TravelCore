using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.CommercialFinance.Domain;

namespace TravelCore.Modules.CommercialFinance.Infrastructure.Persistence;

internal sealed class CommissionAgreementConfiguration : IEntityTypeConfiguration<CommissionAgreement>
{
    public void Configure(EntityTypeBuilder<CommissionAgreement> builder)
    {
        builder.ToTable("commission_agreements");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => CommissionAgreementId.From(value));

        builder.Property(x => x.AgencyProfileId)
            .HasColumnName("agency_profile_id")
            .HasConversion(id => id.Value, value => CommercialFinanceAgencyProfileId.From(value))
            .IsRequired();

        builder.Property(x => x.MarketPolicy)
            .HasColumnName("market_policy")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.EffectiveFrom)
            .HasColumnName("effective_from")
            .IsRequired();

        builder.Property(x => x.EffectiveTo)
            .HasColumnName("effective_to");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.AgencyProfileId)
            .HasDatabaseName("ix_commission_agreements_agency_profile_id");
    }
}

internal sealed class AgencyOfferCommissionOverrideConfiguration
    : IEntityTypeConfiguration<AgencyOfferCommissionOverride>
{
    public void Configure(EntityTypeBuilder<AgencyOfferCommissionOverride> builder)
    {
        builder.ToTable("agency_offer_commission_overrides");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => AgencyOfferCommissionOverrideId.From(value));

        // Logical agency_offer_id — NO cross-schema FK to agency_marketplace.agency_offers.
        builder.Property(x => x.AgencyOfferId)
            .HasColumnName("agency_offer_id")
            .HasConversion(id => id.Value, value => CommercialFinanceAgencyOfferId.From(value))
            .IsRequired();

        builder.Property(x => x.CommissionAgreementId)
            .HasColumnName("commission_agreement_id")
            .HasConversion(id => id.Value, value => CommissionAgreementId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(AgencyOfferCommissionOverride.NotesMaxLength);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.AgencyOfferId)
            .IsUnique()
            .HasDatabaseName("ux_agency_offer_commission_overrides_agency_offer_id");

        builder.HasOne<CommissionAgreement>()
            .WithMany()
            .HasForeignKey(x => x.CommissionAgreementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CommercialObligationConfiguration : IEntityTypeConfiguration<CommercialObligation>
{
    public void Configure(EntityTypeBuilder<CommercialObligation> builder)
    {
        builder.ToTable("commercial_obligations");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => CommercialObligationId.From(value));

        builder.Property(x => x.AgencyProfileId)
            .HasColumnName("agency_profile_id")
            .HasConversion(id => id.Value, value => CommercialFinanceAgencyProfileId.From(value))
            .IsRequired();

        builder.Property(x => x.AgencyOfferId)
            .HasColumnName("agency_offer_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? CommercialFinanceAgencyOfferId.From(value.Value) : null);

        builder.Property(x => x.BookingId)
            .HasColumnName("booking_id")
            .HasConversion(id => id.Value, value => CommercialFinanceBookingId.From(value))
            .IsRequired();

        builder.Property(x => x.PaymentId)
            .HasColumnName("payment_id")
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? CommercialFinancePaymentId.From(value.Value) : null);

        builder.Property(x => x.LifecycleState)
            .HasColumnName("lifecycle_state")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SourceEventKey)
            .HasColumnName("source_event_key")
            .HasMaxLength(CommercialObligation.SourceEventKeyMaxLength)
            .IsRequired();

        builder.OwnsOptionalMoney(
            x => x.AmountSnapshot,
            amountColumnName: "amount_snapshot_amount",
            currencyColumnName: "amount_snapshot_currency_code");

        builder.Property(x => x.EvidenceSnapshotHash)
            .HasColumnName("evidence_snapshot_hash")
            .HasMaxLength(CommercialObligation.EvidenceSnapshotHashMaxLength);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(x => x.StateChangedAt)
            .HasColumnName("state_changed_at")
            .IsRequired();

        builder.HasIndex(x => x.SourceEventKey)
            .IsUnique()
            .HasDatabaseName("ux_commercial_obligations_source_event_key");

        builder.HasIndex(x => new { x.AgencyProfileId, x.LifecycleState })
            .HasDatabaseName("ix_commercial_obligations_agency_profile_lifecycle");
    }
}

internal sealed class SettlementPeriodConfiguration : IEntityTypeConfiguration<SettlementPeriod>
{
    public void Configure(EntityTypeBuilder<SettlementPeriod> builder)
    {
        builder.ToTable("settlement_periods");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SettlementPeriodId.From(value));

        builder.Property(x => x.AgencyProfileId)
            .HasColumnName("agency_profile_id")
            .HasConversion(id => id.Value, value => CommercialFinanceAgencyProfileId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.PeriodStart)
            .HasColumnName("period_start")
            .IsRequired();

        builder.Property(x => x.PeriodEnd)
            .HasColumnName("period_end")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => new { x.AgencyProfileId, x.Status })
            .HasDatabaseName("ix_settlement_periods_agency_profile_status");
    }
}

internal sealed class SettlementRecordConfiguration : IEntityTypeConfiguration<SettlementRecord>
{
    public void Configure(EntityTypeBuilder<SettlementRecord> builder)
    {
        builder.ToTable("settlement_records");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => SettlementRecordId.From(value));

        builder.Property(x => x.SettlementPeriodId)
            .HasColumnName("settlement_period_id")
            .HasConversion(id => id.Value, value => SettlementPeriodId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.ApprovalRequired)
            .HasColumnName("approval_required")
            .IsRequired();

        builder.Property(x => x.ApprovedAt)
            .HasColumnName("approved_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<SettlementPeriod>()
            .WithMany()
            .HasForeignKey(x => x.SettlementPeriodId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PayoutInstructionConfiguration : IEntityTypeConfiguration<PayoutInstruction>
{
    public void Configure(EntityTypeBuilder<PayoutInstruction> builder)
    {
        builder.ToTable("payout_instructions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => PayoutInstructionId.From(value));

        builder.Property(x => x.SettlementRecordId)
            .HasColumnName("settlement_record_id")
            .HasConversion(id => id.Value, value => SettlementRecordId.From(value))
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        builder.OwnsOptionalMoney(
            x => x.PayoutAmountSnapshot,
            amountColumnName: "payout_amount_snapshot_amount",
            currencyColumnName: "payout_amount_snapshot_currency_code");

        builder.Property(x => x.ApprovalRequired)
            .HasColumnName("approval_required")
            .IsRequired();

        builder.Property(x => x.ApprovedAt)
            .HasColumnName("approved_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne<SettlementRecord>()
            .WithMany()
            .HasForeignKey(x => x.SettlementRecordId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CommercialFinanceEventConsumptionRecordConfiguration
    : IEntityTypeConfiguration<CommercialFinanceEventConsumptionRecord>
{
    public void Configure(EntityTypeBuilder<CommercialFinanceEventConsumptionRecord> builder)
    {
        builder.ToTable("event_consumption_records");
        builder.HasKey(x => new { x.SourceKind, x.SourceEventKey });

        builder.Property(x => x.SourceKind)
            .HasColumnName("source_kind")
            .HasConversion<short>()
            .IsRequired();

        builder.Property(x => x.SourceEventKey)
            .HasColumnName("source_event_key")
            .HasMaxLength(CommercialFinanceEventConsumptionRecord.SourceEventKeyMaxLength)
            .IsRequired();

        builder.Property(x => x.ObligationId)
            .HasColumnName("obligation_id")
            .HasConversion(id => id.Value, value => CommercialObligationId.From(value))
            .IsRequired();

        builder.Property(x => x.ConsumedAt)
            .HasColumnName("consumed_at")
            .IsRequired();

        builder.HasOne<CommercialObligation>()
            .WithMany()
            .HasForeignKey(x => x.ObligationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
