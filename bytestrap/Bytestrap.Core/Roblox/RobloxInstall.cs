namespace Bytestrap.Core.Roblox;

/// <summary>
/// A Roblox install found inside a Wine prefix:
/// &lt;prefix&gt;/drive_c/users/&lt;user&gt;/AppData/Local/Roblox/Versions/...
/// </summary>
public sealed record RobloxInstall(
    string WinePrefix,
    string RobloxDir,
    string Launcher,      // "wine" | "vinegar" | "bottles" | "sober" | "proton" | "custom"
    IReadOnlyList<RobloxVersion> Versions
);

/// <summary>One version-* directory containing a Roblox executable.</summary>
public sealed record RobloxVersion(
    string Name,
    string Directory,
    string PlayerExe,     // "" when absent
    string StudioExe      // "" when absent
)
{
    public bool HasPlayer => !string.IsNullOrEmpty(PlayerExe);
    public bool HasStudio => !string.IsNullOrEmpty(StudioExe);
}

/// <summary>
/// Finds Roblox installs across Wine prefixes (plain Wine, Vinegar, Bottles,
/// Sober, Proton). Detection is pattern-based (drive_c/users/*/AppData/Local/
/// Roblox/Versions/version-*) so it survives launcher layout changes.
/// </summary>
public static class RobloxInstallDetector
{
    public const string PlayerExeName = "RobloxPlayerBeta.exe";
    public const string StudioExeName = "RobloxStudioBeta.exe";

