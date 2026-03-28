using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;

namespace MCompWardenUpdater
{
    internal static class Program
    {
        // --- Defaults (edit if needed) ---
        private const string SystemTaskName = "mCompWardenSystem";
        private const string UserTaskName = "mCompWardenUser";

        private static string SourceExe = @"\\rentex.intra\SYSVOL\rentex.intra\scripts\mcompwarden2\mCompWarden2.exe";
        private static string TargetExe = @"C:\it\compwarden\mCompWarden2.exe";
        private static string LogPath = @"C:\it\compwarden\updater\updater-log.txt";

        private const string ImageName = "mCompWarden2"; // without .exe
        private const int KillTimeoutMs = 15000;         // wait for processes to disappear
        private const int SchtasksTimeoutMs = 15000;

        // Exit codes:
        // 0 = success (updated or no update needed)
        // 10 = source not found (skipped)
        // 11 = already up-to-date (skipped)
        // 1 = fatal error
        private static int Main(string[] args)
        {
            ParseArgs(args);

            try
            {
                EnsureElevated();

                Directory.CreateDirectory(Path.GetDirectoryName(LogPath) ?? ".");
                Log("=== MCompWarden Updater started ===");
                Log($"SRC: {SourceExe}");
                Log($"DST: {TargetExe}");

                // Decide first — only proceed if there's something to do
                if (!File.Exists(SourceExe))
                {
                    Log("Source exe not found; skipping update.");
                    return 10;
                }

                if (IsUpToDate(SourceExe, TargetExe, out var reason))
                {
                    Log($"No update needed ({reason}).");
                    return 11;
                }

                Log("Update needed. Proceeding to stop tasks and replace binary.");

                // 1) Stop tasks (best effort)
                StopTask(SystemTaskName);
                StopTask(UserTaskName);

                // 2) Kill all running processes
                KillAllMCompWardenProcesses(KillTimeoutMs);

                // 3) Swap executable safely
                SafeReplaceExecutable(SourceExe, TargetExe);

                // 4) Restart tasks
                StartTask(SystemTaskName);
                StartTask(UserTaskName);

                Log("=== Update finished OK ===");
                return 0;
            }
            catch (Exception ex)
            {
                Log("FATAL: " + ex);
                return 1;
            }
        }

        // ---------------- Decision helpers ----------------

        private static bool IsUpToDate(string src, string dst, out string reason)
        {
            reason = "";
            // If target missing → update needed
            if (!File.Exists(dst))
            {
                reason = "target missing";
                return false;
            }

            // Quick check: size & last write time; if different, likely update needed.
            var sfi = new FileInfo(src);
            var dfi = new FileInfo(dst);
            if (sfi.Length != dfi.Length)
            {
                reason = $"length differs (src {sfi.Length}, dst {dfi.Length})";
                return false;
            }

            // Hash compare (robust)
            var srcHash = ComputeSha256(src);
            var dstHash = ComputeSha256(dst);

            if (!srcHash.SequenceEqual(dstHash))
            {
                reason = "SHA-256 differs";
                return false;
            }

            reason = "SHA-256 equal";
            return true;
        }

        private static byte[] ComputeSha256(string path)
        {
            using (var sha = SHA256.Create())
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return sha.ComputeHash(fs);
            }
        }

        // ---------------- Core steps ----------------

