using Bytestrap.Linux.Commands;

namespace Bytestrap.Linux;

public static class Program
{
    public const string Version = "3.1.0";

    public static int Main(string[] argv)
    {
        var args = Cli.Parse(argv);

        if (args.Command is "" or "help" or "-h" or "--help")
            return Help(args.Subcommand);

        if (args.Command is "-v" or "--version" or "version")
        {
            Cli.WriteLine($"bytestrap {Version} (linux)");
            return 0;
        }

        try
        {
            var ctx = AppContext.Create(args);

            return args.Command switch
            {
                "status" => StatusCommands.Status(ctx),
                "versions" => StatusCommands.Versions(ctx),
                "apply" => StatusCommands.Apply(ctx),
                "flags" or "flag" => FlagCommands.Run(ctx, args),
                "preset" or "presets" or "profile" => PresetCommands.Run(ctx, args),
                "export" => ImportExportCommands.Export(ctx, args),
                "import" => ImportExportCommands.Import(ctx, args),
                "launch" or "play" or "run" => LaunchCommands.Launch(ctx, args),
                "kill" => LaunchCommands.Kill(ctx),
                "clean" => LaunchCommands.Clean(ctx, args),
                "config" or "cfg" => ConfigCommands.Run(ctx, args),
                "install-desktop" => InstallDesktop(ctx),
                _ => Unknown(args.Command),
            };
        }
        catch (Exception ex)
        {
            Cli.Err($"error: {ex.Message}");
            return 1;
        }
    }

    private static int Unknown(string command)
    {
        Cli.Err($"Unknown command '{command}'.");
        Cli.WriteLine("Run 'bytestrap help' for usage.");
        return 2;
    }

    private static int InstallDesktop(AppContext ctx)
    {
        string home = Environment.GetEnvironmentVariable("HOME") ?? "/tmp";
        string applicationsDir = Path.Combine(home, ".local/share/applications");
        string desktopFile = Path.Combine(applicationsDir, "bytestrap.desktop");

        string? selfPath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(selfPath))
        {
            Cli.Err("Could not determine the bytestrap binary path.");
            return 1;
        }

        string iconPath = FindIcon();
        string desktop = $"""
            [Desktop Entry]
            Type=Application
            Name=Bytestrap
            Comment=Launch Roblox with Bytestrap performance flags
            Exec={selfPath} launch
            Icon={iconPath}
            Categories=Game;
            Terminal=false
            StartupNotify=false
            """;

        Directory.CreateDirectory(applicationsDir);
        File.WriteAllText(desktopFile, desktop.Replace("\r\n", "\n") + "\n");

        Cli.Ok($"Installed {desktopFile}");
        return 0;
    }

    private static string FindIcon()
    {
        string home = Environment.GetEnvironmentVariable("HOME") ?? "/tmp";

        // Prefer an installed icon, else fall back to a stock one.
        string[] candidates =
        {
            Path.Combine(home, ".local/share/icons/bytestrap.png"),
            "/usr/share/icons/bytestrap.png",
            "/usr/share/pixmaps/bytestrap.png",
        };

        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
                return candidate;
        }

        return "applications-games";
    }

    private static int Help(string topic)
    {
        if (!string.IsNullOrEmpty(topic))
        {
            return topic switch
            {
                "flags" or "flag" => FlagCommands.Usage(),
                "preset" or "presets" or "profile" => PresetCommands.Usage(),
                "config" or "cfg" => ConfigCommands.Usage(),
                _ => GeneralHelp(),
            };
        }

        return GeneralHelp();
    }

    private static int GeneralHelp()
    {
        Cli.WriteLine($"bytestrap {Version} (linux) - roblox but it actually runs well");
        Cli.WriteLine();
        Cli.WriteLine("usage: bytestrap <command> [args] [options]");
        Cli.WriteLine();
        Cli.WriteLine("  status [--json] [--prefix DIR]     show runner, installs, flag count");
        Cli.WriteLine("  versions                           list detected Roblox versions");
        Cli.WriteLine("  flags ...                          list/get/set/unset/clear fast flags");
        Cli.WriteLine("  preset ...                         list/show/apply performance presets");
        Cli.WriteLine("  apply                              deploy flags to all installs");
        Cli.WriteLine("  export <file> / import <file>      backup/restore flag sets (JSON)");
        Cli.WriteLine("  launch [uri] [--studio]            launch Roblox via wine/proton");
        Cli.WriteLine("  kill                               kill running Roblox processes");
        Cli.WriteLine("  clean [--deep]                     delete Roblox logs (/downloads)");
        Cli.WriteLine("  config ...                         show/get/set bytestrap settings");
        Cli.WriteLine("  install-desktop                    add a launcher to your app menu");
        Cli.WriteLine();
        Cli.WriteLine("global options:");
        Cli.WriteLine("  --prefix DIR     use this Wine prefix (overrides $WINEPREFIX + config)");
        Cli.WriteLine("  --runner BIN     use this runner binary (wine/proton path)");
        Cli.WriteLine("  --json           machine-readable output (where supported)");
        Cli.WriteLine("  --no-apply       change master flags without deploying to installs");
        Cli.WriteLine();
        Cli.WriteLine("examples:");
        Cli.WriteLine("  bytestrap preset apply max-fps");
        Cli.WriteLine("  bytestrap flags set Performance.UnlockFPS 240");
        Cli.WriteLine("  bytestrap launch \"roblox://placeId=1818\"");
        Cli.WriteLine("  bytestrap help flags");
        return 0;
    }
}
