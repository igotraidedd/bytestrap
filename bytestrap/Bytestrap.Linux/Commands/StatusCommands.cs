using Bytestrap.Core.Flags;
using Bytestrap.Core.Roblox;
using Bytestrap.Core.Util;

namespace Bytestrap.Linux.Commands;

public static class StatusCommands
{
    public static int Status(AppContext ctx)
    {
        var installs = ctx.Installs();
        string runner = WineRunner.ResolveRunner(ctx.Settings);
        var sober = FlatpakRunners.Sober();
        var vinegar = FlatpakRunners.Vinegar();

        if (ctx.JsonOutput)
        {
            var payload = new
            {
                runner = string.IsNullOrEmpty(runner) ? null : runner,
                sober = sober.Installed,
                vinegar = vinegar.Installed,
                masterFlags = ctx.MasterFlags.Flags.Count,
                installs = installs.Select(i => new
                {
                    launcher = i.Launcher,
                    prefix = i.WinePrefix,
                    robloxDir = i.RobloxDir,
                    versions = i.Versions.Select(v => new
                    {
                        name = v.Name,
                        player = v.HasPlayer,
                        studio = v.HasStudio,
                    }),
                }),
            };

            Console.WriteLine(Json.Options.WriteIndented(payload));
            return 0;
        }

        Cli.Head("== Bytestrap status ==");
        Cli.WriteLine($"Runner:       {(string.IsNullOrEmpty(runner) ? "(none found)" : runner)}");
        Cli.WriteLine($"Sober:        {(sober.Installed ? $"installed ({sober.Source}) - use it to play" : "not installed")}");
        Cli.WriteLine($"Vinegar:      {(vinegar.Installed ? $"installed ({vinegar.Source})" : "not installed")}");
        Cli.WriteLine($"Master flags: {ctx.MasterFlags.Flags.Count} ({Bytestrap.Core.Config.Paths.MasterFlagsFile})");
        Cli.WriteLine($"Installs:     {installs.Count}");

        if (installs.Count == 0)
        {
            Cli.WriteLine();
            Cli.Warn("No Roblox installs found in any Wine prefix.");
            Cli.WriteLine("To play: install Sober (flatpak install flathub org.vinegarhq.Sober).");
            Cli.WriteLine("To manage flags for a Wine-layout install (e.g. Studio via Vinegar),");
            Cli.WriteLine("point Bytestrap at a prefix:  bytestrap --prefix /path/to/prefix status");
            return 0;
        }

        foreach (var install in installs)
        {
            Cli.WriteLine();
            Cli.WriteLine($"[{install.Launcher}] {install.WinePrefix}");
            Cli.Dim($"  {install.RobloxDir}");
            foreach (var version in install.Versions)
            {
                string kinds = (version.HasPlayer, version.HasStudio) switch
                {
                    (true, true) => "player+studio",
                    (true, false) => "player",
                    (false, true) => "studio",
                    _ => "?",
                };
                Cli.WriteLine($"  {version.Name}  ({kinds})");
            }
        }

        if (string.IsNullOrEmpty(runner))
        {
            Cli.WriteLine();
            Cli.Warn("No Wine/Proton runner found. Set one with:  bytestrap config set runner /path/to/wine");
        }

        return 0;
    }

    public static int Versions(AppContext ctx)
    {
        var installs = ctx.Installs();

        if (installs.Count == 0)
        {
            Cli.Warn("No Roblox installs found.");
            return 1;
        }

        foreach (var install in installs)
        {
            Cli.Head($"[{install.Launcher}] {install.WinePrefix}");
            foreach (var version in install.Versions)
                Cli.WriteLine($"  {version.Name}");
        }

        return 0;
    }

    public static int Apply(AppContext ctx)
    {
        var installs = ctx.Installs();

        if (installs.Count == 0)
        {
            Cli.Warn("No Roblox installs found - flags saved to master copy only.");
            return 1;
        }

        var result = FlagDeployer.ApplyToInstalls(ctx.MasterFlags.Flags, installs);

        Cli.Ok($"Deployed {ctx.MasterFlags.Flags.Count} flags to {result.VersionsUpdated} version dir(s).");

        foreach (string error in result.Errors)
            Cli.Err($"  failed: {error}");

        return result.Errors.Count == 0 ? 0 : 1;
    }
}
