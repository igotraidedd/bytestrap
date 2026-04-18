using System.Management;
using System.Runtime.InteropServices;

namespace Bloxstrap.Utility
{
    public static class HardwareInfo
    {
        public static string GetCPUName()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                    return obj["Name"]?.ToString()?.Trim() ?? "Unknown";
            }
            catch { }
            return "Unknown";
        }

        public static string GetGPUName()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                foreach (var obj in searcher.Get())
                {
                    string? name = obj["Name"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(name))
                        return name.Trim();
                }
            }
            catch { }
            return "Unknown";
        }

        public static string GetTotalRAM()
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

        public static string GetWindowsVersion()
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

        public static string GetGPUDriverVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT DriverVersion FROM Win32_VideoController");
                foreach (var obj in searcher.Get())
                {
                    string? ver = obj["DriverVersion"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(ver))
                        return ver.Trim();
                }
            }
            catch { }
            return "Unknown";
        }
    }
}
