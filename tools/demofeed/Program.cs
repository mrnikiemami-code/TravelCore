using System.Reflection;
using TravelCore.Tools.DemoFeed;

internal static class Program
{
    private const string ToolId = "TravelCore.Tools.DemoFeed";
    private const string BoundaryTaskId = "TC-DEMOFEED-T002";
    private const string DestinationSeedTaskId = "TC-DEMOFEED-T003";

    private static async Task<int> Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintHelp();
            return 0;
        }

        return args[0].ToLowerInvariant() switch
        {
            "status" => Status(),
            "boundaries" => Boundaries(),
            "seed" => await SeedAsync(args),
            "list" => await ListAsync(args),
            "ensure-schema" => await EnsureSchemaAsync(args),
            "purge" => FailClosed("purge", "TC-DEMOFEED-GATE deletion strategy / authorized purge"),
            _ => Unknown(args[0]),
        };
    }

    private static bool IsHelp(string value) =>
        value is "-h" or "--help" or "help" or "/?";

    private static int Status()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";
        Console.WriteLine($"{ToolId}");
        Console.WriteLine($"Boundary task: {BoundaryTaskId}");
        Console.WriteLine($"Destination seed task: {DestinationSeedTaskId}");
        Console.WriteLine("Kind: temporary removable feeder host/boundary");
        Console.WriteLine("Production module registration: NO");
        Console.WriteLine("Domain migrations owned by: ReferenceDataMigrator / DestinationMigrator");
        Console.WriteLine($"Demo identity prefix: {DemoFeedHost.DemoCodePrefix}*");
        Console.WriteLine($"Assembly version: {version}");
        return 0;
    }

    private static int Boundaries()
    {
        Console.WriteLine("DEMOFEED architecture boundaries (fail-closed):");
        Console.WriteLine("- Location: tools/demofeed (outside Modules/*)");
        Console.WriteLine("- Must NOT implement ITravelCoreModule in TravelCore.Api");
        Console.WriteLine("- Must NOT appear in TravelCore.Api Program.cs module list");
        Console.WriteLine("- No demofeed PostgreSQL schema / demofeed migrations");
        Console.WriteLine("- Destination writes only via DestinationApplicationService");
        Console.WriteLine("- Schema apply only via owner migrators when --ensure-schema");
        Console.WriteLine("- Forbidden: Booking · Payment · Pricing · scraping · competitor copy");
        return 0;
    }

    private static async Task<int> SeedAsync(string[] args)
    {
        var rest = args.Skip(1).ToArray();
        if (rest.Length == 0 || !rest[0].Equals("destinations", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Usage: seed destinations [--ensure-schema] [--connection <cs>]");
            Console.Error.WriteLine("T004+ seeds are not authorized in TC-DEMOFEED-T003.");
            return 2;
        }

        var ensureSchema = rest.Any(a => a.Equals("--ensure-schema", StringComparison.OrdinalIgnoreCase));
        try
        {
            var configuration = DemoFeedHost.BuildConfiguration(args);
            await using var services = DemoFeedHost.BuildServices(configuration);
            return await DestinationDemoSeed.SeedAsync(services, ensureSchema, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DEMOFEED seed failed: {ex.Message}");
            return 3;
        }
    }

    private static async Task<int> ListAsync(string[] args)
    {
        try
        {
            var configuration = DemoFeedHost.BuildConfiguration(args);
            await using var services = DemoFeedHost.BuildServices(configuration);
            return await DestinationDemoSeed.ListAsync(services, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DEMOFEED list failed: {ex.Message}");
            return 3;
        }
    }

    private static async Task<int> EnsureSchemaAsync(string[] args)
    {
        try
        {
            var configuration = DemoFeedHost.BuildConfiguration(args);
            await using var services = DemoFeedHost.BuildServices(configuration);
            return await DestinationDemoSeed.EnsureSchemaAsync(services, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"DEMOFEED ensure-schema failed: {ex.Message}");
            return 3;
        }
    }

    private static int FailClosed(string command, string unlockTask)
    {
        Console.Error.WriteLine(
            $"DEMOFEED '{command}' is fail-closed. Authorized later by: {unlockTask}.");
        return 2;
    }

    private static int Unknown(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintHelp();
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine($"{ToolId} — temporary DEMOFEED feeder");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project tools/demofeed -- status");
        Console.WriteLine("  dotnet run --project tools/demofeed -- boundaries");
        Console.WriteLine("  dotnet run --project tools/demofeed -- ensure-schema --connection \"...\"");
        Console.WriteLine("  dotnet run --project tools/demofeed -- seed destinations --ensure-schema --connection \"...\"");
        Console.WriteLine("  dotnet run --project tools/demofeed -- list --connection \"...\"");
        Console.WriteLine();
        Console.WriteLine("Connection: ConnectionStrings__TravelCore env var or --connection");
        Console.WriteLine("Fail-closed: purge (GATE) · seed hotels/tours (T004+)");
    }
}
