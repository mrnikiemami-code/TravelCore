using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Identity.Domain;
using AccountAggregate = TravelCore.Modules.Identity.Domain.Account;

namespace TravelCore.Modules.Identity.Infrastructure.Persistence;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<AccountAggregate>
{
    public void Configure(EntityTypeBuilder<AccountAggregate> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .HasConversion(id => id.Value, value => AccountId.From(value));

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(AccountAggregate.EmailMaxLength)
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasColumnName("normalized_email")
            .HasMaxLength(AccountAggregate.EmailMaxLength)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(AccountAggregate.PasswordHashMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<short>()
            .IsRequired();

        // Opaque UUID reference only — no FK into party schema (ADR 0001 / Identity≠Party).
        builder.Property(x => x.AssociatedPartyId)
            .HasColumnName("associated_party_id");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(x => x.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("ux_accounts_normalized_email");

        builder.HasIndex(x => x.AssociatedPartyId)
            .HasDatabaseName("ix_accounts_associated_party_id");
    }
}
