namespace Bytestrap.Core.Config;

/// <summary>XDG-compliant filesystem layout for Bytestrap on Linux.</summary>
public static class Paths
{
    private static string Home =>
        Environment.GetEnvironmentVariable("HOME") ?? "/tmp";

    private static string EnvOr(string name, string fallback) =>
        Environment.GetEnvironmentVariable(name) is string v && !string.IsNullOrWhiteSpace(v)
            ? v
            : fallback;

    public static string ConfigDir => EnvOr("XDG_CONFIG_HOME", Path.Combine(Home, ".config"));
    public static string DataDir => EnvOr("XDG_DATA_HOME", Path.Combine(Home, ".local/share"));
    public static string CacheDir => EnvOr("XDG_CACHE_HOME", Path.Combine(Home, ".cache"));

    /// <summary>~/.config/bytestrap - settings.json lives here.</summary>
    public static string AppConfigDir => Path.Combine(ConfigDir, "bytestrap");

    /// <summary>~/.local/share/bytestrap - master flags, profiles, logs.</summary>
    public static string AppDataDir => Path.Combine(DataDir, "bytestrap");

    public static string SettingsFile => Path.Combine(AppConfigDir, "settings.json");

    /// <summary>Master ClientAppSettings.json, deployed to every Roblox version dir.</summary>
    public static string MasterFlagsFile => Path.Combine(AppDataDir, "ClientAppSettings.json");

    public static string ProfilesDir => Path.Combine(AppDataDir, "profiles");

    public static string LogsDir => Path.Combine(AppDataDir, "logs");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(AppConfigDir);
        Directory.CreateDirectory(AppDataDir);
        Directory.CreateDirectory(ProfilesDir);
        Directory.CreateDirectory(LogsDir);
    }
}
