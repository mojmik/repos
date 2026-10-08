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
        public string RunAs { get; set; } = "either"; // "either" | "user" | "system"  (<Task runAs="...">)

        // ---- V3: metadata/schedule (ponechte co už máte) ----
        public string V3Id { get; set; } = "";
        public bool V3Enabled { get; set; } = true;
        public string Description { get; set; } = "";
        public string ScheduleType { get; set; } = "daily";
        public string TimeHHmm { get; set; } = "08:00";
        public List<string> WeeklyDays { get; set; } = new List<string>();
        public int? MonthlyDay { get; set; }
        public int? HourlyMinute { get; set; }
        public int? MinutelyInterval { get; set; }
        public string DefaultTimezone { get; set; } = "";

        public bool SingleInstance { get; set; }
        public string WorkDir { get; set; } = "";
        public string Domain { get; set; } = "";
        public string PasswordEnc { get; set; } = "";

        // Seznam akcí V3
        public List<V3Action> V3Actions { get; } = new List<V3Action>();

        // --- typ souboru ---
        public enum FileKind { V2, V3 }
        public FileKind Kind { get; set; } = FileKind.V2;

        public class V3Action
        {
            public string Type { get; set; } = "RunProgram"; // RunProgram | WriteFile | PostMessage
            public string Target { get; set; } = "";         // exe path, cílový soubor, nebo widget
            public string Args { get; set; } = "";
            public string Contents { get; set; } = "";       // contents pro WriteFile, nebo message pro PostMessage
            public bool? Append { get; set; }

            // Back-compat pro starší kód/XAML sloupce:
            public string File { get => Target; set => Target = value; } // pro RunProgram
            public string Path { get => Target; set => Target = value; } // pro WriteFile
            public string Widget { get => Target; set => Target = value; } // pro PostMessage
            public string Message { get => Contents; set => Contents = value; } // pro PostMessage
        }
        

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

                // ----- Task open tag with targeting -----
                var idAttr = System.Security.SecurityElement.Escape(V3Id ?? "task1");
                var enabledAttr = (V3Enabled ? "true" : "false");

                // new optional attributes
                var machineAttr = string.IsNullOrWhiteSpace(MachineName) ? "" : $" machine=\"{System.Security.SecurityElement.Escape(MachineName)}\"";
                var userAttr = string.IsNullOrWhiteSpace(UserName) ? "" : $" user=\"{System.Security.SecurityElement.Escape(UserName)}\"";
                var runAsValue = string.IsNullOrWhiteSpace(RunAs) ? "either" : RunAs.ToLowerInvariant();
                var runAsAttr = runAsValue == "either" ? "" : $" runAs=\"{System.Security.SecurityElement.Escape(runAsValue)}\"";
                var needsNetAttr = NeedsNetwork ? " needsNetwork=\"true\"" : "";
                var singleInstAttr = SingleInstance ? " singleInstance=\"true\"" : "";
                var workDirAttr = string.IsNullOrWhiteSpace(WorkDir) ? "" : $" workDir=\"{System.Security.SecurityElement.Escape(WorkDir)}\"";
                var domainAttr = string.IsNullOrWhiteSpace(Domain) ? "" : $" domain=\"{System.Security.SecurityElement.Escape(Domain)}\"";
                var passEncAttr = string.IsNullOrWhiteSpace(PasswordEnc) ? "" : $" passwordEnc=\"{System.Security.SecurityElement.Escape(PasswordEnc)}\"";

                w.WriteLine($"  <Task id=\"{idAttr}\" enabled=\"{enabledAttr}\"{machineAttr}{userAttr}{runAsAttr}{needsNetAttr}{singleInstAttr}{workDirAttr}{domainAttr}{passEncAttr}>");

                if (!string.IsNullOrWhiteSpace(Description))
                    w.WriteLine($"    <Description>{System.Security.SecurityElement.Escape(Description)}</Description>");

                // ----- Schedule -----
                var type = (ScheduleType ?? "daily").ToLowerInvariant();
                if (type == "hourly")
                {
                    var minute = HourlyMinute.HasValue ? HourlyMinute.Value.ToString() : "0";
                    w.WriteLine($"    <Schedule type=\"hourly\" minute=\"{minute}\" />");
                }
                else if (type == "minutely")
                {
                    var iv = (MinutelyInterval.HasValue && MinutelyInterval.Value > 0) ? MinutelyInterval.Value : 5;
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

                // ----- Actions -----
                w.WriteLine("    <Actions>");
                if (V3Actions != null && V3Actions.Count > 0)
                {
                    foreach (var a in V3Actions)
                    {
                        var t = (a.Type ?? "RunProgram");
                        if (t.Equals("RunProgram", StringComparison.OrdinalIgnoreCase))
                        {
                            var file = System.Security.SecurityElement.Escape(a.File ?? a.Target ?? "");
                            var args = string.IsNullOrWhiteSpace(a.Args) ? "" : $" args=\"{System.Security.SecurityElement.Escape(a.Args)}\"";
                            w.WriteLine($"      <Action type=\"RunProgram\" file=\"{file}\"{args} />");
                        }
                        else if (t.Equals("WriteFile", StringComparison.OrdinalIgnoreCase))
                        {
                            var path = System.Security.SecurityElement.Escape(a.Path ?? a.Target ?? "");
                            var contents = System.Security.SecurityElement.Escape(a.Contents ?? "");
                            var appendAttr = (a.Append.HasValue && a.Append.Value) ? " append=\"true\"" : "";
                            w.WriteLine($"      <Action type=\"WriteFile\" path=\"{path}\" contents=\"{contents}\"{appendAttr} />");
                        }
                        else if (t.Equals("PostMessage", StringComparison.OrdinalIgnoreCase) || t.Equals("Post", StringComparison.OrdinalIgnoreCase))
                        {
                            var widget = System.Security.SecurityElement.Escape(a.Widget ?? a.Target ?? "info");
                            var message = System.Security.SecurityElement.Escape(a.Message ?? a.Contents ?? "");
                            w.WriteLine($"      <Action type=\"PostMessage\" widget=\"{widget}\" message=\"{message}\" />");
                        }
                    }
                }
                else
                {
                    // Back-compat: prostý seznam příkazů jako RunProgram
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
