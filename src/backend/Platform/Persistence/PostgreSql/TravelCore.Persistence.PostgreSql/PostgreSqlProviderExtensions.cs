using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace TravelCore.Persistence.PostgreSql;

/// <summary>
/// Shared Npgsql / EF Core provider policy for module-owned DbContexts.
/// Does not open connections, register a global DbContext, or own module models.
/// </summary>
public static class PostgreSqlProviderExtensions
{
    /// <summary>
    /// Applies TravelCore's PostgreSQL provider configuration to <paramref name="optionsBuilder"/>.
    /// The caller supplies an already-resolved connection string (no secret ownership here).
    /// Includes official NodaTime mapping policy (ADR 0004).
    /// When <paramref name="migrationsHistorySchema"/> is provided, EF history lives in that module schema
    /// (not <c>public</c>); the provider never invents or hard-codes a module schema name.
    /// </summary>
    public static DbContextOptionsBuilder UseTravelCorePostgreSql(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        string? migrationsHistorySchema = null)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // سیاست مشترک ارائه‌دهنده + NodaTime؛ schema تاریخچه migration فقط وقتی مالک ماژول صریح بدهد.
        return optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => ConfigureNpgsql(npgsql, migrationsHistorySchema));
    }

    /// <inheritdoc cref="UseTravelCorePostgreSql(DbContextOptionsBuilder, string, string?)"/>
    public static DbContextOptionsBuilder<TContext> UseTravelCorePostgreSql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string connectionString,
        string? migrationsHistorySchema = null)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => ConfigureNpgsql(npgsql, migrationsHistorySchema));
    }

    private static void ConfigureNpgsql(
        NpgsqlDbContextOptionsBuilder npgsql,
        string? migrationsHistorySchema)
    {
        npgsql.UseNodaTime();

        if (!string.IsNullOrWhiteSpace(migrationsHistorySchema))
        {
            npgsql.MigrationsHistoryTable(
                HistoryRepository.DefaultTableName,
                migrationsHistorySchema);
        }
    }
}
