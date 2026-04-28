using System;
using System.IO;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Policy;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;

namespace mCompWarden2
{
    public class CommandSet
    {
        public List<string> CommandLines { get; set; }
        public DateTime LastRun { get; set; }
        public DateTime FileLastModified { get; set; }
        public DateTime RunAt { get; set; }
        public bool RunAtTime;
        public bool RunAtRan;
        public string SourceFilePath { get; set; }
        public string SourceFileHash { get; set; }
        public string SourceFileName { get; set; }
        public string RepeatingType { get; set; }
        public double RepeatingInterval { get; set; } = 0;
        public string[] ExcludedComputers { get; set; }

        public string ExcludedComputersRegex { get; set; }

        public bool IsRepeating { get; set; }
        public bool IsArchived { get; set; }
        public bool IsRemote { get; set; }      //not used
        public bool NeedsNetwork { get; set; }
        public bool NeedsUser { get; set; }
        public bool NeedsSystem { get; set; }
        public string MachineName { get; set; }
        public string UserName { get; set; }
        public bool RunAlready { get; set; }

        public CommandSet() { }
        public CommandSet(string filePath) { MakeFromFile(filePath); }

        public void MakeFromFile(string filePath)
        {
            CommandLines = new List<string>();
            SourceFilePath = filePath;
            IsArchived = false;
            SourceFileName = Path.GetFileName(SourceFilePath);

            // Get last-write time defensively (file may be mid-write)
            try { FileLastModified = File.GetLastWriteTime(filePath); } catch { FileLastModified = DateTime.MinValue; }

            const string settingsHashCodeV1 = "@cmdsettings:";
            const string settingsHashCodeV2 = "@cmdsettingsV2";

            int lineNum = 0;
            int settingsVersion = 0;
            bool commandsLines = false;

            // Open with FileShare so we can read while another process is writing/locking
            // Also add a few retries in case the writer has an exclusive lock for a moment.
            const int maxAttempts = 5;
            const int delayMs = 150;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (var reader = new StreamReader(fs, Encoding.UTF8, true))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (string.IsNullOrEmpty(line)) { lineNum++; continue; }

                            if (lineNum == 0)
                            {
                                if (line.Contains(settingsHashCodeV1))
                                {
                                    var payload = (line.Length > settingsHashCodeV1.Length) ? line.Substring(settingsHashCodeV1.Length) : "";
                                    try
                                    {
                                        var settingStr = payload.Split(';');
                                        IsRepeating = false;
                                        IsRemote = false;
                                        NeedsNetwork = false;
                                        NeedsSystem = false;
                                        if (ExcludedComputers != null) Array.Clear(ExcludedComputers, 0, ExcludedComputers.Length);
                                        ExcludedComputersRegex = "";

                                        if (settingStr.Length > 0 && !string.IsNullOrEmpty(settingStr[0]))
                                        {
                                            if (settingStr[0] == "0")
                                            {
                                                IsRepeating = false;
                                            }
                                            else
                                            {
                                                IsRepeating = true;
                                                RepeatingType = settingStr[0].Substring(0, 1);
                                                double val;
                                                RepeatingInterval = (settingStr[0].Length > 1 && double.TryParse(settingStr[0].Substring(1),
                                                    System.Globalization.NumberStyles.Float,
                                                    System.Globalization.CultureInfo.InvariantCulture,
                                                    out val)) ? val : 0d;
                                            }
                                        }

                                        if (settingStr.Length > 1 && settingStr[1] == "1") IsRemote = true; // not used
                                        if (settingStr.Length > 2 && settingStr[2] == "1") NeedsNetwork = true;
                                        if (settingStr.Length > 3)
                                        {
                                            if (settingStr[3] == "1") NeedsUser = true;
                                            if (settingStr[3] == "-1") NeedsSystem = true;
                                        }
                                        if (settingStr.Length > 4 && !string.IsNullOrEmpty(settingStr[4]))
                                            ExcludedComputers = settingStr[4].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (settingStr.Length > 5)
                                            ExcludedComputersRegex = settingStr[5];
                                    }
                                    catch (Exception e)
                                    {
                                        throw new Exception("Failed to load command settings (V1)", e);
                                    }

                                    settingsVersion = 1;
                                }
                                else if (line.Contains(settingsHashCodeV2))
                                {
                                    settingsVersion = 2;
                                    if (ExcludedComputers != null) Array.Clear(ExcludedComputers, 0, ExcludedComputers.Length);
                                    ExcludedComputersRegex = "";
                                }
                            }
                            else
                            {
                                if (settingsVersion == 1)
                                {
                                    CommandLines.Add(line);
                                }
                                else if (settingsVersion == 2)
                                {
                                    try
                                    {
                                        if (!commandsLines)
                                        {
                                            var settingStr = line.Split(new[] { ":" }, 2, StringSplitOptions.None);
                                            var key = settingStr[0];
                                            var val = (settingStr.Length > 1) ? settingStr[1] : "";

                                            switch (key)
                                            {
                                                case "MachineName":
                                                    MachineName = val;
                                                    break;
                                                case "UserName":
                                                    UserName = val;
                                                    break;
                                                case "RunAt":
                                                    if (!string.IsNullOrWhiteSpace(val))
                                                    {
                                                        DateTime dt;
                                                        if (DateTime.TryParse(val, System.Globalization.CultureInfo.InvariantCulture,
                                                                               System.Globalization.DateTimeStyles.AssumeLocal, out dt))
                                                        {
                                                            RunAt = dt;
                                                            RunAtTime = true;
                                                        }
                                                    }
                                                    break;
                                                case "IsRepeating":
                                                    bool bRep;
                                                    if (bool.TryParse(val, out bRep)) IsRepeating = bRep;
                                                    break;
                                                case "IsRemote":
                                                    bool bRem;
                                                    if (bool.TryParse(val, out bRem)) IsRemote = bRem;
                                                    break;
                                                case "NeedsNetwork":
                                                    bool bNet;
                                                    if (bool.TryParse(val, out bNet)) NeedsNetwork = bNet;
                                                    break;
                                                case "NeedsSystem":
                                                    bool bSys;
                                                    if (bool.TryParse(val, out bSys)) NeedsSystem = bSys;
                                                    break;
                                                case "NeedsUser":
                                                    bool bUsr;
                                                    if (bool.TryParse(val, out bUsr)) NeedsUser = bUsr;
                                                    break;
                                                case "ExcludedComputers":
                                                    ExcludedComputers = string.IsNullOrEmpty(val)
                                                        ? null
                                                        : val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                                    break;
                                                case "ExcludedComputersRegex":
                                                    ExcludedComputersRegex = val ?? "";
                                                    break;
                                                case "RepeatingType":
                                                    RepeatingType = val ?? "";
                                                    break;
                                                case "RepeatingInterval":
                                                    double d;
                                                    RepeatingInterval = double.TryParse(val, System.Globalization.NumberStyles.Float,
                                                        System.Globalization.CultureInfo.InvariantCulture, out d) ? d : 0d;
                                                    break;
                                                case "commands":
                                                    commandsLines = true;
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            CommandLines.Add(line);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        throw new Exception("Failed to load command settings (V2)", e);
                                    }
                                }
                            }

                            lineNum++;
                        }
                    }

                    // If we got here, read succeeded
                    break;
                }
                catch (IOException) when (attempt < maxAttempts)
                {
                    System.Threading.Thread.Sleep(delayMs);
                    continue;
                }
                catch (UnauthorizedAccessException) when (attempt < maxAttempts)
                {
                    System.Threading.Thread.Sleep(delayMs);
                    continue;
                }
                catch
                {
                    // rethrow unexpected
                    throw;
                }
            }

