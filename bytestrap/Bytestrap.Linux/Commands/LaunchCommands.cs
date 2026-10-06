using Bytestrap.Core.Roblox;
using Bytestrap.Core.Util;

namespace Bytestrap.Linux.Commands;

public static class LaunchCommands
{
    public static int Launch(AppContext ctx, ParsedArgs args)
    {
        bool studio = args.Has("studio");
        bool dryRun = args.Has("dry-run");
        string launchUri = args.Positionals.FirstOrDefault() ?? string.Empty;

        var install = ctx.FirstInstall();
        if (install is null)
        {
            Cli.Err("No Roblox installs found. See: bytestrap status");
            return 1;
        }

        var version = studio
            ? install.Versions.FirstOrDefault(v => v.HasStudio)
            : install.Versions.FirstOrDefault(v => v.HasPlayer);

        if (version is null)
        {
            Cli.Err(studio ? "No Studio install found." : "No Player install found.");
            return 1;
        }

        var plan = WineRunner.PlanLaunch(ctx.Settings, install, version, launchUri, studio);
        if (plan is null)
        {
            Cli.Err("No Wine/Proton runner found. Set one with: bytestrap config set runner /path/to/wine");
            return 1;
        }

        if (dryRun)
        {
            Cli.Head("Launch plan (dry run):");
            Cli.WriteLine($"  runner:  {plan.Runner}");
            Cli.WriteLine($"  exe:     {plan.Executable}");
            Cli.WriteLine($"  args:    {plan.Arguments}");
            Cli.WriteLine($"  prefix:  {plan.WinePrefix}");
            Cli.WriteLine($"  gamemode: {(plan.UseGameMode ? "yes" : "no")}");
            foreach (var pair in plan.Environment)
                Cli.WriteLine($"  env:     {pair.Key}={pair.Value}");
            return 0;
        }

        Cli.WriteLine($"Launching {(studio ? "Studio" : "Player")} {version.Name} via {Path.GetFileName(plan.Runner)}...");

        int pid = WineRunner.Launch(plan, ctx.Settings);

        if (pid == 0)
        {
            Cli.Err("Failed to start the runner process.");
            return 1;
        }

        Cli.Ok($"Started (pid {pid}).");
        return 0;
    }

    public static int Kill(AppContext ctx)
    {
        int killed = 0;
        killed += Shell.KillAll("RobloxPlayerBeta");
        killed += Shell.KillAll("RobloxStudioBeta");
        killed += Shell.KillAll("RobloxCrashHandler");

        if (killed == 0)
            Cli.WriteLine("No Roblox processes running.");
        else
            Cli.Ok($"Killed {killed} process(es).");

        return 0;
    }

    public static int Clean(AppContext ctx, ParsedArgs args)
    {
        var installs = ctx.Installs();

        if (installs.Count == 0)
        {
            Cli.Warn("No Roblox installs found.");
            return 1;
        }

        long bytesFreed = 0;
        int filesDeleted = 0;

        foreach (var install in installs)
        {
            // Roblox logs: <Roblox>/logs/*.log
            DeleteDirContents(Path.Combine(install.RobloxDir, "logs"), ref bytesFreed, ref filesDeleted);

            // Downloads cache: <Roblox>/Downloads/* (safe: re-downloaded on demand)
            // Only clean when --deep is passed, since it forces redownloads.
            if (args.Has("deep"))
                DeleteDirContents(Path.Combine(install.RobloxDir, "Downloads"), ref bytesFreed, ref filesDeleted);
        }

        // Bytestrap's own old logs (>7 days).
        try
        {
            foreach (string log in Directory.GetFiles(Bytestrap.Core.Config.Paths.LogsDir))
            {
                try
                {
                    var info = new FileInfo(log);
                    if (info.LastWriteTimeUtc.AddDays(7) < DateTime.UtcNow)
                    {
                        bytesFreed += info.Length;
                        info.Delete();
                        filesDeleted++;
                    }
                }
                catch { }
            }
        }
        catch { }

        Cli.Ok($"Cleaned {filesDeleted} file(s), freed {bytesFreed / 1048576} MB.");
        return 0;
    }

    private static void DeleteDirContents(string dir, ref long bytesFreed, ref int filesDeleted)
    {
        string[] files;
        try
        {
            files = Directory.Exists(dir) ? Directory.GetFiles(dir) : Array.Empty<string>();
        }
        catch
        {
            return;
        }

        foreach (string file in files)
        {
            try
            {
                var info = new FileInfo(file);
                bytesFreed += info.Length;
                info.Delete();
                filesDeleted++;
            }
            catch { }
        }
    }
}
