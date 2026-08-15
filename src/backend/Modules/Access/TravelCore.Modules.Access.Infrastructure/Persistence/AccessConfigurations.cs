using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelCore.Modules.Access.Domain;
using PermissionEntity = TravelCore.Modules.Access.Domain.Permission;
using RoleEntity = TravelCore.Modules.Access.Domain.Role;
using RolePermissionEntity = TravelCore.Modules.Access.Domain.RolePermission;

namespace TravelCore.Modules.Access.Infrastructure.Persistence;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<PermissionEntity>
{
    public void Configure(EntityTypeBuilder<PermissionEntity> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id")
            .HasConversion(id => id.Value, v => PermissionId.From(v));
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(PermissionEntity.CodeMaxLength).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(PermissionEntity.NameMaxLength).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_permissions_code");
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id")
            .HasConversion(id => id.Value, v => RoleId.From(v));
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(RoleEntity.CodeMaxLength).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(RoleEntity.NameMaxLength).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("ux_roles_code");
        builder.HasMany(x => x.Permissions)
            .WithOne()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Permissions).AutoInclude();
        builder.Metadata.FindNavigation(nameof(RoleEntity.Permissions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissionEntity>
{
    public void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(x => new { x.RoleId, x.PermissionId });
        builder.Property(x => x.RoleId).HasColumnName("role_id")
            .HasConversion(id => id.Value, v => RoleId.From(v));
        builder.Property(x => x.PermissionId).HasColumnName("permission_id")
            .HasConversion(id => id.Value, v => PermissionId.From(v));
        builder.Property(x => x.GrantedAt).HasColumnName("granted_at").IsRequired();
        builder.HasOne<PermissionEntity>()
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SubjectRoleAssignmentConfiguration : IEntityTypeConfiguration<SubjectRoleAssignment>
{
    public void Configure(EntityTypeBuilder<SubjectRoleAssignment> builder)
    {
        builder.ToTable("subject_role_assignments");
        builder.HasKey(x => new { x.SubjectKind, x.SubjectId, x.RoleId });
        builder.Property(x => x.SubjectKind).HasColumnName("subject_kind").HasConversion<short>().IsRequired();
        builder.Property(x => x.SubjectId).HasColumnName("subject_id").IsRequired();
        builder.Property(x => x.RoleId).HasColumnName("role_id")
            .HasConversion(id => id.Value, v => RoleId.From(v));
        builder.Property(x => x.AssignedAt).HasColumnName("assigned_at").IsRequired();
        builder.HasIndex(x => new { x.SubjectKind, x.SubjectId })
            .HasDatabaseName("ix_subject_role_assignments_subject");
        builder.HasOne<RoleEntity>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