            if (RunAtTime && IsRunEnvironment())
            {
                if (IsRepeating && RepeatingType == "d")
                {
                    if (RunAt < DateTime.Now) RunAt = (DateTime.Today + RunAt.TimeOfDay).AddDays(RepeatingInterval);
                    Logger.WriteLog($"run scheduled {SourceFilePath}: {RunAt.ToShortDateString()} {RunAt.ToShortTimeString()} ", Logger.TypeLog.both);
                }
            }
        }

        public bool CommandIsRunnable(string command)
        {
            command = command.Trim();
            command = command.Replace("\r", "");
            command = command.Replace("\n", "");
            string[] splitCmd = command.Split(' ');
            if (splitCmd.Length > 0 && string.Equals(splitCmd[0], "wscript", StringComparison.OrdinalIgnoreCase))
            {
                var scriptPath = (splitCmd.Length > 1) ? splitCmd[1].Trim() : "";
                if (!string.IsNullOrEmpty(scriptPath) && !File.Exists(scriptPath)) return false;
            }

            if (command == "") return false;
            if (command.Substring(0, 1) == ";") return false;
            return true;
        }
        public bool SpecialCommand(string command)
        {
            if (command.Length < 1) return false;
            if (command.Substring(0, 1) == "&")
            {
                string specialCommand = command.Substring(1);
                string cmdParams = "";

                if (specialCommand.IndexOf(":") > 0)
                {
                    cmdParams = specialCommand.Substring(specialCommand.IndexOf(":")).TrimStart(':');
                    specialCommand = specialCommand.Substring(0, specialCommand.IndexOf(":"));
                }
                ;

                if (specialCommand == "scr")
                {
                    Random rnd = new Random();
                    string outFile = Program.outPath + "scr-" + System.Environment.MachineName + "-" + rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999) + ".jpg";
                    MiscCommands.SaveScreenshot(outFile);
                }
                if (specialCommand == "msg")
                {
                    MiscCommands.ShowMessage(cmdParams, "message from admin");
                }
                if (specialCommand == "list")
                {
                    MiscCommands.ListCommands();
                }
                if (specialCommand == "clear")
                {
                    MiscCommands.ClearCommands();
                }
                if (specialCommand == "post")
                {
                    string[] cmdMultiParams;
                    cmdMultiParams = cmdParams.Split('|');
                    if (cmdMultiParams.Length == 2) MiscCommands.PostMessage(cmdMultiParams[0], cmdMultiParams[1]);
                }
                if (specialCommand == "writefile")
                {
                    try
                    {
                        // Expect key=value items separated by '|'
                        // Required: path, payload
                        // Optional: append (0/1), encoding (utf8|utf8bom|ascii|unicode)
                        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var part in cmdParams.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var kv = part.Split(new[] { '=' }, 2);
                            var key = kv[0].Trim();
                            var val = kv.Length > 1 ? kv[1] : "";
                            dict[key] = val;
                        }

                        string path = dict.ContainsKey("path") ? dict["path"] : dict.ContainsKey("target") ? dict["target"] : "";
                        string payloadB64 = dict.ContainsKey("payload") ? dict["payload"] : "";
                        bool append = dict.ContainsKey("append") && (dict["append"] == "1" || dict["append"].Equals("true", StringComparison.OrdinalIgnoreCase));
                        string encName = dict.ContainsKey("encoding") ? dict["encoding"] : "utf8";

                        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(payloadB64))
                        {
                            Logger.WriteLog("writefile: missing 'path' or 'payload'", Logger.TypeLog.both);
                            return true; // treat as handled to avoid spawning cmd.exe
                        }

                        // Choose encoding (default: UTF-8 without BOM)
                        System.Text.Encoding enc;
                        switch ((encName ?? "").ToLowerInvariant())
                        {
                            case "utf8bom": enc = new System.Text.UTF8Encoding(true); break;
                            case "ascii": enc = System.Text.Encoding.ASCII; break;
                            case "unicode": enc = System.Text.Encoding.Unicode; break; // UTF-16LE
                            default: enc = new System.Text.UTF8Encoding(false); break; // utf8
                        }

                        // Decode payload
                        string contents;
                        try
                        {
                            var bytes = Convert.FromBase64String(payloadB64);
                            contents = enc.GetString(bytes);
                        }
                        catch (Exception exB64)
                        {
                            Logger.WriteLog($"writefile: base64 decode failed: {exB64.Message}", Logger.TypeLog.both);
                            return true;
                        }

                        // Ensure directory exists
                        var dir = System.IO.Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                            System.IO.Directory.CreateDirectory(dir);

                        // Write or append
                        if (append)
                            System.IO.File.AppendAllText(path, contents, enc);
                        else
                            System.IO.File.WriteAllText(path, contents, enc);

                        Logger.WriteLog($"writefile: wrote {(append ? "append" : "overwrite")} {path}", Logger.TypeLog.both);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog($"writefile: exception {ex}", Logger.TypeLog.both);
                    }
                    return true; // handled
                }

                return true;
            }
            return false;
        }
        public bool IsRemoved(bool isOnline)
        {
            if ((isOnline && NeedsNetwork) || (!NeedsNetwork))
            {
                if (!File.Exists(SourceFilePath)) return true;
            }
            return false;
        }
        public bool IsRunEnvironment()
        {
            if ((System.Environment.UserName == "SYSTEM") && NeedsUser) return false;
            if ((System.Environment.UserName != "SYSTEM") && NeedsSystem) return false;

            // NEW: machine targeting (optional)
            if (!string.IsNullOrWhiteSpace(MachineName))
            {
                var me = System.Environment.MachineName;
                // allow "all" or exact match (case-insensitive)
                if (!MachineName.Equals("all", StringComparison.OrdinalIgnoreCase) &&
                    !MachineName.Equals(me, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            if (!string.IsNullOrEmpty(UserName))
            {
                if (UserName.ToLower() != Environment.UserName.ToLower()) return false;
            }

            string compName = System.Environment.MachineName;
            if (!string.IsNullOrEmpty(ExcludedComputersRegex))
            {
                var match = System.Text.RegularExpressions.Regex.Match(
                    compName, ExcludedComputersRegex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success) return false;
            }
            return true;
        }

        public bool Run(bool isOnline)
        {
            double ranDiff = 0;
            if (!isOnline && NeedsNetwork) return false;
            if (IsArchived) return false;
            if (!IsRunEnvironment()) return false;

            if (RunAtTime)
            {
                if (RunAt >= DateTime.Now) return false;

                var v3 = this as IV3Schedule;
                if (v3 == null)
                {
                    if (IsRepeating)
                    {
                        if (RepeatingType == "s") RunAt = DateTime.Now.AddSeconds(RepeatingInterval);
                        else if (RepeatingType == "m") RunAt = DateTime.Now.AddMinutes(RepeatingInterval);
                        else if (RepeatingType == "h") RunAt = DateTime.Now.AddHours(RepeatingInterval);
                        else if (RepeatingType == "d") RunAt = (DateTime.Today + RunAt.TimeOfDay).AddDays(RepeatingInterval);
                        else if (RepeatingType == "!") return false; // never
                        Logger.WriteLog($"next run {SourceFilePath}: {RunAt:G}", Logger.TypeLog.both);
                    }
                    else
                    {
                        if (RunAtRan) return false;
                        RunAtRan = true;
                    }
                }
                // V3 advance happens in CommandsManager after successful run
            }
            else
            {
                if (IsRepeating)
                {
                    if (RepeatingType == "s") ranDiff = (DateTime.Now - LastRun).TotalSeconds;
                    else if (RepeatingType == "m") ranDiff = (DateTime.Now - LastRun).TotalMinutes;
                    else if (RepeatingType == "h") ranDiff = (DateTime.Now - LastRun).TotalHours;
                    else if (RepeatingType == "d") ranDiff = (DateTime.Now - LastRun).TotalDays;
                    else if (RepeatingType == "x")
                    {
                        if (RunAlready) return false;
                        ranDiff = RepeatingInterval - 1;
                    }
                    else if (RepeatingType == "!") return false;

                    if (ranDiff < RepeatingInterval) return false;
                }
            }

            foreach (var raw in CommandLines ?? Enumerable.Empty<string>())
            {
                var command = (raw ?? "").Replace("\r", "").Replace("\n", "").Trim();
                if (command.Length == 0) continue;

                if (SpecialCommand(command))
                {
                    // handled internally
                }
                else if (CommandIsRunnable(command))
                {
                    try
                    {
                        System.Diagnostics.ProcessStartInfo psi;
                        string scriptPath, scriptArgs;

                        if (TryParseLeadingPs1Command(command, out scriptPath, out scriptArgs))
                        {
                            Logger.WriteLog(string.Format("Running PowerShell script: \"{0}\" {1}", scriptPath, scriptArgs), Logger.TypeLog.both);
                            psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = string.Format("-ExecutionPolicy Bypass -WindowStyle Hidden -File \"{0}\" {1}", scriptPath, scriptArgs),
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = false,
                                RedirectStandardError = false
                            };
                        }
                        else
                        {
                            psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                Arguments = "/C " + command,
                                UseShellExecute = false,
                                RedirectStandardOutput = true,
                                RedirectStandardError = false,
                                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                                CreateNoWindow = true
                            };
                        }

                        using (var proc = new System.Diagnostics.Process { StartInfo = psi })
                        {
                            proc.Start();
                            // Do not WaitForExit to avoid blocking the scheduler thread
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog("Process start failed for: " + command + " ex: " + ex, Logger.TypeLog.both);
                    }
                }
            }

            LastRun = DateTime.Now;
            return true;
        }

        public void ArchiveSource()
        {
            if (IsArchived) return;
            if (string.IsNullOrWhiteSpace(SourceFilePath)) return;

            try
            {
                if (!File.Exists(SourceFilePath)) { IsArchived = true; return; }

                string targetDir = Program.commandsArcLocalPath ?? "";
                try { Directory.CreateDirectory(targetDir); } catch { /* ignore */ }

                string dest = Path.Combine(targetDir, SourceFileName ?? ("arch_" + Guid.NewGuid().ToString("N")));
                try
                {
                    if (File.Exists(dest)) File.Delete(dest);
                    File.Move(SourceFilePath, dest);
                }
                catch
                {
                    // If move fails (locked), copy then best-effort delete
                    try { File.Copy(SourceFilePath, dest, true); } catch { }
                    try { File.Delete(SourceFilePath); } catch { }
                }
                IsArchived = true;
            }
            catch
            {
                // never throw from archiving
            }
        }
        public string[] ParseMultiSpacedArguments(string commandLine)
        {
            var isLastCharSpace = false;
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++)
            {
                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ' && !isLastCharSpace)
                    parmChars[index] = '\n';

                isLastCharSpace = parmChars[index] == '\n' || parmChars[index] == ' ';
            }

            return (new string(parmChars)).Split('\n');
        }

        private static bool TryParseLeadingPs1Command(string command, out string scriptPath, out string scriptArgs)
        {
            scriptPath = null;
            scriptArgs = "";

            if (string.IsNullOrWhiteSpace(command))
                return false;

            string trimmed = command.Trim();
            string firstToken;
            string rest;

            if (trimmed.StartsWith("\""))
            {
                int closingQuote = trimmed.IndexOf('"', 1);
                if (closingQuote <= 0)
                    return false;

                firstToken = trimmed.Substring(1, closingQuote - 1);
                rest = trimmed.Substring(closingQuote + 1).Trim();
            }
            else
            {
                int firstSpace = trimmed.IndexOf(' ');
                if (firstSpace < 0)
                {
                    firstToken = trimmed;
                    rest = "";
                }
                else
                {
                    firstToken = trimmed.Substring(0, firstSpace);
                    rest = trimmed.Substring(firstSpace + 1).Trim();
                }
            }

            if (!firstToken.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                return false;

            scriptPath = firstToken;
            scriptArgs = rest;
            return true;
        }
    }
}
