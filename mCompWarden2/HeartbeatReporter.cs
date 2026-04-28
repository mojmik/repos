using System;
using System.Collections.Generic;
using System.Management;

namespace mCompWarden2
{
    internal static class HeartbeatReporter
    {
        public static void Report(long pingMs)
        {
            try
            {
                var payload = new Dictionary<string, string>
                {
                    ["users"] = Logger.GetInteractiveUserNames(),
                    ["client_version"] = Program.GetVer(),
                    ["ip_addresses"] = NetworkTools.GetIPs(),
                    ["ping_ms"] = pingMs > -1 ? pingMs.ToString() : "",
                    ["agent_context"] = System.Environment.UserName,
                    ["os_version"] = GetOsCaption(),
                    ["boot_time"] = GetLastBootTime()
                };

                Logger.WriteRemoteStatus(payload);
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Heartbeat report failed: " + ex.Message, Logger.TypeLog.local);
            }
        }

        private static string GetOsCaption()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        return (os["Caption"] as string) ?? Environment.OSVersion.VersionString;
                    }
                }
            }
            catch
            {
            }

            return Environment.OSVersion.VersionString;
        }

        private static string GetLastBootTime()
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
            catch
            {
            }

            return "";
        }
    }
}