        private static void StopTask(string taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName)) return;
            Log($"Stopping task '{taskName}'...");
            RunTool("schtasks", $"/End /TN \"{taskName}\"", SchtasksTimeoutMs, ignoreNonZeroExit: true);
        }

        private static void StartTask(string taskName)
        {
            if (string.IsNullOrWhiteSpace(taskName)) return;
            Log($"Starting task '{taskName}'...");
            RunTool("schtasks", $"/Run /TN \"{taskName}\"", SchtasksTimeoutMs, ignoreNonZeroExit: false);
        }

        private static void KillAllMCompWardenProcesses(int timeoutMs)
        {
            try
            {
                var procs = Process.GetProcessesByName(ImageName);
                if (procs.Length == 0)
                {
                    Log("No mCompWarden2.exe processes found.");
                    return;
                }

                Log($"Found {procs.Length} {ImageName}.exe process(es). Attempting graceful close...");
                foreach (var p in procs)
                    try { p.CloseMainWindow(); } catch { }

                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 2000)
                {
                    if (Process.GetProcessesByName(ImageName).Length == 0) break;
                    Thread.Sleep(200);
                }

                foreach (var p in Process.GetProcessesByName(ImageName))
                {
                    var pid = TryGetPid(p);
                    if (pid <= 0) continue;
                    Log($"Force-killing PID {pid} (and tree) via taskkill...");
                    var ec = RunTool("taskkill", $"/PID {pid} /T /F", 10000, ignoreNonZeroExit: true);
                    if (ec != 0)
                    {
                        try { p.Kill(); Log($"Process.Kill fallback issued for PID {pid}"); }
                        catch (Exception ex) { Log($"Kill fallback failed for PID {pid}: {ex.Message}"); }
                    }
                }

                sw.Restart();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (Process.GetProcessesByName(ImageName).Length == 0) break;
                    Thread.Sleep(250);
                }

                var still = Process.GetProcessesByName(ImageName).Length;
                Log(still > 0
                    ? $"WARNING: {still} {ImageName}.exe process(es) still present after kill timeout."
                    : "All mCompWarden2.exe processes terminated.");
            }
            catch (Exception ex)
            {
                Log("KillAllMCompWardenProcesses error: " + ex);
            }
        }

        private static void SafeReplaceExecutable(string source, string target)
        {
            // precondition: source exists and differs (checked earlier)
            var targetDir = Path.GetDirectoryName(target);
            if (string.IsNullOrEmpty(targetDir))
                throw new IOException("Target directory is empty.");
            Directory.CreateDirectory(targetDir);

            var tempPath = Path.Combine(targetDir, $"mCompWarden2.new.{Guid.NewGuid():N}.tmp");
            var backupPath = Path.Combine(targetDir, $"mCompWarden2.bak.{DateTime.Now:yyyyMMddHHmmss}");

            Log("Copying fresh payload to temp...");
            File.Copy(source, tempPath, true);

            // Try to ensure destination isn't locked (best effort small retries)
            WaitUntilUnlocked(target, 4000);

            try
            {
                if (File.Exists(target))
                {
                    try
                    {
                        Log("Swapping into place with File.Replace (atomic)...");
                        File.Replace(tempPath, target, backupPath); // creates backup
                        TryDelete(backupPath);
                        Log("File.Replace completed.");
                    }
                    catch (Exception ex)
                    {
                        Log($"File.Replace failed ({ex.Message}), falling back to delete+move...");
                        TryDelete(target);
                        File.Move(tempPath, target);
                    }
                }
                else
                {
                    Log("Target not present; moving new payload into place...");
                    File.Move(tempPath, target);
                }
            }
            finally
            {
                TryDelete(tempPath);
            }

            var fi = new FileInfo(target);
            Log($"New target in place: {fi.FullName} ({fi.Length} bytes, {fi.LastWriteTime})");
        }

        // ---------------- Helpers ----------------

        private static void WaitUntilUnlocked(string path, int timeoutMs)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                        return; // unlocked
                }
                catch
                {
                    Thread.Sleep(150);
                }
            }
        }

        private static void TryDelete(string path)
        {
            try { if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path); } catch { }
        }

        private static int TryGetPid(Process p)
        {
            try { return p.Id; } catch { return -1; }
        }

        private static int RunTool(string fileName, string arguments, int timeoutMs, bool ignoreNonZeroExit)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            using (var p = Process.Start(psi))
            {
                if (p == null)
                {
                    Log($"ERROR: failed to start {fileName} {arguments}");
                    return -1;
                }
                if (!p.WaitForExit(timeoutMs))
                {
                    try { p.Kill(); } catch { }
                    Log($"ERROR: {fileName} timed out after {timeoutMs} ms");
                    return -2;
                }

                var output = p.StandardOutput.ReadToEnd();
                var err = p.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(output)) Log($"[{fileName}] {output.Trim()}");
                if (!string.IsNullOrWhiteSpace(err)) Log($"[{fileName} ERR] {err.Trim()}");

                if (p.ExitCode != 0 && !ignoreNonZeroExit)
                    Log($"{fileName} exited with code {p.ExitCode}");

                return p.ExitCode;
            }
        }

        private static void ParseArgs(string[] args)
        {
            foreach (var a in args ?? Array.Empty<string>())
            {
                var kv = a.Split(new[] { '=' }, 2);
                var key = kv[0].Trim().TrimStart('-', '/').ToLowerInvariant();
                var val = kv.Length > 1 ? TrimQuotes(kv[1].Trim()) : "";

                switch (key)
                {
                    case "src":
                    case "source":
                        if (!string.IsNullOrWhiteSpace(val)) SourceExe = val;
                        break;
                    case "dst":
                    case "target":
                        if (!string.IsNullOrWhiteSpace(val)) TargetExe = val;
                        break;
                    case "log":
                    case "logpath":
                        if (!string.IsNullOrWhiteSpace(val)) LogPath = val;
                        break;
                }
            }
        }

        private static string TrimQuotes(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s.Length >= 2)
            {
                char first = s[0];
                char last = s[s.Length - 1];
                if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                    return s.Substring(1, s.Length - 2);
            }
            return s;
        }

        private static void EnsureElevated()
        {
            using (WindowsIdentity id = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(id);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    throw new InvalidOperationException("This updater must be run as Administrator.");
            }
        }

        private static void Log(string msg)
        {
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {msg}";
            Console.WriteLine(line);
            try
            {
                var dir = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
            catch { }
        }
    }
}
