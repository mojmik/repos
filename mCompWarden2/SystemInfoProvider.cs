using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;

namespace mCompWarden2
{
    /// <summary>
    /// Provides system and hardware information metrics for admin monitoring and remote reporting.
    /// Supports built-in tokens, parameterized tokens (e.g. {disk:D}, {env:VAR}),
    /// custom tokens loaded from settings or XML, and custom command evaluation.
    /// </summary>
    public static class SystemInfoProvider
    {
        // ---------------- P/Invoke for instant, reliable RAM info ----------------
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32.dll")]
        private static extern ulong GetTickCount64();

        // ---------------- Token Registries ----------------
        private static readonly Dictionary<string, Func<string>> _builtinProviders =
            new Dictionary<string, Func<string>>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> _customTokens =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, string> _customCommands =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly object _syncLock = new object();

        static SystemInfoProvider()
        {
            RegisterBuiltins();
            TryLoadDefaultTokenFiles();
        }

        private static void RegisterBuiltins()
        {
            // RAM
            _builtinProviders["ram"] = GetTotalRam;
            _builtinProviders["ram_total"] = GetTotalRam;
            _builtinProviders["ram_free"] = GetFreeRam;
            _builtinProviders["ram_used"] = GetUsedRam;
            _builtinProviders["ram_summary"] = GetRamSummary;

            // Disk space
            _builtinProviders["free_c"] = () => GetDiskSpace("C");
            _builtinProviders["disk_c"] = () => GetDiskSpace("C");
            _builtinProviders["disk_c_free"] = () => GetDiskFreeGb("C");
            _builtinProviders["disk_c_total"] = () => GetDiskTotalGb("C");
            _builtinProviders["disks"] = GetAllDisksSummary;

            // Printers
            _builtinProviders["printers"] = GetPrinters;
            _builtinProviders["mapped_printers"] = GetPrinters;
            _builtinProviders["default_printer"] = GetDefaultPrinter;

            // Hardware & OS
            _builtinProviders["cpu"] = GetCpuInfo;
            _builtinProviders["os"] = GetOsInfo;
            _builtinProviders["os_version"] = GetOsInfo;
            _builtinProviders["uptime"] = GetUptime;
            _builtinProviders["boot_time"] = GetBootTime;
            _builtinProviders["battery"] = GetBatteryStatus;

            // Admin & Identity
            _builtinProviders["reboot_pending"] = GetPendingReboot;
            _builtinProviders["top_processes"] = () => GetTopProcesses(5);
            _builtinProviders["summary"] = GetAdminSummary;
            _builtinProviders["sysinfo"] = GetAdminSummary;
            _builtinProviders["admin_info"] = GetAdminSummary;

            // Networking & Environment (backward compatibility with mCompWarden)
            _builtinProviders["ip"] = () => NetworkTools.GetIPs();
            _builtinProviders["ver"] = () => Program.GetVer();
            _builtinProviders["user"] = () => Environment.UserName;
            _builtinProviders["users"] = Logger.GetInteractiveUserNames;
            _builtinProviders["computer"] = () => Environment.MachineName;
            _builtinProviders["hostname"] = () => Environment.MachineName;
            _builtinProviders["domain"] = () => Environment.UserDomainName;
        }

        // ---------------- Helpers ----------------
        private static string FormatGb(double gb)
        {
            return gb.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " GB";
        }

        private static string FormatGbNum(double gb)
        {
            return gb.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }

        // =====================================================================
        // MEMORY METRICS
        // =====================================================================

        public static string GetTotalRam()
        {
            try
            {
                var mem = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(mem) && mem.ullTotalPhys > 0)
                {
                    double gb = mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    return FormatGb(gb);
                }
            }
            catch { }

            // Fallback to WMI
            try
            {
                using (var s = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject mo in s.Get())
                    {
                        var kb = Convert.ToDouble(mo["TotalVisibleMemorySize"]);
                        return FormatGb(kb / (1024.0 * 1024.0));
                    }
                }
            }
            catch { }

