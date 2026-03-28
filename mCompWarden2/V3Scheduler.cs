using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace mCompWarden2
{
    /// <summary>
    /// Interface for V3 schedules so CommandsManager can call AdvanceAfterRun.
    /// </summary>
    public interface IV3Schedule
    {
        void AdvanceAfterRun();
    }

    /// <summary>
    /// Holds parsed metadata from XML before mapping into a CommandSet.
    /// </summary>
    public class V3ScheduleTask
    {
        public string Id { get; set; }
        public bool Enabled { get; set; } = true;

        // schedule
        public string ScheduleType { get; set; } // daily, weekly, monthly, hourly, minutely
        public string WeeklyDaysCsv { get; set; } // e.g. "Mon,Wed,Fri"
        public int? MonthlyDay { get; set; }
        public string TimeHHmm { get; set; }      // for daily/weekly/monthly
        public int? HourlyMinute { get; set; }    // for hourly (0..59)
        public int? MinutelyInterval { get; set; }// for minutely (N>0)

        public List<string> CommandLines { get; } = new List<string>();

        public string MachineName { get; set; }   // optional
        public string UserName { get; set; }      // optional
        public string RunAs { get; set; } = "either"; // "user" | "system" | "either"
        public bool? NeedsNetwork { get; set; }   // optional
    }

    /// <summary>
    /// Loader for V3 XML files.
    /// </summary>
    public static class V3ConfigLoader
    {

        public static List<CommandSet> LoadFile(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                throw new FileNotFoundException("Config file not found", xmlPath);

            var doc = XDocument.Load(xmlPath);
            var tasks = new List<CommandSet>();

            foreach (var t in doc.Descendants("Task"))
            {
                var id = (string)t.Attribute("id") ?? "";
                bool enabled = ((string)t.Attribute("enabled"))?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? true;
                if (!enabled)
                    continue;

                var sched = t.Element("Schedule");
                if (sched == null)
                    throw new InvalidOperationException($"Task {id} missing <Schedule>.");

                var v3 = new V3ScheduleTask
                {
                    Id = id,
                    Enabled = enabled,
                    ScheduleType = (string)sched.Attribute("type") ?? "",
                    // targeting
                    MachineName = (string)t.Attribute("machine") ?? "",
                    UserName = (string)t.Attribute("user") ?? "",
                    RunAs = ((string)t.Attribute("runAs") ?? "either").ToLowerInvariant(),
                    NeedsNetwork = ParseBoolOrNull((string)t.Attribute("needsNetwork"))
                };

                // schedule attributes by type
                var type = (v3.ScheduleType ?? "").ToLowerInvariant();
                if (type == "daily" || type == "weekly" || type == "monthly")
                {
                    v3.WeeklyDaysCsv = (string)sched.Attribute("days") ?? "";
                    v3.TimeHHmm = (string)sched.Attribute("time") ?? "";
                    if (type == "monthly")
                    {
                        if (int.TryParse((string)sched.Attribute("day"), out var md))
                            v3.MonthlyDay = md;
                    }
                }
                else if (type == "hourly")
                {
                    if (!int.TryParse((string)sched.Attribute("minute"), out var m)) m = 0;
                    v3.HourlyMinute = Math.Max(0, Math.Min(59, m));
                }
                else if (type == "minutely")
                {
                    if (!int.TryParse((string)sched.Attribute("interval"), out var n) || n <= 0) n = 5;
                    v3.MinutelyInterval = n;
                }
                else if (type == "once")
                {
                    // no extra attributes; it's a fire-once task
                }
                else
                {
                    throw new InvalidOperationException($"Task {id}: unknown schedule type '{v3.ScheduleType}'.");
                }

                // --- parse actions (supports <Action> and <Actions><Action>) ---
                IEnumerable<XElement> actionNodes =
                    t.Elements("Action").Concat(t.Elements("Actions").Elements("Action"));

                if (!actionNodes.Any())
                    throw new InvalidOperationException($"Task {id} missing <Action>.");

                foreach (var act in actionNodes)
                {
                    var atype = ((string)act.Attribute("type") ?? "").ToLowerInvariant();

                    if (atype == "runprogram")
                    {
                        var file = System.Net.WebUtility.HtmlDecode((string)act.Attribute("file") ?? "");
                        var args = System.Net.WebUtility.HtmlDecode((string)act.Attribute("args") ?? "");
                        if (string.IsNullOrWhiteSpace(file))
                            throw new InvalidOperationException($"Task {id}: RunProgram requires 'file'.");
                        string cmd = Quote(file) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args);
                        v3.CommandLines.Add(cmd);
                    }
                    else if (atype == "writefile")
                    {
                        var target = System.Net.WebUtility.HtmlDecode(
                            (string)act.Attribute("target")
                            ?? (string)act.Attribute("path")  // legacy GUI field
                            ?? ""
                        );
                        var contents = System.Net.WebUtility.HtmlDecode((string)act.Attribute("contents") ?? "");
                        bool append = ((string)act.Attribute("append"))?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

                        if (string.IsNullOrWhiteSpace(target))
                            throw new InvalidOperationException($"Task {id}: WriteFile requires 'target' (or legacy 'path').");

                        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(contents));
                        var appendFlag = append ? "1" : "0";
                        var cmd = $"&writefile:path={target}|append={appendFlag}|encoding=utf8|payload={payload}";
                        v3.CommandLines.Add(cmd);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Task {id}: unknown Action '{atype}'.");
                    }
                }

                // Map into a V3CommandSet
                var cmdset = new V3CommandSet
                {
                    CommandLines = v3.CommandLines,
                    SourceFilePath = xmlPath,
                    SourceFileName = Path.GetFileName(xmlPath),
                    FileLastModified = File.GetLastWriteTime(xmlPath),

                    // V2 fields kept for compatibility; schedule advancement handled below
                    IsRepeating = !string.Equals(v3.ScheduleType, "once", StringComparison.OrdinalIgnoreCase),
                    RepeatingType = "d",
                    RepeatingInterval = 1,                    

                    ScheduleTypeV3 = v3.ScheduleType,
                    WeeklyDaysCsvV3 = v3.WeeklyDaysCsv,
                    MonthlyDayV3 = v3.MonthlyDay,
                    TimeHHmmV3 = v3.TimeHHmm,
                    HourlyMinuteV3 = v3.HourlyMinute ?? 0,
                    MinutelyIntervalV3 = v3.MinutelyInterval ?? 0,

                    // targeting → existing CommandSet fields
                    MachineName = string.IsNullOrWhiteSpace(v3.MachineName) ? "" : v3.MachineName,
                    UserName = string.IsNullOrWhiteSpace(v3.UserName) ? "" : v3.UserName,
                    NeedsUser = v3.RunAs == "user",
                    NeedsSystem = v3.RunAs == "system",
                    NeedsNetwork = v3.NeedsNetwork ?? false
                };

                // Seed first RunAt
                var now = DateTime.Now;
                if (string.Equals(v3.ScheduleType, "once", StringComparison.OrdinalIgnoreCase))
                {
                    // fire as soon as possible; a tiny offset prevents “past” evaluation
                    cmdset.RunAt = now.AddSeconds(2);
                    cmdset.RunAtTime = true;
                }
                else
                {
                    TimeSpan tod;
                    if (!TryParseHHmm(v3.TimeHHmm, out tod)) tod = new TimeSpan(0, 0, 0);

                    var schedule = (v3.ScheduleType ?? "").ToLowerInvariant();
                    bool isOneShot = schedule == "once";
                    if (schedule == "daily")
                    {
                        cmdset.RunAt = DateTime.Today.Add(tod);
                        if (cmdset.RunAt <= now) cmdset.RunAt = cmdset.RunAt.AddDays(1);
                        cmdset.RunAtTime = true;
                    }
                    else if (schedule == "weekly")
                    {
                        var next = ComputeNextWeeklyOccurrence(now, v3.WeeklyDaysCsv, tod);
                        cmdset.RunAt = next.When;
                        cmdset.RepeatingInterval = next.DaysUntil;
                        cmdset.RunAtTime = true;
                    }
                    else if (schedule == "monthly")
                    {
                        int day = Math.Max(1, Math.Min(31, v3.MonthlyDay ?? DateTime.Today.Day));
                        var next = ComputeNextMonthlyOccurrence(now, day, tod);
                        cmdset.RunAt = next.When;
                        cmdset.RepeatingInterval = (next.When - now).TotalDays;
                        cmdset.RunAtTime = true;
                    }
                    else if (schedule == "hourly")
                    {
                        int minute = v3.HourlyMinute ?? 0;
                        var candidate = new DateTime(now.Year, now.Month, now.Day, now.Hour, minute, 0);
                        if (candidate <= now) candidate = candidate.AddHours(1);
                        cmdset.RunAt = candidate;
                        cmdset.RunAtTime = true;
                    }
                    else if (schedule == "minutely")
                    {
                        int interval = v3.MinutelyInterval ?? 5;
                        cmdset.RunAt = RoundUpToNextMinuteInterval(now, interval);
                        cmdset.RunAtTime = true;
                    }
                }

                tasks.Add(cmdset);
            }

            return tasks;
        }


        // --- helpers ---
        private static bool? ParseBoolOrNull(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            bool b; return bool.TryParse(s, out b) ? b : (bool?)null;
        }
        private static string Quote(string s) => s.Contains(" ") ? "\"" + s + "\"" : s;
        

        public static bool TryParseHHmm(string txt, out TimeSpan result)
        {
            result = new TimeSpan();
            if (string.IsNullOrWhiteSpace(txt)) return false;
            var parts = txt.Split(':');
            int h, m;
            if (parts.Length == 2 && int.TryParse(parts[0], out h) && int.TryParse(parts[1], out m))
            {
                result = new TimeSpan(h, m, 0);
                return true;
            }
            return false;
        }

      

        private static string EscapePS(string s)
        {
            // In single-quoted PS strings, escape single quotes by doubling them
            return (s ?? "").Replace("'", "''");
        }

        public static (DateTime When, int DaysUntil) ComputeNextWeeklyOccurrence(DateTime now, string daysCsv, TimeSpan tod)
        {
            var days = new List<DayOfWeek>();
            foreach (var d in daysCsv.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                DayOfWeek dow;
                if (Enum.TryParse(d, true, out dow)) days.Add(dow);
            }
            if (days.Count == 0) days.Add(now.DayOfWeek);

            for (int i = 0; i < 14; i++)
            {
                var candidate = now.Date.AddDays(i).Add(tod);
                if (days.Contains(candidate.DayOfWeek) && candidate > now)
                    return (candidate, i);
            }
            return (now.AddDays(7), 7);
        }

        public static (DateTime When, int DaysUntil) ComputeNextMonthlyOccurrence(DateTime now, int day, TimeSpan tod)
        {
            var y = now.Year;
            var m = now.Month;
            DateTime candidate;
            try
            {
                candidate = new DateTime(y, m, day).Add(tod);
            }
            catch
            {
                int lastDay = DateTime.DaysInMonth(y, m);
                candidate = new DateTime(y, m, lastDay).Add(tod);
            }

            if (candidate <= now)
            {
                m++;
                if (m > 12) { m = 1; y++; }
                int lastDay = DateTime.DaysInMonth(y, m);
                int d = Math.Min(day, lastDay);
                candidate = new DateTime(y, m, d).Add(tod);
            }

            return (candidate, (candidate - now).Days);
        }

        public static DateTime RoundUpToNextMinuteInterval(DateTime now, int interval)
        {
            if (interval <= 1) return now.AddSeconds(-(now.Second)).AddMinutes(1); // next minute boundary
            // drop seconds, then compute next multiple of 'interval'
            var baseMinute = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
            int nextBucket = ((now.Minute / interval) * interval);
            if (baseMinute <= now) nextBucket += interval;
            if (nextBucket >= 60)
            {
                // roll to next hour
                var topHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
                return topHour.AddMinutes(nextBucket - 60);
            }
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, nextBucket, 0);
        }
    }

    /// <summary>
    /// V3CommandSet = wrapper around CommandSet with schedule logic that advances after each run.
    /// </summary>
    public class V3CommandSet : CommandSet, IV3Schedule
    {
        public string ScheduleTypeV3 { get; set; }
        public string WeeklyDaysCsvV3 { get; set; }
        public int? MonthlyDayV3 { get; set; }
        public string TimeHHmmV3 { get; set; }

        // NEW for hourly / minutely
        public int HourlyMinuteV3 { get; set; } = 0;
        public int MinutelyIntervalV3 { get; set; } = 0;

        public void AdvanceAfterRun()
        {
            // For one-time tasks, do nothing here.
            if (string.Equals(ScheduleTypeV3, "once", StringComparison.OrdinalIgnoreCase))
                return;

            DateTime now = DateTime.Now;
            TimeSpan tod;
            if (!V3ConfigLoader.TryParseHHmm(TimeHHmmV3, out tod))
                tod = RunAt.TimeOfDay;

            var type = (ScheduleTypeV3 ?? "").ToLowerInvariant();
            if (type == "daily")
            {
                RunAt = DateTime.Today.AddDays(1).Add(tod);
            }
            else if (type == "weekly")
            {
                var next = V3ConfigLoader.ComputeNextWeeklyOccurrence(now, WeeklyDaysCsvV3, tod);
                RunAt = next.When;
                RepeatingInterval = next.DaysUntil;
            }
            else if (type == "monthly")
            {
                int day = Math.Max(1, Math.Min(31, MonthlyDayV3 ?? DateTime.Today.Day));
                var next = V3ConfigLoader.ComputeNextMonthlyOccurrence(now, day, tod);
                RunAt = next.When;
                RepeatingInterval = (next.When - now).TotalDays;
            }
            else if (type == "hourly")
            {
                int minute = Math.Max(0, Math.Min(59, HourlyMinuteV3));
                var cand = new DateTime(now.Year, now.Month, now.Day, now.Hour, minute, 0).AddHours(1);
                if (cand <= now) cand = cand.AddHours(1);
                RunAt = cand;
            }
            else if (type == "minutely")
            {
                int interval = (MinutelyIntervalV3 <= 0) ? 5 : MinutelyIntervalV3;
                RunAt = V3ConfigLoader.RoundUpToNextMinuteInterval(now, interval);
            }
        }


    }
}
