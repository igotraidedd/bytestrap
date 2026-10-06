using Bytestrap.Core.Util;

namespace Bytestrap.Core.Roblox;

/// <summary>
/// Detects the Flatpak-based Roblox runners. Since Roblox's Hyperion
/// anti-cheat blocks the stock Windows Player under Wine, Sober (Android
/// runtime, for playing) and Vinegar (Wine wrapper, for Studio) are the
/// practical way to run Roblox on Linux - so `status` reports them alongside
/// any Wine-prefix installs.
/// </summary>
public static class FlatpakRunners
{
    public const string SoberAppId = "org.vinegarhq.Sober";
    public const string VinegarAppId = "org.vinegarhq.Vinegar";

    public sealed record RunnerStatus(bool Installed, string Source);

    public static RunnerStatus Sober() => Probe(SoberAppId);
    public static RunnerStatus Vinegar() => Probe(VinegarAppId);

    private static RunnerStatus Probe(string appId)
    {
        string home = Environment.GetEnvironmentVariable("HOME") ?? "/tmp";

        // Fast path: per-user flatpak app data exists.
        string userData = Path.Combine(home, ".var/app", appId);
        try
        {
            if (Directory.Exists(userData))
                return new RunnerStatus(true, "flatpak (user)");
        }
        catch { }

        // System-wide flatpak install.
        string systemData = Path.Combine("/var/lib/flatpak/app", appId);
        try
        {
            if (Directory.Exists(systemData))
                return new RunnerStatus(true, "flatpak (system)");
        }
        catch { }

        // Ask flatpak directly (slower, most accurate).
        if (Shell.Which("flatpak") is not null)
        {
            string? info = Shell.RunAndCapture("flatpak", $"info --show-ref {appId}", 5000);
            if (!string.IsNullOrWhiteSpace(info))
                return new RunnerStatus(true, "flatpak");
        }

        return new RunnerStatus(false, string.Empty);
    }

    public static string InstallHint(string appId) =>
        $"flatpak install flathub {appId}";
}