            return "Unknown RAM";
        }

        public static string GetFreeRam()
        {
            try
            {
                var mem = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(mem))
                {
                    double gb = mem.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                    return FormatGb(gb);
                }
            }
            catch { }

            try
            {
                using (var s = new ManagementObjectSearcher("SELECT FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject mo in s.Get())
                    {
                        var kb = Convert.ToDouble(mo["FreePhysicalMemory"]);
                        return FormatGb(kb / (1024.0 * 1024.0));
                    }
                }
            }
            catch { }

            return "Unknown Free RAM";
        }

        public static string GetUsedRam()
        {
            try
            {
                var mem = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(mem) && mem.ullTotalPhys > 0)
                {
                    ulong usedBytes = mem.ullTotalPhys - mem.ullAvailPhys;
                    double usedGb = usedBytes / (1024.0 * 1024.0 * 1024.0);
                    return $"{FormatGb(usedGb)} ({mem.dwMemoryLoad}% used)";
                }
            }
            catch { }

            return "Unknown Used RAM";
        }

        public static string GetRamSummary()
        {
            try
            {
                var mem = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(mem) && mem.ullTotalPhys > 0)
                {
                    double totalGb = mem.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    double freeGb = mem.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                    return $"Total: {FormatGb(totalGb)}, Free: {FormatGb(freeGb)} ({mem.dwMemoryLoad}% used)";
                }
            }
            catch { }

            return $"{GetTotalRam()} (Free: {GetFreeRam()})";
        }

        // =====================================================================
        // DISK METRICS
        // =====================================================================

        public static string GetDiskSpace(string driveLetter = "C")
        {
            try
            {
                var clean = (driveLetter ?? "C").Trim().TrimEnd(':', '\\');
                var drive = new DriveInfo(clean);
                if (!drive.IsReady) return $"{clean}: Not ready";

                double freeGb = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                double totalGb = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                double pctFree = totalGb > 0 ? (freeGb / totalGb * 100.0) : 0;
                return $"{FormatGb(freeGb)} free / {FormatGb(totalGb)} ({pctFree:0}% free)";
            }
            catch (Exception ex)
            {
                return $"Drive error ({driveLetter}): {ex.Message}";
            }
        }

        public static string GetDiskFreeGb(string driveLetter = "C")
        {
            try
            {
                var clean = (driveLetter ?? "C").Trim().TrimEnd(':', '\\');
                var drive = new DriveInfo(clean);
                if (!drive.IsReady) return "Not ready";
                double freeGb = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                return FormatGb(freeGb);
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string GetDiskTotalGb(string driveLetter = "C")
        {
            try
            {
                var clean = (driveLetter ?? "C").Trim().TrimEnd(':', '\\');
                var drive = new DriveInfo(clean);
                if (!drive.IsReady) return "Not ready";
                double totalGb = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                return FormatGb(totalGb);
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string GetAllDisksSummary()
        {
            try
            {
                var list = new List<string>();
                foreach (var d in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (d.DriveType == DriveType.Fixed && d.IsReady)
                        {
                            var letter = d.Name.TrimEnd('\\');
                            double freeGb = d.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
                            double totalGb = d.TotalSize / (1024.0 * 1024.0 * 1024.0);
                            double pctFree = totalGb > 0 ? (freeGb / totalGb * 100.0) : 0;
                            list.Add($"{letter} {FormatGbNum(freeGb)}/{FormatGb(totalGb)} ({pctFree:0}% free)");
                        }
                    }
                    catch { }
                }

                return list.Count > 0 ? string.Join(", ", list) : "No fixed drives";
            }
            catch (Exception ex)
            {
                return "Disk error: " + ex.Message;
            }
        }

        // =====================================================================
        // PRINTER METRICS
        // =====================================================================

        public static string GetPrinters()
        {
            var printerList = new List<string>();
            try
            {
                // Primary: Query WMI Win32_Printer to distinguish Default & Network printers
                using (var searcher = new ManagementObjectSearcher("SELECT Name, [Default], Network FROM Win32_Printer"))
                {
                    foreach (ManagementObject p in searcher.Get())
                    {
                        var name = p["Name"] as string;
                        if (string.IsNullOrWhiteSpace(name)) continue;

                        bool isDef = false;
                        try { isDef = Convert.ToBoolean(p["Default"] ?? false); } catch { }

                        bool isNet = false;
                        try { isNet = Convert.ToBoolean(p["Network"] ?? false); } catch { }

                        var flags = new List<string>();
                        if (isDef) flags.Add("Default");
                        if (isNet) flags.Add("Network");

                        if (flags.Count > 0)
                            printerList.Add($"{name} [{string.Join(", ", flags)}]");
                        else
                            printerList.Add(name);
                    }
                }
            }
            catch { }

            // Fallback: PrinterSettings.InstalledPrinters
            if (printerList.Count == 0)
            {
                try
                {
                    string defName = "";
                    try { defName = new PrinterSettings().PrinterName; } catch { }

                    foreach (string p in PrinterSettings.InstalledPrinters)
                    {
                        if (string.Equals(p, defName, StringComparison.OrdinalIgnoreCase))
                            printerList.Add($"{p} [Default]");
                        else
                            printerList.Add(p);
                    }
                }
                catch { }
            }

            return printerList.Count > 0 ? string.Join("; ", printerList) : "No printers installed";
        }

        public static string GetDefaultPrinter()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name, [Default] FROM Win32_Printer WHERE [Default] = TRUE"))
                {
                    foreach (ManagementObject p in searcher.Get())
                    {
                        var name = p["Name"] as string;
                        if (!string.IsNullOrWhiteSpace(name)) return name;
                    }
                }
            }
            catch { }

            try
            {
                var def = new PrinterSettings().PrinterName;
                if (!string.IsNullOrWhiteSpace(def)) return def;
            }
            catch { }

            return "None";
        }

        // =====================================================================
        // HARDWARE & OS METRICS
        // =====================================================================

        public static string GetCpuInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors FROM Win32_Processor"))
                {
                    foreach (ManagementObject proc in searcher.Get())
                    {
                        var name = (proc["Name"] as string ?? "").Trim();
                        var cores = proc["NumberOfCores"]?.ToString();
                        var threads = proc["NumberOfLogicalProcessors"]?.ToString();

                        if (!string.IsNullOrEmpty(cores) && !string.IsNullOrEmpty(threads))
                            return $"{name} ({cores} cores, {threads} threads)";
                        return name;
                    }
                }
            }
            catch { }

            return $"CPU ({Environment.ProcessorCount} cores)";
        }

        public static string GetOsInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption, OSArchitecture, BuildNumber FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        var caption = (os["Caption"] as string ?? "").Trim();
                        var arch = (os["OSArchitecture"] as string ?? "").Trim();
                        var build = os["BuildNumber"]?.ToString();
                        return $"{caption} ({arch}, build {build})";
                    }
                }
            }
            catch { }

            return Environment.OSVersion.VersionString + (Environment.Is64BitOperatingSystem ? " (64-bit)" : " (32-bit)");
        }

        public static string GetBootTime()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        var raw = os["LastBootUpTime"] as string;
                        if (!string.IsNullOrWhiteSpace(raw))
                        {
                            return ManagementDateTimeConverter.ToDateTime(raw).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                    }
                }
            }
            catch { }

            return "";
        }

        public static string GetUptime()
        {
            try
            {
                ulong ms = GetTickCount64();
                var span = TimeSpan.FromMilliseconds(ms);
                if (span.TotalDays >= 1)
                    return $"{(int)span.TotalDays}d {span.Hours}h {span.Minutes}m";
                return $"{span.Hours}h {span.Minutes}m {span.Seconds}s";
            }
            catch
            {
                try
                {
                    var boot = GetBootTime();
                    if (DateTime.TryParse(boot, out var dt))
                    {
                        var span = DateTime.Now - dt;
                        return $"{(int)span.TotalDays}d {span.Hours}h {span.Minutes}m";
                    }
                }
                catch { }
            }

            return "Unknown";
        }

        public static string GetPendingReboot()
        {
            try
            {
                // Check Component Based Servicing
                using (var cbs = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending"))
                {
                    if (cbs != null) return "Yes";
                }

                // Check Windows Update
                using (var wu = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired"))
                {
                    if (wu != null) return "Yes";
                }

                // Check PendingFileRenameOperations
                using (var sm = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager"))
                {
                    var val = sm?.GetValue("PendingFileRenameOperations");
                    if (val is string[] arr && arr.Length > 0) return "Yes";
                    if (val is string s && !string.IsNullOrWhiteSpace(s)) return "Yes";
                }

                return "No";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string GetBatteryStatus()
        {
            try
            {
                var power = SystemInformation.PowerStatus;
                if ((power.BatteryChargeStatus & BatteryChargeStatus.NoSystemBattery) != 0)
                {
                    return "AC only (No battery)";
                }

                int pct = (int)(power.BatteryLifePercent * 100);
                return $"{pct}% ({power.PowerLineStatus})";
            }
            catch
            {
                return "Unknown";
            }
        }

        public static string GetTopProcesses(int count = 5)
        {
            try
            {
                var top = Process.GetProcesses()
                    .OrderByDescending(p =>
                    {
                        try { return p.WorkingSet64; } catch { return 0; }
                    })
                    .Take(count)
                    .Select(p =>
                    {
                        try
                        {
                            double mb = p.WorkingSet64 / (1024.0 * 1024.0);
                            return $"{p.ProcessName} ({mb:0.0} MB)";
                        }
                        catch
                        {
                            return p.ProcessName;
                        }
                    });

                return string.Join(", ", top);
            }
            catch (Exception ex)
            {
                return "Processes error: " + ex.Message;
            }
        }

        public static string GetAdminSummary()
        {
            try
            {
                return $"Host: {Environment.MachineName} | User: {Logger.GetInteractiveUserNames()} | " +
                       $"RAM: {GetRamSummary()} | C: {GetDiskSpace("C")} | " +
                       $"Printers: {GetPrinters()} | OS: {GetOsInfo()} | Uptime: {GetUptime()} | " +
                       $"IP: {NetworkTools.GetIPs()} | RebootPending: {GetPendingReboot()}";
            }
            catch (Exception ex)
            {
                return "Summary error: " + ex.Message;
            }
        }

        // =====================================================================
        // ADVANCED PROVIDERS (Environment, Registry, Command execution)
        // =====================================================================

        public static string GetEnvVar(string varName)
        {
            if (string.IsNullOrWhiteSpace(varName)) return "";
            try
            {
                return Environment.GetEnvironmentVariable(varName) ?? "";
            }
            catch (Exception ex)
            {
                return $"[env error: {ex.Message}]";
            }
        }

        public static string GetRegistryValue(string fullKeyPath)
        {
            if (string.IsNullOrWhiteSpace(fullKeyPath)) return "";
            try
            {
                // Format: HKLM\Path\To\Key\ValueName or HKLM\Path\To\Key (default value)
                RegistryHive hive;
                string rest;
                if (fullKeyPath.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase))
                {
                    hive = RegistryHive.LocalMachine;
                    rest = fullKeyPath.Substring(5);
                }
                else if (fullKeyPath.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase))
                {
                    hive = RegistryHive.CurrentUser;
                    rest = fullKeyPath.Substring(5);
                }
                else
                {
                    hive = RegistryHive.LocalMachine;
                    rest = fullKeyPath;
                }

                int lastSlash = rest.LastIndexOf('\\');
                string subKey = lastSlash >= 0 ? rest.Substring(0, lastSlash) : "";
                string valName = lastSlash >= 0 ? rest.Substring(lastSlash + 1) : rest;

                using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
                using (var key = baseKey.OpenSubKey(subKey))
                {
                    if (key == null) return "[reg: key not found]";
                    var val = key.GetValue(valName) ?? key.GetValue("");
                    return val != null ? val.ToString() : "[reg: value not found]";
                }
            }
            catch (Exception ex)
            {
                return $"[reg error: {ex.Message}]";
            }
        }

        public static string ExecuteCommand(string commandLine, int timeoutMs = 4000)
        {
            if (string.IsNullOrWhiteSpace(commandLine)) return "";
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c " + commandLine,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    if (proc == null) return "[cmd: start failed]";
                    var output = proc.StandardOutput.ReadToEnd();
                    if (!proc.WaitForExit(timeoutMs))
                    {
                        try { proc.Kill(); } catch { }
                        return "[cmd: timeout]";
                    }
                    return output.Trim();
                }
            }
            catch (Exception ex)
            {
                return $"[cmd error: {ex.Message}]";
            }
        }

        // =====================================================================
        // TOKEN REGISTRATION & CONFIG EXTENSION
        // =====================================================================

        public static void RegisterToken(string name, string templateOrValue)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            lock (_syncLock)
            {
                _customTokens[name.Trim()] = templateOrValue ?? "";
            }
        }

        public static void RegisterCustomCommand(string name, string commandLine)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            lock (_syncLock)
            {
                _customCommands[name.Trim()] = commandLine ?? "";
            }
        }

        public static void RegisterProvider(string name, Func<string> provider)
        {
            if (string.IsNullOrWhiteSpace(name) || provider == null) return;
            lock (_syncLock)
            {
                _builtinProviders[name.Trim()] = provider;
            }
        }

        public static void LoadTokensFromXDocument(XDocument doc)
        {
            if (doc == null) return;
            try
            {
                var tokenNodes = doc.Descendants("Token").Concat(doc.Descendants("CustomToken"));
                foreach (var el in tokenNodes)
                {
                    var name = (string)el.Attribute("name") ?? (string)el.Attribute("id");
                    var val = (string)el.Attribute("value") ?? (string)el.Attribute("template") ?? (string)el.Attribute("contents") ?? el.Value;
                    var cmd = (string)el.Attribute("command") ?? (string)el.Attribute("cmd");

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (!string.IsNullOrWhiteSpace(cmd))
                            RegisterCustomCommand(name, cmd);
                        else if (!string.IsNullOrWhiteSpace(val))
                            RegisterToken(name, val);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("LoadTokensFromXDocument failed: " + ex.Message, Logger.TypeLog.local);
            }
        }

        public static void LoadTokensFromFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) return;
            try
            {
                var doc = XDocument.Load(filePath);
                LoadTokensFromXDocument(doc);
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"LoadTokensFromFile ({filePath}) failed: {ex.Message}", Logger.TypeLog.local);
            }
        }

        private static void TryLoadDefaultTokenFiles()
        {
            try
            {
                var candidates = new[]
                {
                    Path.Combine(Program.localPath, "tokens.xml"),
                    Path.Combine(Program.commandsLocalPath, "tokens.xml"),
                    Path.Combine(Program.mainPath, "tokens.xml")
                };

                foreach (var path in candidates)
                {
                    try
                    {
                        if (File.Exists(path)) LoadTokensFromFile(path);
                    }
                    catch { }
                }
            }
            catch { }
        }

        // =====================================================================
        // TOKEN RESOLUTION ENGINE
        // =====================================================================

        /// <summary>
        /// Evaluates any tokens (#token, {token}, {disk:X}, {env:VAR}, {cmd:...})
        /// within the given message string.
        /// </summary>
        public static string ResolveTokens(string message, int depth = 0)
        {
            if (string.IsNullOrEmpty(message) || depth > 5)
                return message ?? "";

            var trimmed = message.Trim();

            // 1) Direct single-token match (e.g. "#ram", "#free_c", "#printers", "ram", "free_c")
            string directKey = trimmed;
            if (directKey.StartsWith("#"))
                directKey = directKey.Substring(1);

            if (TryEvaluateToken(directKey, out var directResult))
            {
                return directResult;
            }

            // 2) Replace {placeholder} tokens
            var regexBraces = new Regex(@"\{([^}]+)\}", RegexOptions.IgnoreCase);
            var resolved = regexBraces.Replace(message, match =>
            {
                var key = match.Groups[1].Value.Trim();
                if (TryEvaluateToken(key, out var val))
                    return val;
                return match.Value; // keep untouched if unknown
            });

            // 3) Replace standalone #token identifiers in text (e.g. "RAM: #ram | Disk: #free_c")
            var regexHash = new Regex(@"#([a-zA-Z0-9_:]+)", RegexOptions.IgnoreCase);
            resolved = regexHash.Replace(resolved, match =>
            {
                var key = match.Groups[1].Value;
                if (TryEvaluateToken(key, out var val))
                    return val;
                return match.Value;
            });

            return resolved;
        }

        private static bool TryEvaluateToken(string key, out string result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(key)) return false;

            // Parameterized tokens: disk:X
            if (key.StartsWith("disk:", StringComparison.OrdinalIgnoreCase))
            {
                var letter = key.Substring(5);
                result = GetDiskSpace(letter);
                return true;
            }

            // Parameterized tokens: env:VAR
            if (key.StartsWith("env:", StringComparison.OrdinalIgnoreCase))
            {
                var varName = key.Substring(4);
                result = GetEnvVar(varName);
                return true;
            }

            // Parameterized tokens: reg:PATH
            if (key.StartsWith("reg:", StringComparison.OrdinalIgnoreCase))
            {
                var regPath = key.Substring(4);
                result = GetRegistryValue(regPath);
                return true;
            }

            // Parameterized tokens: cmd:...
            if (key.StartsWith("cmd:", StringComparison.OrdinalIgnoreCase))
            {
                var cmd = key.Substring(4);
                result = ExecuteCommand(cmd);
                return true;
            }

            // Custom commands registered by admin
            lock (_syncLock)
            {
                if (_customCommands.TryGetValue(key, out var cmd))
                {
                    result = ExecuteCommand(cmd);
                    return true;
                }

                // Custom tokens/templates registered by admin
                if (_customTokens.TryGetValue(key, out var template))
                {
                    result = ResolveTokens(template, depth: 1);
                    return true;
                }

                // Built-in providers
                if (_builtinProviders.TryGetValue(key, out var provider))
                {
                    try
                    {
                        result = provider() ?? "";
                        return true;
                    }
                    catch (Exception ex)
                    {
                        result = $"[error: {ex.Message}]";
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
