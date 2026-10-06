using Bytestrap.Core.Flags;
using Bytestrap.Core.Util;

namespace Bytestrap.Linux.Commands;

public static class FlagCommands
{
    public static int Run(AppContext ctx, ParsedArgs args)
    {
        return args.Subcommand switch
        {
            "list" or "ls" or "" => List(ctx, args),
            "get" => Get(ctx, args),
            "set" => Set(ctx, args),
            "unset" or "rm" or "delete" => Unset(ctx, args),
            "clear" => Clear(ctx, args),
            _ => Usage(),
        };
    }

    private static int List(AppContext ctx, ParsedArgs args)
    {
        string prefix = args.Positionals.FirstOrDefault() ?? string.Empty;

        var flags = ctx.MasterFlags.Flags
            .Where(p => prefix.Length == 0 || p.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.Key, StringComparer.Ordinal)
            .ToArray();

        if (ctx.JsonOutput)
        {
            Console.WriteLine(Json.Options.WriteIndented(flags.ToDictionary(p => p.Key, p => p.Value)));
            return 0;
        }

        if (flags.Length == 0)
        {
            Cli.WriteLine(prefix.Length == 0 ? "No flags set." : $"No flags matching '{prefix}'.");
            return 0;
        }

        // Show friendly preset key alongside the raw FFlag when known.
        var reverse = FlagPresets.PresetFlags
            .GroupBy(p => p.Value, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().Key, StringComparer.OrdinalIgnoreCase);

        foreach (var pair in flags)
        {
            if (reverse.TryGetValue(pair.Key, out string? preset))
                Cli.WriteLine($"{pair.Key} = {pair.Value}   ({preset})");
            else
                Cli.WriteLine($"{pair.Key} = {pair.Value}");
        }

        Cli.Dim($"-- {flags.Length} flag(s) --");
        return 0;
    }

    private static int Get(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 1)
        {
            Cli.Err("usage: bytestrap flags get <fflag-or-preset>");
            return 2;
        }

        string name = args.Positionals[0];
        string? value = ctx.MasterFlags.Get(name);

        if (value is null)
        {
            Cli.WriteLine("(unset)");
            return 1;
        }

        Cli.WriteLine(value);
        return 0;
    }

    private static int Set(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 2)
        {
            Cli.Err("usage: bytestrap flags set <fflag-or-preset> <value>");
            return 2;
        }

        string name = args.Positionals[0];
        string value = args.Positionals[1];

        if (!FlagPresets.IsPresetKey(name) && !FlagPresets.IsPresetFlag(name))
            Cli.Warn($"'{name}' is not a known preset - setting as a raw FFlag.");

        ctx.MasterFlags.Set(name, value);
        ctx.SaveMaster();

        string resolved = FlagPresets.ResolveFlagName(name);
        Cli.Ok($"Set {resolved} = {value}");

        return MaybeApply(ctx, args);
    }

    private static int Unset(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 1)
        {
            Cli.Err("usage: bytestrap flags unset <fflag-or-preset>");
            return 2;
        }

        string name = args.Positionals[0];

        if (!ctx.MasterFlags.Unset(name))
        {
            Cli.Warn($"'{FlagPresets.ResolveFlagName(name)}' was not set.");
            return 1;
        }

        ctx.SaveMaster();
        Cli.Ok($"Unset {FlagPresets.ResolveFlagName(name)}");

        return MaybeApply(ctx, args);
    }

    private static int Clear(AppContext ctx, ParsedArgs args)
    {
        string prefix = args.Positionals.FirstOrDefault() ?? string.Empty;

        if (!args.Has("yes", "y") && !Confirm($"Remove ALL{(prefix.Length > 0 ? $" '{prefix}'" : "")} flags? [y/N] "))
            return 1;

        int removed = prefix.Length == 0
            ? ClearAll(ctx)
            : ctx.MasterFlags.ClearByPrefix(prefix);

        ctx.SaveMaster();
        Cli.Ok($"Removed {removed} flag(s).");

        return MaybeApply(ctx, args);
    }

    private static int ClearAll(AppContext ctx)
    {
        int count = ctx.MasterFlags.Flags.Count;
        ctx.MasterFlags.ClearByPrefix(string.Empty);
        return count;
    }

    /// <summary>Deploy to installs unless --no-apply was passed.</summary>
    public static int MaybeApply(AppContext ctx, ParsedArgs args)
    {
        if (args.Has("no-apply"))
        {
            Cli.Dim("(skipped deploy: --no-apply)");
            return 0;
        }

        var installs = ctx.Installs();
        if (installs.Count == 0)
        {
            Cli.Dim("(no installs detected - saved to master copy only)");
            return 0;
        }

        var result = FlagDeployer.ApplyToInstalls(ctx.MasterFlags.Flags, installs);
        Cli.Dim($"deployed to {result.VersionsUpdated} version dir(s)");

        foreach (string error in result.Errors)
            Cli.Err($"  failed: {error}");

        return result.Errors.Count == 0 ? 0 : 1;
    }

    private static bool Confirm(string prompt)
    {
        Console.Write(prompt);
        string? answer = Console.ReadLine();
        return answer is not null &&
            (answer.Equals("y", StringComparison.OrdinalIgnoreCase) ||
             answer.Equals("yes", StringComparison.OrdinalIgnoreCase));
    }

    public static int Usage()
    {
        Cli.WriteLine("usage:");
        Cli.WriteLine("  bytestrap flags list [prefix] [--json]");
        Cli.WriteLine("  bytestrap flags get <fflag-or-preset>");
        Cli.WriteLine("  bytestrap flags set <fflag-or-preset> <value> [--no-apply]");
        Cli.WriteLine("  bytestrap flags unset <fflag-or-preset> [--no-apply]");
        Cli.WriteLine("  bytestrap flags clear [prefix] [--yes] [--no-apply]");
        return 2;
    }
}
