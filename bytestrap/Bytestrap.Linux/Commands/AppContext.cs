using Bytestrap.Core.Config;
using Bytestrap.Core.Flags;
using Bytestrap.Core.Roblox;

namespace Bytestrap.Linux.Commands;

/// <summary>Shared per-invocation state: settings, master flags, detected installs.</summary>
public sealed class AppContext
{
    public Settings Settings { get; }
    public ClientSettingsFile MasterFlags { get; }
    public bool JsonOutput { get; }

    private List<RobloxInstall>? _installs;

    public AppContext(Settings settings, ClientSettingsFile masterFlags, bool jsonOutput)
    {
        Settings = settings;
        MasterFlags = masterFlags;
        JsonOutput = jsonOutput;
    }

    public static AppContext Create(ParsedArgs args)
    {
        Paths.EnsureCreated();
        var settings = Settings.Load();

        // Per-invocation overrides (env + flags beat the settings file).
        string? prefixOpt = args.Get("prefix", "wineprefix");
        if (!string.IsNullOrWhiteSpace(prefixOpt))
            settings.WinePrefix = prefixOpt;

        string? runnerOpt = args.Get("runner");
        if (!string.IsNullOrWhiteSpace(runnerOpt))
            settings.Runner = runnerOpt;

        var master = new ClientSettingsFile(Paths.MasterFlagsFile);
        master.Load();

        return new AppContext(settings, master, args.Has("json"));
    }

    public List<RobloxInstall> Installs() =>
        _installs ??= RobloxInstallDetector.Detect(Settings.WinePrefix, Settings.ExtraWinePrefixes);

    public RobloxInstall? FirstInstall() => Installs().FirstOrDefault();

    public void SaveMaster() => MasterFlags.Save();

    public void SaveSettings() => Settings.Save();
}
