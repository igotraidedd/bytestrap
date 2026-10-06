using Bytestrap.Core.Flags;
using Bytestrap.Core.Util;

namespace Bytestrap.Linux.Commands;

public static class ConfigCommands
{
    public static int Run(AppContext ctx, ParsedArgs args)
    {
        return args.Subcommand switch
        {
            "show" or "" => Show(ctx),
            "set" => Set(ctx, args),
            "get" => Get(ctx, args),
            "path" => Path(ctx),
            _ => Usage(),
        };
    }

    private static int Show(AppContext ctx)
    {
        var s = ctx.Settings;

        if (ctx.JsonOutput)
        {
            Console.WriteLine(Json.Options.WriteIndented(s));
            return 0;
        }

        Cli.WriteLine($"winePrefix:  {Empty(s.WinePrefix)}");
        Cli.WriteLine($"runner:      {Empty(s.Runner)}");
        Cli.WriteLine($"runnerArgs:  {Empty(s.RunnerArgs)}");
        Cli.WriteLine($"launchArgs:  {Empty(s.LaunchArgs)}");
        Cli.WriteLine($"useGameMode: {s.UseGameMode}");
        Cli.WriteLine($"esync:       {s.EnableEsync}");
        Cli.WriteLine($"fsync:       {s.EnableFsync}");
        Cli.WriteLine($"extraPrefixes: {(s.ExtraWinePrefixes.Count == 0 ? "(none)" : string.Join(", ", s.ExtraWinePrefixes))}");
        Cli.WriteLine($"extraEnv:      {(s.ExtraEnv.Count == 0 ? "(none)" : string.Join(", ", s.ExtraEnv.Select(p => $"{p.Key}={p.Value}")))}");
        return 0;
    }

    private static string Empty(string v) => string.IsNullOrEmpty(v) ? "(auto)" : v;

    private static int Get(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 1)
        {
            Cli.Err("usage: bytestrap config get <key>");
            return 2;
        }

        string? value = GetKey(ctx, args.Positionals[0]);
        if (value is null)
        {
            Cli.Err($"Unknown key '{args.Positionals[0]}'.");
            return 2;
        }

        Cli.WriteLine(value);
        return 0;
    }

    private static int Set(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 2)
        {
            Cli.Err("usage: bytestrap config set <key> <value>");
            return 2;
        }

        if (!SetKey(ctx, args.Positionals[0], args.Positionals[1]))
        {
            Cli.Err($"Unknown key '{args.Positionals[0]}'.");
            return 2;
        }

        ctx.SaveSettings();
        Cli.Ok($"{args.Positionals[0]} = {args.Positionals[1]}");
        return 0;
    }

    private static int Path(AppContext ctx)
    {
        Cli.WriteLine(Bytestrap.Core.Config.Paths.SettingsFile);
        return 0;
    }

    private static string? GetKey(AppContext ctx, string key)
    {
        var s = ctx.Settings;
        return key.ToLowerInvariant() switch
        {
            "wineprefix" or "prefix" => s.WinePrefix,
            "runner" => s.Runner,
            "runnerargs" => s.RunnerArgs,
            "launchargs" => s.LaunchArgs,
            "usegamemode" or "gamemode" => s.UseGameMode.ToString(),
            "esync" => s.EnableEsync.ToString(),
            "fsync" => s.EnableFsync.ToString(),
            _ => null,
        };
    }

    private static bool SetKey(AppContext ctx, string key, string value)
    {
        var s = ctx.Settings;
        switch (key.ToLowerInvariant())
        {
            case "wineprefix":
            case "prefix":
                s.WinePrefix = value; return true;
            case "runner":
                s.Runner = value; return true;
            case "runnerargs":
                s.RunnerArgs = value; return true;
            case "launchargs":
                s.LaunchArgs = value; return true;
            case "usegamemode":
            case "gamemode":
                if (!bool.TryParse(value, out bool gm)) return false;
                s.UseGameMode = gm; return true;
            case "esync":
                if (!bool.TryParse(value, out bool es)) return false;
                s.EnableEsync = es; return true;
            case "fsync":
                if (!bool.TryParse(value, out bool fs)) return false;
                s.EnableFsync = fs; return true;
            default:
                return false;
        }
    }

    public static int Usage()
    {
        Cli.WriteLine("usage:");
        Cli.WriteLine("  bytestrap config show [--json]");
        Cli.WriteLine("  bytestrap config get <key>");
        Cli.WriteLine("  bytestrap config set <key> <value>");
        Cli.WriteLine("  bytestrap config path");
        Cli.WriteLine("keys: prefix, runner, runnerArgs, launchArgs, gamemode, esync, fsync");
        return 2;
    }
}

public static class ImportExportCommands
{
    public static int Export(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 1)
        {
            Cli.Err("usage: bytestrap export <file.json>");
            return 2;
        }

        string file = args.Positionals[0];
        File.WriteAllText(file, Json.Options.WriteIndented(ctx.MasterFlags.Flags));
        Cli.Ok($"Exported {ctx.MasterFlags.Flags.Count} flags to {file}");
        return 0;
    }

    public static int Import(AppContext ctx, ParsedArgs args)
    {
        if (args.Positionals.Count < 1)
        {
            Cli.Err("usage: bytestrap import <file.json> [--replace] [--no-apply]");
            return 2;
        }

        string file = args.Positionals[0];
        if (!File.Exists(file))
        {
            Cli.Err($"File not found: {file}");
            return 1;
        }

        Dictionary<string, object?>? parsed;
        try
        {
            parsed = Json.Options.Read<Dictionary<string, object?>>(File.ReadAllText(file));
        }
        catch (Exception ex)
        {
            Cli.Err($"Invalid JSON: {ex.Message}");
            return 1;
        }

        if (parsed is null || parsed.Count == 0)
        {
            Cli.Warn("No flags found in file.");
            return 1;
        }

        // Accept both raw FFlags and friendly preset keys (Windows export format).
        var normalized = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in parsed)
            normalized[FlagPresets.ResolveFlagName(pair.Key)] = pair.Value?.ToString() ?? string.Empty;

        if (args.Has("replace"))
            ctx.MasterFlags.ClearByPrefix(string.Empty);

        ctx.MasterFlags.Merge(normalized);
        ctx.SaveMaster();

        Cli.Ok($"Imported {normalized.Count} flags{(args.Has("replace") ? " (replaced)" : " (merged)")}.");
        return FlagCommands.MaybeApply(ctx, args);
    }
}
