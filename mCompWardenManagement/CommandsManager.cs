using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace mCompWardenManagement
{
    class CommandsManager
    {
        public List<CommandFile> commandFiles = new List<CommandFile>();

        public List<string> repeatingTypes = new List<string>();
        public static string FileExtensionV2 = "cwd";
        public static string FileExtensionV3 = "mcw3.xml";

        public CommandsManager()
        {
            // typy opakování pro V2
            repeatingTypes.Add("");   // none
            repeatingTypes.Add("s");  // seconds
            repeatingTypes.Add("m");  // minutes
            repeatingTypes.Add("h");  // hours
            repeatingTypes.Add("d");  // days
            repeatingTypes.Add("!");  // run once at startup
        }

        public CommandFile AddCommandFile()
        {
            CommandFile cf = new CommandFile();
            commandFiles.Add(cf);
            return cf;
        }

        /// <summary>
        /// Uloží CommandFile podle zvoleného typu (V2 nebo V3)
        /// </summary>
        public bool SaveCommandFile(CommandFile cf, string path, bool isV3)
        {
            try
            {
                if (isV3) return cf.SaveV3(path);
                else return cf.Save(path); // V2 používá Save()
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Chyba při ukládání: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Vrátí seznam existujících souborů příkazů (V2 i V3)
        /// </summary>
        public IEnumerable<string> GetExistingFiles(string folder)
        {
            var result = new List<string>();
            if (Directory.Exists(folder))
            {
                result.AddRange(Directory.GetFiles(folder, "*." + FileExtensionV2));
                result.AddRange(Directory.GetFiles(folder, "*." + FileExtensionV3));
            }
            return result;
        }

        /// <summary>
        /// Načte CommandFile z V2 textového souboru
        /// </summary>
        public CommandFile LoadV2(string filePath)
        {
            var cf = new CommandFile();
            cf.SourceFilePath = filePath;
            cf.SourceFileName = Path.GetFileName(filePath);

            var lines = File.ReadAllLines(filePath);
            var cmds = new List<string>();

            bool inCommands = false;
            foreach (var raw in lines)
            {
                var l = raw ?? "";
                if (l.StartsWith("@")) { continue; } // hlavička

                if (!inCommands && l.StartsWith("commands", StringComparison.OrdinalIgnoreCase))
                {
                    inCommands = true;
                    continue;
                }

                if (!inCommands)
                {
                    int idx = l.IndexOf(':');
                    if (idx > 0)
                    {
                        var key = l.Substring(0, idx);
                        var val = l.Substring(idx + 1);

                        switch (key)
                        {
                            case "MachineName": cf.MachineName = val; break;
                            case "UserName": cf.UserName = val; break;
                            case "IsRepeating": cf.IsRepeating = SafeBool(val); break;
                            case "IsRemote": cf.IsRemote = SafeBool(val); break;
                            case "NeedsNetwork": cf.NeedsNetwork = SafeBool(val); break;
                            case "NeedsSystem": cf.NeedsSystem = SafeBool(val); break;
                            case "NeedsUser": cf.NeedsUser = SafeBool(val); break;
                            case "RunAt": cf.RunAt = val; break;
                            case "RepeatingType": cf.RepeatingType = val; break;
                            case "RepeatingInterval": cf.RepeatingInterval = SafeDouble(val); break;
                            case "ExcludedComputers": cf.ExcludedComputers = (val ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); break;
                            case "ExcludedComputersRegex": cf.ExcludedComputersRegex = val; break;
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(l))
                        cmds.Add(l);
                }
            }

            cf.CommandLines = cmds;
            return cf;
        }

        /// <summary>
        /// Načte CommandFile z V3 XML souboru (Tasks/Task/Schedule + Actions/Action).
        /// </summary>
        public CommandFile LoadV3(string filePath)
        {
            var xdoc = System.Xml.Linq.XDocument.Load(filePath);

            // Root can be <Tasks> with many <Task> or a single <Task>
            var tasksRoot = xdoc.Root;

            // Grab first <Task> for editing (UI currently supports one at a time)
            System.Xml.Linq.XElement task;
            if (tasksRoot != null && tasksRoot.Name.LocalName.Equals("Tasks", StringComparison.OrdinalIgnoreCase))
            {
                task = tasksRoot.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("Task", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                task = xdoc.Element("Task");
            }

            if (task == null)
                throw new InvalidOperationException("V3: Nenalezen žádný <Task> v souboru.");

            var cf = new CommandFile
            {
                Kind = CommandFile.FileKind.V3,
                SourceFilePath = filePath,
                SourceFileName = Path.GetFileName(filePath)
            };

            // Preserve defaultTimezone if present
            if (tasksRoot != null && tasksRoot.Name.LocalName.Equals("Tasks", StringComparison.OrdinalIgnoreCase))
            {
                var tzAttr = tasksRoot.Attribute("defaultTimezone");
                if (tzAttr != null) cf.DefaultTimezone = (string)tzAttr;
            }

            // Task attrs
            cf.V3Id = (string)task.Attribute("id") ?? "task1";
            bool enabled;
            cf.V3Enabled = bool.TryParse((string)task.Attribute("enabled"), out enabled) ? enabled : true;
            cf.MachineName = (string)task.Attribute("machine") ?? "";
            cf.UserName = (string)task.Attribute("user") ?? "";
            cf.RunAs = (string)task.Attribute("runAs") ?? "either";

            // Description
            var desc = task.Element(task.Name.Namespace + "Description") ??
                       task.Element("Description");
            cf.Description = desc != null ? (string)desc : null;

            // Schedule
            var sched = task.Element(task.Name.Namespace + "Schedule") ??
                        task.Element("Schedule");
            if (sched == null)
                throw new InvalidOperationException("V3: Chybí <Schedule>.");

            var sType = ((string)sched.Attribute("type") ?? "daily").ToLowerInvariant();
            cf.ScheduleType = sType;

            if (sType == "hourly")
            {
                int m;
                if (int.TryParse((string)sched.Attribute("minute"), out m))
                    cf.HourlyMinute = m;
            }
            else if (sType == "minutely")
            {
                int iv;
                if (int.TryParse((string)sched.Attribute("interval"), out iv) && iv > 0)
                    cf.MinutelyInterval = iv;
                else
                    cf.MinutelyInterval = 5; // sane default if missing/invalid
            }
            else if (sType == "weekly")
            {
                var days = (string)sched.Attribute("days") ?? "";
                cf.WeeklyDays = days
                    .Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
                cf.TimeHHmm = (string)sched.Attribute("time") ?? "00:00";
            }
            else if (sType == "monthly")
            {
                int md;
                if (int.TryParse((string)sched.Attribute("day"), out md))
                    cf.MonthlyDay = md;
                cf.TimeHHmm = (string)sched.Attribute("time") ?? "00:00";
            }
            else // daily or default
            {
                cf.TimeHHmm = (string)sched.Attribute("time") ?? "00:00";
            }

            // Actions
            cf.V3Actions.Clear();
            // Try both <Actions><Action/> and top-level <Action/>
            var actionsParent = task.Element(task.Name.Namespace + "Actions") ??
                                task.Element("Actions");
            IEnumerable<System.Xml.Linq.XElement> actionNodes;
            if (actionsParent != null)
                actionNodes = actionsParent.Elements().Where(e => e.Name.LocalName.Equals("Action", StringComparison.OrdinalIgnoreCase));
            else
                actionNodes = task.Elements().Where(e => e.Name.LocalName.Equals("Action", StringComparison.OrdinalIgnoreCase));

            foreach (var a in actionNodes)
            {
                var t = ((string)a.Attribute("type") ?? "").ToLowerInvariant();
                if (t == "runprogram")
                {
                    cf.V3Actions.Add(new CommandFile.V3Action
                    {
                        Type = "RunProgram",
                        File = (string)a.Attribute("file") ?? "",
                        Args = (string)a.Attribute("args") ?? ""
                    });
                }
                else if (t == "writefile")
                {
                    bool app;
                    bool? appendVal = null;
                    var appendAttr = (string)a.Attribute("append");
                    if (!string.IsNullOrEmpty(appendAttr) && bool.TryParse(appendAttr, out app))
                        appendVal = app;

                    cf.V3Actions.Add(new CommandFile.V3Action
                    {
                        Type = "WriteFile",
                        Path = (string)a.Attribute("path") ?? "",
                        Contents = (string)a.Attribute("contents") ?? "",
                        Append = appendVal
                    });
                }
                // unknown types ignored (or you can throw)
            }

            // Back-compat: derive CommandLines for UI preview if needed
            cf.CommandLines = cf.V3Actions.Select(v =>
            {
                if (v.Type == "RunProgram")
                    return string.IsNullOrWhiteSpace(v.Args) ? v.File : (v.File + " " + v.Args);
                if (v.Type == "WriteFile")
                    return $"WriteFile -> {v.Path}";
                return "";
            }).Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            return cf;
        }

        private static bool SafeBool(string s)
        {
            bool b; return bool.TryParse(s, out b) ? b : false;
        }
        private static double SafeDouble(string s)
        {
            double d; return double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out d) ? d : 0.0;
        }
    }
}
