using Microsoft.Win32.TaskScheduler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Timers;

namespace mCompWardenService
{
    public partial class CompWardenService : ServiceBase
    {
        private System.Timers.Timer _timer;
        private readonly string systemTaskName = "mCompWardenSystem";  // Actual task name
        private readonly string userTaskName = "mCompWardenUser";    // Actual task name
        private readonly string logFilePath = @"c:\it\compwarden\compwardenservice.txt";
        private readonly string cmdFilePath = @"c:\it\compwarden\compwardenservice-cmd.txt";
        private readonly string taskExe = @"c:\it\compwarden\mCompWarden2.exe";
        private readonly string srcExe = @"\\rentex.intra\SYSVOL\rentex.intra\scripts\mcompwarden2\mCompWarden2.exe";

        [DllImport("kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        public CompWardenService()
        {
            InitializeComponent(); // keep designer-generated method; do NOT re-declare it here
            ServiceName = "CompWardenService";
        }

        protected override void OnStart(string[] args)
        {
            LogEvent("Service started.");
            _timer = new System.Timers.Timer(60000); // 60s
            _timer.Elapsed += TimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        protected override void OnStop()
        {
            LogEvent("Service stopped.");
            _timer?.Stop();
            _timer?.Dispose();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // If a command file exists, process its commands first.
            if (File.Exists(cmdFilePath))
            {
                try
                {
                    string[] commands = File.ReadAllLines(cmdFilePath)
                                             .Select(c => c.Trim().ToLowerInvariant())
                                             .Where(c => !string.IsNullOrWhiteSpace(c))
                                             .ToArray();

                    bool updateCommand = commands.Any(c => c == "update");
                    bool stopCommand = commands.Any(c => c == "stop");

                    if (updateCommand)
                    {
                        PerformUpdate();
                        return; // Exit this tick
                    }
                    else if (stopCommand)
                    {
                        LogEvent("Stop command detected. Stopping tasks and leaving them stopped until command file is removed.");
                        StopTaskAndDisable(systemTaskName);
                        StopTaskAndDisable(userTaskName);
                        return; // Exit this tick
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"Error processing command file: {ex}");
                }
            }

            // Normal operation: ensure tasks are running (run in STA threads for COM interop)
            Thread t1 = new Thread(() => CheckAndRestartTask(systemTaskName));
            t1.SetApartmentState(ApartmentState.STA);
            t1.Start();

            Thread t2 = new Thread(() => CheckAndRestartTask(userTaskName));
            t2.SetApartmentState(ApartmentState.STA);
            t2.Start();
        }

        // ------------------------- Core flows -------------------------

        private void PerformUpdate()
        {
            LogEvent("Update command detected. Disabling tasks, stopping tasks, killing processes, updating executable, then re-enabling and restarting tasks.");

            DisableTask(systemTaskName);
            DisableTask(userTaskName);

            StopTaskAndDisable(systemTaskName);
            StopTaskAndDisable(userTaskName);

            KillAllMCompWardenProcesses(timeoutMs: 20000);

            if (!WaitForFileUnlock(taskExe, timeoutMs: 15000))
                LogEvent("Executable is still locked after waiting; proceeding but replace may fail.");

            bool updated = false;
            try
            {
                SafeReplaceExecutable(srcExe, taskExe);
                updated = true;
                LogEvent("Task executable updated successfully from source via atomic replace.");
            }
            catch (Exception ex)
            {
                LogEvent($"Error updating task executable: {ex}");
            }

            if (updated)
            {
                try
                {
                    File.Delete(cmdFilePath);
                    LogEvent("Command file deleted after update.");
                }
                catch (Exception ex)
                {
                    LogEvent($"Error deleting command file after update: {ex}");
                }

                EnableTask(systemTaskName);
                EnableTask(userTaskName);

                RestartTaskSynchronously(systemTaskName);
                RestartTaskSynchronously(userTaskName);
            }
            else
            {
                LogEvent("Update failed; leaving tasks disabled to avoid running mixed versions. Re-issue UPDATE after investigating the logs.");
            }
        }

        // ------------------------- Task helpers -------------------------

        private void CheckAndRestartTask(string taskName)
        {
            try
            {
                using (TaskService ts = new TaskService())
                {
                    var task = ts.GetTask(taskName);
                    if (task == null) return;

                    if (taskName == userTaskName)
                    {
                        // 1. Zjistíme ID aktivní session (kdo je u klávesnice)
                        uint activeSessionId = WTSGetActiveConsoleSessionId();
                        if (activeSessionId == 0xFFFFFFFF) return; // Nikdo není přihlášen

                        // 2. Najdeme všechny procesy mCompWarden2
                        var processes = Process.GetProcessesByName("mCompWarden2");

                        bool activeUserProcessExists = false;

                        foreach (var p in processes)
                        {
                            // Ignorujeme systémovou instanci v Session 0
                            if (p.SessionId == 0) continue;

                            // Pokud proces běží v JINÉ než aktivní session, ukončíme ho
                            if (p.SessionId != (int)activeSessionId)
                            {
                                LogEvent($"Ukončuji instanci v neaktivní Session {p.SessionId}.");
                                try { p.Kill(); } catch { }
                            }
                            else
                            {
                                activeUserProcessExists = true;
                            }
                        }

                        // 3. Pokud v aktivní session nic neběží, vynutíme start
                        if (!activeUserProcessExists)
                        {
                            LogEvent($"Spouštím {userTaskName} pro aktivní Session {activeSessionId}.");
                            task.Run();
                        }
                    }
                    else // Logika pro mCompWardenSystem
                    {
                        // Systémová úloha běží v Session 0, stačí prostá kontrola běhu
                        if (task.State != TaskState.Running)
                        {
                            LogEvent($"Restartuji systémovou úlohu {taskName}.");
                            task.Run();
                        }
                    }
                }
            }
            catch (Exception ex) { LogEvent($"Watchdog Error: {ex.Message}"); }
        }

        private void RestartTaskSynchronously(string taskName)
        {
            Thread restartThread = new Thread(() =>
            {
                try
                {
                    using (TaskService ts = new TaskService())
                    {
                        var task = ts.GetTask(taskName);
                        if (task == null)
                        {
                            LogEvent($"RestartTaskSynchronously: '{taskName}' does not exist.");
                            return;
                        }

                        if (task.State == TaskState.Running)
                        {
                            LogEvent($"Task '{taskName}' already running; leaving as-is.");
                            return;
                        }

                        LogEvent($"Starting task '{taskName}'.");
                        task.Run();
                    }
                }
                catch (Exception ex)
                {
                    LogEvent($"RestartTaskSynchronously error for '{taskName}': {ex}");
                }
            });
            restartThread.SetApartmentState(ApartmentState.STA);
            restartThread.Start();
            restartThread.Join();
        }

        /// <summary>
        /// Disable the task and try to stop it; loops until State != Running or timeout.
        /// This avoids relying on Task.Instances (not available in older TaskScheduler builds).
        /// </summary>
        private void StopTaskAndDisable(string taskName, int waitMs = 15000)
        {
            try
            {
                using (var ts = new TaskService())
                {
                    var task = ts.GetTask(taskName);
                    if (task == null)
                    {
                        LogEvent($"StopTaskAndDisable: '{taskName}' does not exist.");
                        return;
                    }

                    if (task.Enabled)
                    {
                        task.Enabled = false;
                        task.RegisterChanges();
                        LogEvent($"Task '{taskName}' disabled.");
                    }

                    if (task.State == TaskState.Running)
                    {
                        LogEvent($"Task '{taskName}' running. Stopping it.");
                        try { task.Stop(); } catch (Exception ex) { LogEvent($"Task.Stop() error for '{taskName}': {ex.Message}"); }
                    }

                    var sw = Stopwatch.StartNew();
                    while (sw.ElapsedMilliseconds < waitMs)
                    {
                        var t = ts.GetTask(taskName);
                        if (t == null || t.State != TaskState.Running) break;
                        Thread.Sleep(250);
                    }

                    var t2 = ts.GetTask(taskName);
                    if (t2 != null && t2.State == TaskState.Running)
                        LogEvent($"WARNING: Task '{taskName}' still reports running after timeout.");
                    else
                        LogEvent($"Task '{taskName}' stopped (or not running).");
                }
            }
            catch (Exception ex)
            {
                LogEvent($"StopTaskAndDisable error for '{taskName}': {ex}");
            }
        }

        private void DisableTask(string taskName)
        {
            try
            {
                using (var ts = new TaskService())
                {
                    var task = ts.GetTask(taskName);
                    if (task == null)
                    {
                        LogEvent($"DisableTask: '{taskName}' does not exist.");
                        return;
                    }
                    if (task.Enabled)
                    {
                        task.Enabled = false;
                        task.RegisterChanges();
                        LogEvent($"Task '{taskName}' disabled.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogEvent($"DisableTask error for '{taskName}': {ex}");
            }
        }

        private void EnableTask(string taskName)
        {
            try
            {
                using (var ts = new TaskService())
                {
                    var task = ts.GetTask(taskName);
                    if (task == null)
                    {
                        LogEvent($"EnableTask: '{taskName}' does not exist.");
                        return;
                    }
                    if (!task.Enabled)
                    {
                        task.Enabled = true;
                        task.RegisterChanges();
                        LogEvent($"Task '{taskName}' enabled.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogEvent($"EnableTask error for '{taskName}': {ex}");
            }
        }

        // ------------------------- Process & File helpers -------------------------

        private void KillAllMCompWardenProcesses(int timeoutMs)
        {
            try
            {
                var procs = Process.GetProcessesByName("mCompWarden2");
                if (procs.Length == 0)
                {
                    LogEvent("No mCompWarden2.exe processes found to kill.");
                    return;
                }

                LogEvent($"Found {procs.Length} mCompWarden2.exe process(es). Attempting graceful close, then force kill.");

                foreach (var p in procs)
                {
                    try { p.CloseMainWindow(); } catch { }
                }

                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < 3000)
                {
                    if (Process.GetProcessesByName("mCompWarden2").Length == 0) break;
                    Thread.Sleep(200);
                }

                foreach (var p in Process.GetProcessesByName("mCompWarden2"))
                {
                    try { p.Kill(); } catch { }
                }

                // Final sweep by image name across sessions
                try
                {
                    var psiAll = new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = "/IM mCompWarden2.exe /F /T",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                    using (var tkAll = Process.Start(psiAll)) tkAll?.WaitForExit(10000);
                }
                catch { }

                sw.Restart();
                while (sw.ElapsedMilliseconds < timeoutMs)
                {
                    if (Process.GetProcessesByName("mCompWarden2").Length == 0) break;
                    Thread.Sleep(250);
                }

                var still = Process.GetProcessesByName("mCompWarden2").Length;
                LogEvent(still > 0 ? $"WARNING: {still} mCompWarden2.exe process(es) still present after kill timeout." : "All mCompWarden2.exe processes terminated.");
            }
            catch (Exception ex)
            {
                LogEvent($"KillAllMCompWardenProcesses error: {ex}");
            }
        }

        private bool WaitForFileUnlock(string path, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (!File.Exists(path)) return true;
                FileStream fs = null;
                try
                {
                    fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    return true; // exclusive lock obtained
                }
                catch
                {
                    Thread.Sleep(200);
                }
                finally
                {
                    try { fs?.Dispose(); } catch { }
                }
            }
            return false;
        }

        private void SafeReplaceExecutable(string source, string target)
        {
            var targetDir = Path.GetDirectoryName(target);
            if (string.IsNullOrEmpty(targetDir))
                throw new IOException("Target directory is empty.");

            Directory.CreateDirectory(targetDir);

            var tempPath = Path.Combine(targetDir, $"mCompWarden2.new.{Guid.NewGuid():N}.tmp");
            var backupPath = Path.Combine(targetDir, $"mCompWarden2.bak.{DateTime.Now:yyyyMMddHHmmss}");

            File.Copy(source, tempPath, true);

            try
            {
                if (File.Exists(target))
                {
                    try
                    {
                        File.Replace(tempPath, target, backupPath);
                        try { File.Delete(backupPath); } catch { }
                    }
                    catch
                    {
                        try { File.Delete(target); } catch { }
                        File.Move(tempPath, target);
                    }
                }
                else
                {
                    File.Move(tempPath, target);
                }
            }
            finally
            {
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
            }
        }

        private void LogEvent(string message)
        {
            try
            {
                string directory = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                string logMessage = $"{DateTime.Now:G} - {message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logMessage);
            }
            catch { }
        }
    }
}
