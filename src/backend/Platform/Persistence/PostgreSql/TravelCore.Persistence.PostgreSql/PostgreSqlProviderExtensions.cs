using Microsoft.EntityFrameworkCore;
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
    /// </summary>
    public static DbContextOptionsBuilder UseTravelCorePostgreSql(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // سیاست مشترک ارائه‌دهنده + NodaTime؛ بدون retry/timeout speculative و بدون باز کردن اتصال در startup.
        return optionsBuilder.UseNpgsql(connectionString, ConfigureNpgsql);
    }

    /// <inheritdoc cref="UseTravelCorePostgreSql(DbContextOptionsBuilder, string)"/>
    public static DbContextOptionsBuilder<TContext> UseTravelCorePostgreSql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string connectionString)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        return optionsBuilder.UseNpgsql(connectionString, ConfigureNpgsql);
    }

    private static void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder npgsql)
    {
        npgsql.UseNodaTime();
    }
}
