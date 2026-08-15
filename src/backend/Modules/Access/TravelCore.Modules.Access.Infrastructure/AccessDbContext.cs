using Microsoft.EntityFrameworkCore;
using PermissionEntity = TravelCore.Modules.Access.Domain.Permission;
using RoleEntity = TravelCore.Modules.Access.Domain.Role;
using RolePermissionEntity = TravelCore.Modules.Access.Domain.RolePermission;

namespace TravelCore.Modules.Access.Infrastructure;

/// <summary>
/// Access-owned DbContext. Owns PostgreSQL schema <c>access</c>.
/// </summary>
public sealed class AccessDbContext : DbContext
{
    public const string SchemaName = "access";

    public AccessDbContext(DbContextOptions<AccessDbContext> options)
        : base(options)
    {
    }

    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();

    public DbSet<RoleEntity> Roles => Set<RoleEntity>();

    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccessDbContext).Assembly);
    }
}