    /// <summary>
    /// Candidate prefix roots, in priority order, deduplicated.
    /// Explicit config and $WINEPREFIX always win.
    /// </summary>
    public static IEnumerable<(string Prefix, string Launcher)> CandidatePrefixes(
        string? configuredPrefix = null,
        IEnumerable<string>? extraPrefixes = null)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (prefix, launcher) in EnumerateCandidates(configuredPrefix, extraPrefixes))
        {
            if (string.IsNullOrWhiteSpace(prefix))
                continue;

            string full;
            try
            {
                full = Path.GetFullPath(prefix);
            }
            catch
            {
                continue;
            }

            if (seen.Add(full))
                yield return (full, launcher);
        }
    }

    private static IEnumerable<(string Prefix, string Launcher)> EnumerateCandidates(
        string? configuredPrefix,
        IEnumerable<string>? extraPrefixes)
    {
        string home = Environment.GetEnvironmentVariable("HOME") ?? "/tmp";

        // 1. Explicit config / env always win.
        if (!string.IsNullOrWhiteSpace(configuredPrefix))
            yield return (configuredPrefix, "custom");

        string? winePrefixEnv = Environment.GetEnvironmentVariable("WINEPREFIX");
        if (!string.IsNullOrWhiteSpace(winePrefixEnv))
            yield return (winePrefixEnv, "custom");

        if (extraPrefixes is not null)
        {
            foreach (string extra in extraPrefixes)
            {
                if (!string.IsNullOrWhiteSpace(extra))
                    yield return (extra, "custom");
            }
        }

        // 2. Stock Wine.
        yield return (Path.Combine(home, ".wine"), "wine");

        // 3. Vinegar (native + flatpak layouts).
        foreach (string dir in SafeDirs(Path.Combine(home, ".local/share/vinegar/prefixes")))
            yield return (dir, "vinegar");
        foreach (string dir in SafeDirs(Path.Combine(home, ".var/app/org.vinegarhq.Vinegar/data/vinegar/prefixes")))
            yield return (dir, "vinegar");

        // 4. Bottles: ~/.local/share/bottles/bottles/<name> (+ flatpak).
        foreach (string bottles in new[]
        {
            Path.Combine(home, ".local/share/bottles/bottles"),
            Path.Combine(home, ".var/app/com.usebottles.bottles/data/bottles/bottles"),
        })
        {
            foreach (string dir in SafeDirs(bottles))
                yield return (dir, "bottles");
        }

        // 5. Sober (flatpak): prefix lives under its app data.
        foreach (string dir in SafeDirs(Path.Combine(home, ".var/app/org.vinegarhq.Sober/data/sober")))
            yield return (dir, "sober");
        foreach (string dir in SafeDirs(Path.Combine(home, ".var/app/org.vinegarhq.Sober/data")))
            yield return (dir, "sober");

        // 6. Proton (Steam compatdata): .../compatdata/<appid>/pfx
        foreach (string steam in new[]
        {
            Path.Combine(home, ".steam/steam/steamapps/compatdata"),
            Path.Combine(home, ".local/share/Steam/steamapps/compatdata"),
        })
        {
            foreach (string appDir in SafeDirs(steam))
                yield return (Path.Combine(appDir, "pfx"), "proton");
        }
    }

    private static IEnumerable<string> SafeDirs(string path)
    {
        string[] dirs;
        try
        {
            dirs = Directory.Exists(path) ? Directory.GetDirectories(path) : Array.Empty<string>();
        }
        catch
        {
            yield break;
        }

        foreach (string dir in dirs)
            yield return dir;
    }

    /// <summary>Scans all candidate prefixes and returns installs that have versions.</summary>
    public static List<RobloxInstall> Detect(
        string? configuredPrefix = null,
        IEnumerable<string>? extraPrefixes = null)
    {
        var installs = new List<RobloxInstall>();

        foreach (var (prefix, launcher) in CandidatePrefixes(configuredPrefix, extraPrefixes))
        {
            foreach (string robloxDir in FindRobloxDirs(prefix))
            {
                var versions = EnumerateVersions(robloxDir);
                if (versions.Count > 0)
                    installs.Add(new RobloxInstall(prefix, robloxDir, launcher, versions));
            }
        }

        return installs;
    }

    /// <summary>drive_c/users/*/AppData/Local/Roblox under a prefix.</summary>
    public static IEnumerable<string> FindRobloxDirs(string winePrefix)
    {
        string usersDir = Path.Combine(winePrefix, "drive_c/users");

        string[] users;
        try
        {
            users = Directory.Exists(usersDir) ? Directory.GetDirectories(usersDir) : Array.Empty<string>();
        }
        catch
        {
            yield break;
        }

        foreach (string userDir in users)
        {
            string robloxDir = Path.Combine(userDir, "AppData/Local/Roblox");
            string versionsDir = Path.Combine(robloxDir, "Versions");

            bool exists;
            try
            {
                exists = Directory.Exists(versionsDir);
            }
            catch
            {
                continue;
            }

            if (exists)
                yield return robloxDir;
        }
    }

    public static List<RobloxVersion> EnumerateVersions(string robloxDir)
    {
        var versions = new List<RobloxVersion>();
        string versionsDir = Path.Combine(robloxDir, "Versions");

        string[] dirs;
        try
        {
            dirs = Directory.Exists(versionsDir) ? Directory.GetDirectories(versionsDir) : Array.Empty<string>();
        }
        catch
        {
            return versions;
        }

        foreach (string dir in dirs.OrderByDescending(d => d, StringComparer.Ordinal))
        {
            string player = Path.Combine(dir, PlayerExeName);
            string studio = Path.Combine(dir, StudioExeName);

            bool hasPlayer, hasStudio;
            try
            {
                hasPlayer = File.Exists(player);
                hasStudio = File.Exists(studio);
            }
            catch
            {
                continue;
            }

            if (!hasPlayer && !hasStudio)
                continue;

            versions.Add(new RobloxVersion(
                Path.GetFileName(dir),
                dir,
                hasPlayer ? player : string.Empty,
                hasStudio ? studio : string.Empty));
        }

        return versions;
    }

    /// <summary>Newest (first) version dir - EnumerateVersions sorts descending.</summary>
    public static RobloxVersion? Latest(RobloxInstall install) =>
        install.Versions.Count > 0 ? install.Versions[0] : null;
}
