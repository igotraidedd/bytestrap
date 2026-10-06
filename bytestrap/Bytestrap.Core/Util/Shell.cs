using System.Diagnostics;

namespace Bytestrap.Core.Util;

/// <summary>Small process helpers (PATH lookup, process listing, detached launch).</summary>
public static class Shell
{
    /// <summary>Finds an executable on PATH, like `which`. Returns null if missing.</summary>
    public static string? Which(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        // Absolute/relative path given directly.
        if (name.Contains(Path.DirectorySeparatorChar))
            return File.Exists(name) ? Path.GetFullPath(name) : null;

        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
            return null;

        foreach (string dir in pathEnv.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(dir))
                continue;

            try
            {
                string candidate = Path.Combine(dir, name);
                if (File.Exists(candidate))
                    return candidate;
            }
            catch { }
        }

        return null;
    }

    /// <summary>Runs a process to completion and captures stdout. Returns null on failure.</summary>
    public static string? RunAndCapture(string fileName, string arguments, int timeoutMs = 10_000)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (!process.Start())
                return null;

            if (!process.WaitForExit(timeoutMs))
            {
                try { process.Kill(entireProcessTree: true); } catch { }
                return null;
            }

            return process.ExitCode == 0 ? process.StandardOutput.ReadToEnd().Trim() : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Starts a detached process that outlives us (for launching Roblox).</summary>
    public static Process? StartDetached(ProcessStartInfo startInfo)
    {
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = false;
        return Process.Start(startInfo);
    }

    /// <summary>Best-effort check for whether a process with this name is running.</summary>
    public static bool IsProcessRunning(string processName)
    {
        try
        {
            return Process.GetProcessesByName(processName).Length > 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>Kills all processes with the given name. Returns number killed.</summary>
    public static int KillAll(string processName)
    {
        int killed = 0;

        try
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                    killed++;
                }
                catch { }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch { }

        return killed;
    }
}
