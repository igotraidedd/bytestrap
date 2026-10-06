using System.Management;
using System.Runtime.InteropServices;

namespace Bloxstrap.Utility
{
    /// <summary>
    /// Hardware/OS information helpers.
    ///
    /// WMI queries are expensive (each one can take 100ms+ on first use as the
    /// WMI subsystem spins up), and these values effectively never change during
    /// a process lifetime - so every query is executed at most once and cached.
    /// All members are thread-safe and may be called from any thread.
    /// </summary>
    public static class HardwareInfo
    {
        // Lazy<T> with ExecutionAndPublication (default) = thread-safe, executes once.
        private static readonly Lazy<string> _cpuName = new(QueryCPUName, LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<string> _gpuName = new(QueryGPUName, LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<string> _gpuDriver = new(QueryGPUDriverVersion, LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<string> _totalRAM = new(QueryTotalRAM, LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<string> _osVersion = new(QueryOSVersion, LazyThreadSafetyMode.ExecutionAndPublication);

        public static string GetCPUName() => _cpuName.Value;

        public static string GetGPUName() => _gpuName.Value;

        public static string GetGPUDriverVersion() => _gpuDriver.Value;

        public static string GetTotalRAM() => _totalRAM.Value;

        public static string GetWindowsVersion() => _osVersion.Value;

        // Uptime intentionally stays uncached - it changes constantly and is cheap to compute.
        public static string GetUptime()
        {
            try
            {
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                if (uptime.Days > 0)
                    return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
                return $"{uptime.Hours}h {uptime.Minutes}m";
            }
            catch { }
            return "Unknown";
        }

        // Disk space stays uncached - it changes as Roblox downloads/updates.
        public static string GetDiskSpace()
        {
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:");
                double freeGB = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                double totalGB = drive.TotalSize / (1024.0 * 1024 * 1024);
                return $"{freeGB:F1} / {totalGB:F1} GB free";
            }
            catch { }
            return "Unknown";
        }

        /// <summary>
        /// Warms the WMI cache on a background thread so the first UI page that
        /// shows hardware info doesn't stall. Fire-and-forget, safe to call anywhere.
        /// </summary>
        public static void WarmCacheInBackground()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    _ = _cpuName.Value;
                    _ = _gpuName.Value;
                    _ = _gpuDriver.Value;
                    _ = _totalRAM.Value;
                    _ = _osVersion.Value;
                }
                catch { }
            });
        }

        private static string QuerySingleWmiValue(string query, string property)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(query);
                foreach (var obj in searcher.Get())
                {
                    string? value = obj[property]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value.Trim();
                }
            }
            catch { }
            return "Unknown";
        }

        private static string QueryCPUName() =>
            QuerySingleWmiValue("SELECT Name FROM Win32_Processor", "Name");

        private static string QueryGPUName() =>
            QuerySingleWmiValue("SELECT Name FROM Win32_VideoController", "Name");

        private static string QueryGPUDriverVersion() =>
            QuerySingleWmiValue("SELECT DriverVersion FROM Win32_VideoController", "DriverVersion");

        private static string QueryTotalRAM()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (var obj in searcher.Get())
                {
                    if (ulong.TryParse(obj["TotalPhysicalMemory"]?.ToString(), out ulong bytes))
                        return $"{bytes / (1024 * 1024 * 1024.0):F1} GB";
                }
            }
            catch { }
            return "Unknown";
        }

        private static string QueryOSVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get())
                    return $"{obj["Caption"]} ({obj["Version"]})";
            }
            catch { }
            return RuntimeInformation.OSDescription;
        }
    }
}
