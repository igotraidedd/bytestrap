using Bytestrap.Core.Util;

namespace Bytestrap.Core.Config;

/// <summary>
/// Persistent Linux settings (~/.config/bytestrap/settings.json).
/// Runner/prefix can also be overridden per-invocation via CLI flags or env.
/// </summary>
public sealed class Settings
{
    /// <summary>Explicit Wine prefix. Empty = auto-detect (plus $WINEPREFIX).</summary>
    public string WinePrefix { get; set; } = string.Empty;

    /// <summary>Extra Wine prefixes to also deploy flags into.</summary>
    public List<string> ExtraWinePrefixes { get; set; } = new();

    /// <summary>
    /// Runner binary: "wine", "wine64", "proton", an absolute path, ...
    /// Empty = auto-detect (wine in PATH, else common Proton locations).
    /// </summary>
    public string Runner { get; set; } = string.Empty;

    /// <summary>Extra args prepended before the Roblox executable.</summary>
    public string RunnerArgs { get; set; } = string.Empty;

    /// <summary>Extra args appended to the Roblox launch URI/command line.</summary>
    public string LaunchArgs { get; set; } = string.Empty;

    /// <summary>Prefix the launch with `gamemoderun` when available.</summary>
    public bool UseGameMode { get; set; } = true;

    /// <summary>Set WINEESYNC=1 (requires an esync-capable kernel/wine).</summary>
    public bool EnableEsync { get; set; } = true;

    /// <summary>Set WINEFSYNC=1 (requires an fsync-capable kernel/wine).</summary>
    public bool EnableFsync { get; set; } = true;

    /// <summary>Extra environment variables applied at launch.</summary>
    public Dictionary<string, string> ExtraEnv { get; set; } = new(StringComparer.Ordinal);

    public static Settings Load()
    {
        try
        {
            if (File.Exists(Paths.SettingsFile))
            {
                var loaded = Json.Options.Read<Settings>(File.ReadAllText(Paths.SettingsFile));
                if (loaded is not null)
                    return loaded;
            }
        }
        catch { }

        return new Settings();
    }

    public void Save()
    {
        try
        {
            Paths.EnsureCreated();
            File.WriteAllText(Paths.SettingsFile, Json.Options.WriteIndented(this));
        }
        catch { }
    }
}
