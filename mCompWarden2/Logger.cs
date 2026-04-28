using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using static mCompWarden2.Program;

namespace mCompWarden2
{
    public static class Logger
    {
        public static string logFilePath, remoteLogPath, remoteInfoPath, remoteUrl, remoteStatusUrl;

        public enum TypeLog { local, remote, both }

        // ---------------- helpers ----------------
        private static void EnsureDirForFile(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
            }
            catch { /* never throw from logger */ }
        }

        private static void SafeAppendLine(string path, string line)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                EnsureDirForFile(path);
                // Uses your retry helper you added to Program.cs
                IoSafe.AppendAllTextRetry(path, line + Environment.NewLine);
            }
            catch { /* swallow */ }
        }

        private static string NowDt()
        {
            // Stable, locale-independent timestamp
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        // ---------------- public API ----------------
        public static void WriteLog(string txt, TypeLog typLogu)
        {
            try
            {
                var prefix = $"{NowDt()} {GetComputerName()} {GetUserName()} {Program.GetVer()} ";
                var line = prefix + (txt ?? string.Empty);

                if (typLogu == TypeLog.local || typLogu == TypeLog.both)
                    SafeAppendLine(logFilePath, line);

                if (typLogu == TypeLog.remote || typLogu == TypeLog.both)
                    SafeAppendLine(remoteLogPath, line);
            }
            catch
            {
                // never break caller on logging
            }
        }

        public static void WriteRemoteInfo(string widget, string value, bool postLog = true)
        {
            try
            {
                if (postLog && !string.IsNullOrWhiteSpace(remoteUrl))
                {
                    var data = new System.Collections.Specialized.NameValueCollection
                    {
                        ["comp"] = GetComputerName(),
                        ["user"] = GetUserNames(),
                        ["opt"] = widget ?? "",
                        ["val"] = value ?? "",
                        ["dt"] = NowDt()
                    };

                    try
                    {
                        using (var wb = new WebClient())
                        {
                            // WebClient has no built-in timeout in .NET 4.8; rely on server/OS
                            var response = wb.UploadValues(remoteUrl, "POST", data);
                            // optionally log success/response if you want:
                            // SafeAppendLine(logFilePath, $"{NowDt()} RemoteInfo POST ok: {Encoding.UTF8.GetString(response)}");
                        }
                    }
                    catch
                    {
                        // Fall back to local log on POST failure (do not throw)
                        SafeAppendLine(logFilePath, $"{NowDt()} RemoteInfo POST failed");
                    }
                }
                else
                {
                    var line = $"{GetComputerName()};{GetUserName()};{widget};{value}";
                    var fileName = Path.Combine(remoteInfoPath ?? "", $"mlog_{GetRnd()}_{GetComputerName()}_{GetUserName()}.txt");
                    EnsureDirForFile(fileName);
                    SafeAppendLine(fileName, line);
                }
            }
            catch
            {
                // swallow
            }
        }

        public static bool WriteRemoteStatus(Dictionary<string, string> values)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(remoteStatusUrl))
                    return false;

                var data = new System.Collections.Specialized.NameValueCollection
                {
                    ["comp"] = GetComputerName(),
                    ["dt"] = NowDt()
                };

                if (values != null)
                {
                    foreach (var pair in values)
                    {
                        data[pair.Key ?? ""] = pair.Value ?? "";
                    }
                }

                using (var wb = new WebClient())
                {
                    wb.UploadValues(remoteStatusUrl, "POST", data);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        // ---------------- misc helpers ----------------
        private static string GetRnd()
        {
            var rnd = new Random();
            return rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999);
        }

        public static string GetComputerName() => Environment.MachineName;

        public static string GetUserName() => Environment.UserName;

        public static string GetInteractiveUserNames()
        {
            // Enumerate interactive users via explorer.exe owners; be resilient
            try
            {
                var users = new List<string>();
                using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Process"))
                {
                    foreach (ManagementObject proc in searcher.Get())
                    {
                        var exePath = proc["ExecutablePath"] as string;
                        if (exePath != null && string.Equals(Path.GetFileName(exePath), "explorer.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            var owner = new string[2];
                            proc.InvokeMethod("GetOwner", owner);
                            if (!string.IsNullOrWhiteSpace(owner[0]))
                                users.Add(owner[0]);
                        }
                    }
                }
                if (users.Count == 0) users.Add(Environment.UserName);
                return string.Join("-", users.Distinct().ToArray());
            }
            catch
            {
                return Environment.UserName;
            }
        }

        private static string GetUserNames()
        {
            return GetInteractiveUserNames();
        }
    }
}
