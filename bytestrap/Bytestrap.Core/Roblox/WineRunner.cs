using System.Diagnostics;
using Bytestrap.Core.Config;
using Bytestrap.Core.Util;

namespace Bytestrap.Core.Roblox;

/// <summary>
/// Resolves the Wine/Proton runner binary and launches Roblox with a
/// performance-tuned environment (esync/fsync, gamemode, GL cache).
/// </summary>
public static class WineRunner
{
    /// <summary>Resolves which binary to run Roblox with. Empty = none found.</summary>
    public static string ResolveRunner(Settings settings)
    {
        // Explicit setting wins (path or PATH name).
        if (!string.IsNullOrWhiteSpace(settings.Runner))
        {
            string? direct = Shell.Which(settings.Runner.Trim());
            if (direct is not null)
                return direct;

            // Trust it anyway - it may be a wrapper script resolved at exec time.
            return settings.Runner.Trim();
        }

        // Common runner names on PATH.
        foreach (string name in new[] { "wine", "wine64", "proton", "umu-run" })
        {
            string? found = Shell.Which(name);
            if (found is not null)
                return found;
        }

        // Steam/Proton installs live outside PATH - probe well-known spots.
        foreach (string proton in ProbeProtonInstalls())
            return proton;

        return string.Empty;
    }

    private static IEnumerable<string> ProbeProtonInstalls()
    {
        string home = Environment.GetEnvironmentVariable("HOME") ?? "/tmp";

        var roots = new[]
        {
            Path.Combine(home, ".steam/steam/steamapps/common"),
            Path.Combine(home, ".local/share/Steam/steamapps/common"),
            "/usr/share/steam/compatibilitytools.d",
            Path.Combine(home, ".steam/compatibilitytools.d"),
            Path.Combine(home, ".local/share/Steam/compatibilitytools.d"),
        };

        foreach (string root in roots)
        {
            string[] dirs;
            try
            {
                dirs = Directory.Exists(root) ? Directory.GetDirectories(root) : Array.Empty<string>();
            }
            catch
            {
                continue;
            }

            foreach (string dir in dirs.OrderByDescending(d => d, StringComparer.Ordinal))
            {
                string name = Path.GetFileName(dir);
                if (!name.StartsWith("Proton", StringComparison.OrdinalIgnoreCase))
                    continue;

                string protonBin = Path.Combine(dir, "proton");
                if (File.Exists(protonBin))
                    yield return protonBin;
            }
        }
    }

    public sealed record LaunchPlan(
        string Runner,
        string Executable,
        string Arguments,
        string WinePrefix,
        Dictionary<string, string> Environment,
        bool UseGameMode
    );

    /// <summary>
    /// Builds a launch plan without executing it (used by `launch --dry-run`).
    /// </summary>
    public static LaunchPlan? PlanLaunch(
        Settings settings,
        RobloxInstall install,
        RobloxVersion version,
        string launchUri,
        bool studio)
    {
        string runner = ResolveRunner(settings);
        if (string.IsNullOrEmpty(runner))
            return null;

        string exe = studio ? version.StudioExe : version.PlayerExe;
        if (string.IsNullOrEmpty(exe))
            return null;

        var env = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "WINEPREFIX", install.WinePrefix },
        };

        if (settings.EnableEsync)
            env["WINEESYNC"] = "1";
        if (settings.EnableFsync)
            env["WINEFSYNC"] = "1";

        // Sane graphics/cache defaults; user ExtraEnv overrides all of these.
        env["__GL_SHADER_DISK_CACHE"] = "1";
        env["__GL_SHADER_DISK_CACHE_SKIP_CLEANUP"] = "1";
        env["mesa_glthread"] = "true";
        env["DXVK_ASYNC"] = "1";

        foreach (var pair in settings.ExtraEnv)
            env[pair.Key] = pair.Value;

        string args = launchUri.Trim();
        if (!string.IsNullOrWhiteSpace(settings.LaunchArgs))
            args = string.IsNullOrEmpty(args) ? settings.LaunchArgs : $"{args} {settings.LaunchArgs}";

        bool useGameMode = settings.UseGameMode && Shell.Which("gamemoderun") is not null;

        return new LaunchPlan(runner, exe, args, install.WinePrefix, env, useGameMode);
    }

    /// <summary>Executes a launch plan as a detached process. Returns the PID or 0.</summary>
    public static int Launch(LaunchPlan plan, Settings settings)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = plan.UseGameMode ? "gamemoderun" : plan.Runner,
            WorkingDirectory = Path.GetDirectoryName(plan.Executable) ?? plan.WinePrefix,
        };

        // arg order: [runner-args] <exe> [launch-uri/args]
        var parts = new List<string>();
        if (plan.UseGameMode)
            parts.Add(plan.Runner);
        if (!string.IsNullOrWhiteSpace(settings.RunnerArgs))
            parts.Add(settings.RunnerArgs);
        parts.Add($"\"{plan.Executable}\"");
        if (!string.IsNullOrWhiteSpace(plan.Arguments))
            parts.Add(plan.Arguments);

        startInfo.Arguments = string.Join(' ', parts);

        foreach (var pair in plan.Environment)
            startInfo.Environment[pair.Key] = pair.Value;

        var process = Shell.StartDetached(startInfo);
        return process?.Id ?? 0;
    }
}
