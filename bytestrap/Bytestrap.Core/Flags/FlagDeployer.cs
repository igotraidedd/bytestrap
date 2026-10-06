using Bytestrap.Core.Roblox;

namespace Bytestrap.Core.Flags;

/// <summary>
/// Deploys the master flag set into every detected Roblox version directory
/// (the Linux equivalent of the Windows app's bypass flag method).
/// </summary>
public static class FlagDeployer
{
    public sealed record DeployResult(int VersionsUpdated, List<string> Errors);

    public static DeployResult ApplyToInstalls(
        IReadOnlyDictionary<string, string> masterFlags,
        IEnumerable<RobloxInstall> installs)
    {
        var errors = new List<string>();
        int updated = 0;
        object sync = new();

        var versionDirs = installs
            .SelectMany(i => i.Versions)
            .Select(v => v.Directory)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Parallel.ForEach(versionDirs, versionDir =>
        {
            try
            {
                string clientSettingsDir = Path.Combine(versionDir, "ClientSettings");
                var file = new ClientSettingsFile(Path.Combine(clientSettingsDir, "ClientAppSettings.json"));

                // Preserve unrelated flags already in the version dir (e.g. set by
                // another tool), master wins on conflicts.
                file.Load();
                file.Merge(masterFlags);
                file.Save();

                lock (sync)
                    updated++;
            }
            catch (Exception ex)
            {
                lock (sync)
                    errors.Add($"{versionDir}: {ex.Message}");
            }
        });

        return new DeployResult(updated, errors);
    }
}
