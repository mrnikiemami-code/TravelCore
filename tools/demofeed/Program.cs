using System.Reflection;

namespace TravelCore.Tools.DemoFeed;

/// <summary>
/// Temporary DEMOFEED feeder host (TC-DEMOFEED-T002).
/// Removable. Not an ITravelCoreModule. Not composed into TravelCore.Api.
/// </summary>
internal static class Program
{
    private const string ToolId = "TravelCore.Tools.DemoFeed";
    private const string TaskId = "TC-DEMOFEED-T002";
    private const string DemoSlugPrefix = "demofeed-";

    private static int Main(string[] args)
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
            "seed" => FailClosed("seed", "TC-DEMOFEED-T003+ (Destination / Place / Tour / Media)"),
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
        Console.WriteLine($"Task: {TaskId}");
        Console.WriteLine("Kind: temporary removable feeder host/boundary");
        Console.WriteLine("Production module registration: NO");
        Console.WriteLine("Domain migrations: NO");
        Console.WriteLine($"Demo identity prefix (planned): {DemoSlugPrefix}*");
        Console.WriteLine($"Assembly version: {version}");
        Console.WriteLine("Seed commands: fail-closed until later DEMOFEED tasks");
        return 0;
    }

    private static int Boundaries()
    {
        Console.WriteLine("DEMOFEED architecture boundaries (fail-closed):");
        Console.WriteLine("- Location: tools/demofeed (outside Modules/*)");
        Console.WriteLine("- Must NOT implement ITravelCoreModule");
        Console.WriteLine("- Must NOT appear in TravelCore.Api Program.cs module list");
        Console.WriteLine("- Must NOT add demofeed PostgreSQL schema / domain migrations");
        Console.WriteLine("- Future writes only via Destination / Place / Tour / Media owner paths");
        Console.WriteLine("- Forbidden: Booking · Payment · Pricing · scraping · competitor copy");
        Console.WriteLine("- Demo rows must remain identifiable for purge (slug/code prefix)");
        return 0;
    }

    private static int FailClosed(string command, string unlockTask)
    {
        Console.Error.WriteLine(
            $"DEMOFEED '{command}' is fail-closed in {TaskId}. Authorized later by: {unlockTask}.");
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
        Console.WriteLine($"{ToolId} — temporary DEMOFEED feeder ({TaskId})");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project tools/demofeed -- status");
        Console.WriteLine("  dotnet run --project tools/demofeed -- boundaries");
        Console.WriteLine("  dotnet run --project tools/demofeed -- help");
        Console.WriteLine();
        Console.WriteLine("Fail-closed until later tasks:");
        Console.WriteLine("  seed   → T003+");
        Console.WriteLine("  purge  → GATE / authorized purge");
    }
}
