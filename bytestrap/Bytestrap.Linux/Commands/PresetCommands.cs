using Bytestrap.Core.Flags;
using Bytestrap.Core.Util;

namespace Bytestrap.Linux.Commands;

public static class PresetCommands
{
    public static int Run(AppContext ctx, ParsedArgs args)
    {
        return args.Subcommand switch
        {
            "list" or "ls" or "" => List(ctx),
            "show" => Show(ctx, args),
            "apply" => Apply(ctx, args),
            _ => Usage(),
        };
    }

    private static int List(AppContext ctx)
    {
        if (ctx.JsonOutput)
        {
            var payload = PerformanceProfiles.All.ToDictionary(
                p => p.Key,
                p => new { description = p.Value.Description, flags = p.Value.Flags.Count });
            Console.WriteLine(Json.Options.WriteIndented(payload));
            return 0;
        }

        Cli.Head("Available performance presets:");
        foreach (var pair in PerformanceProfiles.All)
            Cli.WriteLine($"  {pair.Key,-18} {pair.Value.Description} ({pair.Value.Flags.Count} flags)");

        return 0;
    }

    private static int Show(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 1)
        {
            Cli.Err("usage: bytestrap preset show <name>");
            return 2;
        }

        if (!PerformanceProfiles.All.TryGetValue(args.Positionals[0], out var profile))
        {
            Cli.Err($"Unknown preset '{args.Positionals[0]}'. See: bytestrap preset list");
            return 2;
        }

        var rendered = PerformanceProfiles.Render(profile);

        if (ctx.JsonOutput)
        {
            Console.WriteLine(Json.Options.WriteIndented(rendered));
            return 0;
        }

        Cli.Head($"{profile.Name} - {profile.Description}");
        foreach (var pair in rendered.OrderBy(p => p.Key, StringComparer.Ordinal))
            Cli.WriteLine($"  {pair.Key} = {pair.Value}");

        return 0;
    }

    private static int Apply(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 1)
        {
            Cli.Err("usage: bytestrap preset apply <name> [--no-apply]");
            return 2;
        }

        if (!PerformanceProfiles.All.TryGetValue(args.Positionals[0], out var profile))
        {
            Cli.Err($"Unknown preset '{args.Positionals[0]}'. See: bytestrap preset list");
            return 2;
        }

        var rendered = PerformanceProfiles.Render(profile);
        ctx.MasterFlags.Merge(rendered);
        ctx.SaveMaster();

        Cli.Ok($"Applied preset '{profile.Name}' ({rendered.Count} flags).");

        return FlagCommands.MaybeApply(ctx, args);
    }

    public static int Usage()
    {
        Cli.WriteLine("usage:");
        Cli.WriteLine("  bytestrap preset list [--json]");
        Cli.WriteLine("  bytestrap preset show <name> [--json]");
        Cli.WriteLine("  bytestrap preset apply <name> [--no-apply]");
        return 2;
    }
}
