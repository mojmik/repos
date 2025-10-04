using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mCompWardenManagement
{
    class CommandFile
    {
        // --- společné vlastnosti ---
        public List<string> CommandLines { get; set; } = new List<string>();
        public DateTime LastRun { get; set; }
        public string SourceFilePath { get; set; }
        public string SourceFileName { get; set; }

        // --- V2 vlastnosti ---
        public string RunAt { get; set; }
        public string RepeatingType { get; set; }
        public double RepeatingInterval { get; set; }
        public string[] ExcludedComputers { get; set; }
        public string ExcludedComputersRegex { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }

        public bool IsRepeating { get; set; }
        public bool IsArchived { get; set; }
        public bool IsRemote { get; set; }
        public bool NeedsNetwork { get; set; }
        public bool NeedsUser { get; set; }
        public bool NeedsSystem { get; set; }

        // --- V3 vlastnosti ---
        public string ScheduleType { get; set; }    // daily / weekly / monthly
        public string TimeHHmm { get; set; }        // "HH:mm"
        public int? MonthlyDay { get; set; }        // den v měsíci
        public List<string> WeeklyDays { get; set; } = new List<string>();
        // --- V3 extra ---
        public string V3Id { get; set; } = "task1";
        public bool V3Enabled { get; set; } = true;
        public string Description { get; set; }
        public int? HourlyMinute { get; set; }   // for type="hourly"
        public int? MinutelyInterval { get; set; }   // for type="minutely" (>=1)
        public string DefaultTimezone { get; set; }  // from <Tasks defaultTimezone="...">

        // --- typ souboru ---
        public enum FileKind { V2, V3 }
        public FileKind Kind { get; set; } = FileKind.V2;

        public class V3Action
        {
            public string Type { get; set; } // RunProgram, WriteFile
            public string File { get; set; }
            public string Args { get; set; }
            public string Path { get; set; }
            public string Contents { get; set; }
            public bool? Append { get; set; }
        }
        public List<V3Action> V3Actions { get; set; } = new List<V3Action>();

        public CommandFile() { }

        // ----------------- V2 fluent API -----------------
        public CommandFile SetRepeating(string rep, string repType)
        {
            if ((repType == "") || (repType == "0"))
            {
                IsRepeating = false;
                RepeatingInterval = 0;
                RepeatingType = "";
            }
            else
            {
                IsRepeating = true;
                double val;
                if (!double.TryParse(rep, out val)) val = 0;
                RepeatingInterval = val;
                RepeatingType = repType;
            }
            return this;
        }
        public CommandFile SetRemote(bool remote) { IsRemote = remote; return this; }
        public CommandFile SetNetwork(bool network) { NeedsNetwork = network; return this; }
        public CommandFile SetRunAt(string inStr) { RunAt = inStr; return this; }
        public CommandFile SetMachineName(string inStr) { MachineName = (inStr ?? "").ToLower(); return this; }
        public CommandFile SetUserName(string inStr) { UserName = inStr; return this; }
        public CommandFile SetUser(bool b) { NeedsUser = b; return this; }
        public CommandFile SetSystem(bool b) { NeedsSystem = b; return this; }
        public CommandFile SetExcluded(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
                ExcludedComputers = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return this;
        }
        public CommandFile SetExcludedRegex(string str) { ExcludedComputersRegex = str; return this; }
        public CommandFile SetCommandLines(List<string> lines)
        {
            CommandLines = new List<string>();
            if (lines != null)
            {
                foreach (string line in lines)
                {
                    var clean = (line ?? "").Replace("\r", "").Replace("\n", "");
                    if (!string.IsNullOrWhiteSpace(clean))
                        CommandLines.Add(clean);
                }
            }
            return this;
        }

        // ----------------- V2 Save -----------------
        // POZN.: Zachováváme název Save(...) a přidáváme SaveV2(...) jako alias,
        // aby fungovaly všechny starší i nové volající.
        public bool Save(string filePath)
        {
            if (Kind != FileKind.V2)
                throw new InvalidOperationException("Save() je určené jen pro V2. Pro V3 použij SaveV3().");

            if (string.IsNullOrEmpty(filePath)) return false;
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var file = new StreamWriter(filePath, true))
            {
                file.WriteLine("@cmdsettingsV2");
                if (!string.IsNullOrEmpty(MachineName)) file.WriteLine("MachineName:" + MachineName);
                if (!string.IsNullOrEmpty(UserName)) file.WriteLine("UserName:" + UserName);
                file.WriteLine("IsRepeating:" + IsRepeating);
                file.WriteLine("IsRemote:" + IsRemote);
                file.WriteLine("NeedsNetwork:" + NeedsNetwork);
                file.WriteLine("NeedsSystem:" + NeedsSystem);
                file.WriteLine("NeedsUser:" + NeedsUser);
                if (RunAt != null) file.WriteLine("RunAt:" + RunAt);
                file.WriteLine("RepeatingType:" + (RepeatingType ?? ""));
                file.WriteLine("RepeatingInterval:" + (long)RepeatingInterval);
                if (ExcludedComputers != null && ExcludedComputers.Length > 0)
                    file.WriteLine("ExcludedComputers:" + string.Join(",", ExcludedComputers));
                file.WriteLine("ExcludedComputersRegex:" + (ExcludedComputersRegex ?? ""));
                file.WriteLine("commands:");
                foreach (string cmd in CommandLines)
                    file.WriteLine(cmd);
            }
            return true;
        }

        // alias kvůli starším voláním
        public bool SaveV2(string filePath) => Save(filePath);

        // ----------------- V3 Save -----------------
        public bool SaveV3(string filePath)
        {
            if (Kind != FileKind.V3)
                throw new InvalidOperationException("SaveV3() je určené jen pro V3.");

            if (string.IsNullOrEmpty(filePath)) return false;
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var w = new StreamWriter(filePath, false))
            {
                // Root
                if (!string.IsNullOrWhiteSpace(DefaultTimezone))
                    w.WriteLine($"<Tasks defaultTimezone=\"{System.Security.SecurityElement.Escape(DefaultTimezone)}\">");
                else
                    w.WriteLine("<Tasks>");

                w.Write($"  <Task id=\"{System.Security.SecurityElement.Escape(V3Id ?? "task1")}\" enabled=\"{(V3Enabled ? "true" : "false")}\">");
                w.WriteLine();

                if (!string.IsNullOrWhiteSpace(Description))
                    w.WriteLine($"    <Description>{System.Security.SecurityElement.Escape(Description)}</Description>");

                // Schedule
                var type = (ScheduleType ?? "daily").ToLowerInvariant();
                if (type == "hourly")
                {
                    var minute = HourlyMinute.HasValue ? HourlyMinute.Value.ToString() : "0";
                    w.WriteLine($"    <Schedule type=\"hourly\" minute=\"{minute}\" />");
                }
                else if (type == "minutely")
                {
                    var iv = (MinutelyInterval.HasValue && MinutelyInterval.Value > 0)
                                ? MinutelyInterval.Value
                                : 5; // sane default
                    w.WriteLine($"    <Schedule type=\"minutely\" interval=\"{iv}\" />");
                }
                else if (type == "weekly")
                {
                    var days = (WeeklyDays != null && WeeklyDays.Count > 0) ? string.Join(",", WeeklyDays) : "";
                    var time = string.IsNullOrWhiteSpace(TimeHHmm) ? "00:00" : TimeHHmm;
                    w.WriteLine($"    <Schedule type=\"weekly\" days=\"{days}\" time=\"{time}\" />");
                }
                else if (type == "monthly")
                {
                    var time = string.IsNullOrWhiteSpace(TimeHHmm) ? "00:00" : TimeHHmm;
                    var day = MonthlyDay.HasValue ? MonthlyDay.Value.ToString() : "1";
                    w.WriteLine($"    <Schedule type=\"monthly\" day=\"{day}\" time=\"{time}\" />");
                }
                else // daily default
                {
                    var time = string.IsNullOrWhiteSpace(TimeHHmm) ? "00:00" : TimeHHmm;
                    w.WriteLine($"    <Schedule type=\"daily\" time=\"{time}\" />");
                }

                // Actions
                w.WriteLine("    <Actions>");
                if (V3Actions != null && V3Actions.Count > 0)
                {
                    foreach (var a in V3Actions)
                    {
                        var t = (a.Type ?? "RunProgram"); // canonical case
                        if (t.Equals("runprogram", StringComparison.OrdinalIgnoreCase) || t.Equals("RunProgram"))
                        {
                            var file = System.Security.SecurityElement.Escape(a.File ?? "");
                            var args = string.IsNullOrWhiteSpace(a.Args) ? "" : $" args=\"{System.Security.SecurityElement.Escape(a.Args)}\"";
                            w.WriteLine($"      <Action type=\"RunProgram\" file=\"{file}\"{args} />");
                        }
                        else if (t.Equals("writefile", StringComparison.OrdinalIgnoreCase) || t.Equals("WriteFile"))
                        {
                            var path = System.Security.SecurityElement.Escape(a.Path ?? "");
                            var contents = System.Security.SecurityElement.Escape(a.Contents ?? "");
                            var appendAttr = (a.Append.HasValue && a.Append.Value) ? " append=\"true\"" : "";
                            w.WriteLine($"      <Action type=\"WriteFile\" path=\"{path}\" contents=\"{contents}\"{appendAttr} />");
                        }
                    }
                }
                else
                {
                    // Back-compat: turn plain CommandLines into RunProgram actions
                    foreach (var cmd in CommandLines)
                    {
                        var file = System.Security.SecurityElement.Escape(cmd ?? "");
                        w.WriteLine($"      <Action type=\"RunProgram\" file=\"{file}\" />");
                    }
                }
                w.WriteLine("    </Actions>");

                w.WriteLine("  </Task>");
                w.WriteLine("</Tasks>");
            }
            return true;
        }
    }
}
