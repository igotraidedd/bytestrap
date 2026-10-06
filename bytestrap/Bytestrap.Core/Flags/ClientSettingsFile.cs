using Bytestrap.Core.Util;

namespace Bytestrap.Core.Flags;

/// <summary>
/// Reads/writes Roblox ClientAppSettings.json files.
/// All values are stored as strings, matching the Windows app's behavior.
/// Writes are atomic (temp file + rename) so a crash can't corrupt flags.
/// </summary>
public sealed class ClientSettingsFile
{
    private readonly object _lock = new();

    public string FilePath { get; }

    public Dictionary<string, string> Flags { get; private set; } =
        new(StringComparer.Ordinal);

    public ClientSettingsFile(string filePath)
    {
        FilePath = filePath;
    }

    public void Load()
    {
        lock (_lock)
        {
            if (!File.Exists(FilePath))
            {
                Flags = new Dictionary<string, string>(StringComparer.Ordinal);
                return;
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                var parsed = Json.Options.Read<Dictionary<string, object?>>(json);

                var flags = new Dictionary<string, string>(StringComparer.Ordinal);
                if (parsed is not null)
                {
                    foreach (var pair in parsed)
                        flags[pair.Key] = pair.Value?.ToString() ?? string.Empty;
                }

                Flags = flags;
            }
            catch
            {
                // Corrupt file: back it up and start clean rather than crash.
                try
                {
                    File.Copy(FilePath, FilePath + ".bak", overwrite: true);
                }
                catch { }

                Flags = new Dictionary<string, string>(StringComparer.Ordinal);
            }
        }
    }

    public void Save()
    {
        lock (_lock)
        {
            string? dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            string json = Json.Options.WriteIndented(Flags);

            string tmpPath = FilePath + ".tmp";
            File.WriteAllText(tmpPath, json);

            // Atomic replace (POSIX rename). Falls back to copy on exotic FSes.
            try
            {
                File.Move(tmpPath, FilePath, overwrite: true);
            }
            catch
            {
                File.Copy(tmpPath, FilePath, overwrite: true);
                try { File.Delete(tmpPath); } catch { }
            }
        }
    }

    public string? Get(string presetOrFlag)
    {
        string flag = FlagPresets.ResolveFlagName(presetOrFlag);

        lock (_lock)
            return Flags.TryGetValue(flag, out string? value) ? value : null;
    }

    public void Set(string presetOrFlag, string value)
    {
        string flag = FlagPresets.ResolveFlagName(presetOrFlag);

        lock (_lock)
            Flags[flag] = value;
    }

    /// <returns>True if a flag was actually removed.</returns>
    public bool Unset(string presetOrFlag)
    {
        string flag = FlagPresets.ResolveFlagName(presetOrFlag);

        lock (_lock)
            return Flags.Remove(flag);
    }

    /// <summary>Merges raw FFlag pairs in (profile render output, imports).</summary>
    public void Merge(IEnumerable<KeyValuePair<string, string>> pairs)
    {
        lock (_lock)
        {
            foreach (var pair in pairs)
                Flags[pair.Key] = pair.Value;
        }
    }

    /// <returns>Number of flags removed.</returns>
    public int ClearByPrefix(string prefix)
    {
        lock (_lock)
        {
            var keys = Flags.Keys
                .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
                .ToArray();

            foreach (string key in keys)
                Flags.Remove(key);

            return keys.Length;
        }
    }
}
