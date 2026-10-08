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
using System.Runtime.InteropServices;

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
        public bool HasPasswordEnc { get; set; }

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
                if (specialCommand == "post" || specialCommand == "postmessage")
                {
                    string[] cmdMultiParams = cmdParams.Split(new[] { '|' }, 2);
                    if (cmdMultiParams.Length == 2)
                    {
                        MiscCommands.PostMessage(cmdMultiParams[0].Trim(), cmdMultiParams[1]);
                    }
                    else if (cmdMultiParams.Length == 1 && !string.IsNullOrWhiteSpace(cmdMultiParams[0]))
                    {
                        MiscCommands.PostMessage("info", cmdMultiParams[0]);
                    }
                }
                if (specialCommand == "writefile")
                {
                    try
                    {
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
                            return true;
                        }

                        System.Text.Encoding enc;
                        switch ((encName ?? "").ToLowerInvariant())
                        {
                            case "utf8bom": enc = new System.Text.UTF8Encoding(true); break;
                            case "ascii": enc = System.Text.Encoding.ASCII; break;
                            case "unicode": enc = System.Text.Encoding.Unicode; break;
                            default: enc = new System.Text.UTF8Encoding(false); break;
                        }

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

                        var dir = System.IO.Path.GetDirectoryName(path);
                        if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                            System.IO.Directory.CreateDirectory(dir);

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
                    return true;
                }
                if (specialCommand == "runprogram")
                {
                    try
                    {
                        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var part in cmdParams.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var kv = part.Split(new[] { '=' }, 2);
                            var key = kv[0].Trim();
                            var val = kv.Length > 1 ? kv[1] : "";
                            dict[key] = val;
                        }

                        string DecodeB64(string val)
                        {
                            if (string.IsNullOrWhiteSpace(val)) return "";
                            try
                            {
                                var bytes = Convert.FromBase64String(val);
                                return System.Text.Encoding.UTF8.GetString(bytes);
                            }
                            catch
                            {
                                return val;
                            }
                        }

                        string file = dict.ContainsKey("file") ? DecodeB64(dict["file"]) : "";
                        string args = dict.ContainsKey("args") ? DecodeB64(dict["args"]) : "";
                        string workDir = dict.ContainsKey("workdir") ? DecodeB64(dict["workdir"]) : "";
                        bool singleInstance = dict.ContainsKey("singleinstance") && (dict["singleinstance"] == "1" || dict["singleinstance"].Equals("true", StringComparison.OrdinalIgnoreCase));
                        string procName = dict.ContainsKey("procname") ? DecodeB64(dict["procname"]) : "";
                        string domain = dict.ContainsKey("domain") ? DecodeB64(dict["domain"]) : "";
                        string passEnc = dict.ContainsKey("passenc") ? DecodeB64(dict["passenc"]) : "";

                        if (string.IsNullOrWhiteSpace(file))
                        {
                            Logger.WriteLog("runprogram: missing 'file'", Logger.TypeLog.both);
                            return true;
                        }

                        if (singleInstance)
                        {
                            string targetProcName = procName;
                            if (string.IsNullOrWhiteSpace(targetProcName))
                            {
                                string candidate = System.IO.Path.GetFileNameWithoutExtension(file);
                                if (!string.Equals(candidate, "cmd", StringComparison.OrdinalIgnoreCase) &&
                                    !string.Equals(candidate, "powershell", StringComparison.OrdinalIgnoreCase) &&
                                    !string.Equals(candidate, "pwsh", StringComparison.OrdinalIgnoreCase))
                                {
                                    targetProcName = candidate;
                                }
                            }

                            if (!string.IsNullOrWhiteSpace(targetProcName))
                            {
                                try
                                {
                                    var existingProcs = System.Diagnostics.Process.GetProcessesByName(targetProcName);
                                    if (existingProcs != null && existingProcs.Length > 0)
                                    {
                                        Logger.WriteLog($"runprogram: singleInstance check - process '{targetProcName}' is already running ({existingProcs.Length} instance(s)). Skipping execution of '{file}'.", Logger.TypeLog.both);
                                        return true;
                                    }
                                }
                                catch (Exception exProc)
                                {
                                    Logger.WriteLog($"runprogram: process check failed for '{targetProcName}': {exProc.Message}", Logger.TypeLog.both);
                                }
                            }
                        }

                        System.Diagnostics.ProcessStartInfo psi;
                        string scriptPath, scriptArgs;

                        string fullCmdLine = (file.Contains(" ") ? $"\"{file}\"" : file) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args);

                        if (TryParseLeadingPs1Command(fullCmdLine, out scriptPath, out scriptArgs) || file.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                        {
                            string psScript = string.IsNullOrEmpty(scriptPath) ? file : scriptPath;
                            string psArgs = string.IsNullOrEmpty(scriptPath) ? args : scriptArgs;

                            Logger.WriteLog($"Running PowerShell script: \"{psScript}\" {psArgs}" + (!string.IsNullOrWhiteSpace(workDir) ? $" (WorkDir: {workDir})" : ""), Logger.TypeLog.both);
                            psi = new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = $"-ExecutionPolicy Bypass -WindowStyle Hidden -File \"{psScript}\" {psArgs}",
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                RedirectStandardOutput = false,
                                RedirectStandardError = false
                            };
                        }
                        else
                        {
                            Logger.WriteLog($"Running program: \"{file}\" {args}" + (!string.IsNullOrWhiteSpace(workDir) ? $" (WorkDir: {workDir})" : ""), Logger.TypeLog.both);

                            bool isVbs = file.EndsWith(".vbs", StringComparison.OrdinalIgnoreCase);
                            bool isBat = file.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase);

                            if (isVbs)
                            {
                                psi = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "wscript.exe",
                                    Arguments = $"//nologo \"{file}\"" + (string.IsNullOrWhiteSpace(args) ? "" : " " + args),
                                    UseShellExecute = true,
                                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                                };
                            }
                            else if (isBat && string.IsNullOrWhiteSpace(workDir))
                            {
                                psi = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = "cmd.exe",
                                    Arguments = $"/c \"\"{file}\"" + (string.IsNullOrWhiteSpace(args) ? "" : " " + args) + "\"",
                                    UseShellExecute = true,
                                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                                };
                            }
                            else if (string.IsNullOrWhiteSpace(workDir))
                            {
                                psi = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = file,
                                    Arguments = args ?? "",
                                    UseShellExecute = true,
                                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                                };
                            }
                            else
                            {
                                psi = new System.Diagnostics.ProcessStartInfo
                                {
                                    FileName = file,
                                    Arguments = args ?? "",
                                    UseShellExecute = false,
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = false,
                                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                                    CreateNoWindow = true
                                };
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(workDir))
                        {
                            psi.WorkingDirectory = workDir;
                        }

                        // Encrypted Password & Credentials (supports AES & DPAPI)
                        if (!string.IsNullOrWhiteSpace(passEnc))
                        {
                            string plainPassword = DpapiHelper.Decrypt(passEnc);
                            if (!string.IsNullOrWhiteSpace(plainPassword))
                            {
                                Logger.WriteLog($"runprogram: launching via CreateProcessWithTokenW for user {(string.IsNullOrWhiteSpace(domain) ? "" : domain + "\\")}{UserName}", Logger.TypeLog.both);
                                UserProcessLauncher.Launch(domain, UserName, plainPassword, file, args, workDir);
                                Logger.WriteLog($"runprogram: successfully started {file} as user {(string.IsNullOrWhiteSpace(domain) ? "" : domain + "\\")}{UserName}", Logger.TypeLog.both);
                            }
                            else
                            {
                                Logger.WriteLog($"runprogram ERROR: passwordEnc decryption failed for user {(string.IsNullOrWhiteSpace(domain) ? "" : domain + "\\")}{UserName}. Skipping execution.", Logger.TypeLog.both);
                            }
                        }
                        else
                        {
                            using (var proc = new System.Diagnostics.Process { StartInfo = psi })
                            {
                                proc.Start();
                            }
                            Logger.WriteLog($"runprogram: started {file}" + (!string.IsNullOrWhiteSpace(workDir) ? $" in {workDir}" : ""), Logger.TypeLog.both);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLog($"runprogram: exception {ex}", Logger.TypeLog.both);
                    }
                    return true;
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
            bool isSystemInstance = string.Equals(System.Environment.UserName, "SYSTEM", StringComparison.OrdinalIgnoreCase);

            if (HasPasswordEnc)
            {
                // Tasks with encrypted passwords must be dispatched by the elevated SYSTEM service instance
                if (!isSystemInstance) return false;
            }
            else
            {
                if (isSystemInstance && NeedsUser) return false;
                if (!isSystemInstance && NeedsSystem) return false;

                if (!string.IsNullOrEmpty(UserName))
                {
                    if (!string.Equals(UserName, Environment.UserName, StringComparison.OrdinalIgnoreCase)) return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(MachineName))
            {
                var me = System.Environment.MachineName;
                if (!MachineName.Equals("all", StringComparison.OrdinalIgnoreCase) &&
                    !MachineName.Equals(me, StringComparison.OrdinalIgnoreCase))
                    return false;
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

    public static class UserProcessLauncher
    {
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessAsUser(
            IntPtr hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcessWithTokenW(
            IntPtr hToken,
            uint dwLogonFlags,
            string lpApplicationName,
            string lpCommandLine,
            uint dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_LOGON_BATCH = 4;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const uint LOGON_WITH_PROFILE = 0x00000001;
        private const uint CREATE_NO_WINDOW = 0x08000000;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct NETRESOURCE
        {
            public uint dwScope;
            public uint dwType;
            public uint dwDisplayType;
            public uint dwUsage;
            public string lpLocalName;
            public string lpRemoteName;
            public string lpComment;
            public string lpProvider;
        }

        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        private static extern int WNetAddConnection2(ref NETRESOURCE lpNetResource, string lpPassword, string lpUsername, uint dwFlags);

        private static void AuthenticateUncShare(string uncPath, string domain, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(uncPath) || !uncPath.StartsWith(@"\\")) return;

            try
            {
                string[] parts = uncPath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) return;

                string uncShare = @"\\" + parts[0] + @"\" + parts[1];
                string fullUser = string.IsNullOrWhiteSpace(domain) ? username : domain + @"\" + username;

                var nr = new NETRESOURCE
                {
                    dwType = 1, // RESOURCETYPE_DISK
                    lpRemoteName = uncShare
                };

                int res = WNetAddConnection2(ref nr, password, fullUser, 0);
                if (res != 0 && res != 1219) // 1219 = ERROR_SESSION_CREDENTIAL_CONFLICT
                {
                    Logger.WriteLog($"WNetAddConnection2 returned {res} for {uncShare} user {fullUser}", Logger.TypeLog.both);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog($"AuthenticateUncShare exception: {ex.Message}", Logger.TypeLog.both);
            }
        }

        private static string MapUncWorkingDirToDrive(string uncWorkDir, string domain, string username, string password, out string mappedLetter)
        {
            mappedLetter = null;
            if (string.IsNullOrWhiteSpace(uncWorkDir) || !uncWorkDir.StartsWith(@"\\")) return uncWorkDir;

            string fullUser = string.IsNullOrWhiteSpace(domain) ? username : domain + @"\" + username;

            for (char c = 'Z'; c >= 'F'; c--)
            {
                string letter = c + ":";
                if (!Directory.Exists(letter))
                {
                    var nr = new NETRESOURCE
                    {
                        dwType = 1, // RESOURCETYPE_DISK
                        lpLocalName = letter,
                        lpRemoteName = uncWorkDir
                    };

                    int res = WNetAddConnection2(ref nr, password, fullUser, 0);
                    if (res == 0 || res == 1219)
                    {
                        mappedLetter = letter;
                        Logger.WriteLog($"UserProcessLauncher: Mapped UNC workDir {uncWorkDir} -> {letter}\\", Logger.TypeLog.both);
                        return letter + @"\";
                    }
                }
            }

            return uncWorkDir;
        }

        public static void Launch(string domain, string username, string password, string file, string args, string workDir)
        {
            string targetDomain = string.IsNullOrWhiteSpace(domain) ? "." : domain;
            IntPtr hToken = IntPtr.Zero;

            // LOGON32_LOGON_BATCH (4) first so process executes locally under target user identity (srv)
            int[] logonTypes = new int[]
            {
                LOGON32_LOGON_BATCH,
                LOGON32_LOGON_SERVICE,
                LOGON32_LOGON_INTERACTIVE,
                LOGON32_LOGON_NETWORK_CLEARTEXT,
                LOGON32_LOGON_NEW_CREDENTIALS
            };

            int lastError = 0;
            bool loggedOn = false;

            foreach (int lt in logonTypes)
            {
                loggedOn = LogonUser(username, targetDomain, password, lt, LOGON32_PROVIDER_DEFAULT, out hToken);
                if (loggedOn) break;
                lastError = Marshal.GetLastWin32Error();
            }

            if (!loggedOn)
            {
                throw new System.ComponentModel.Win32Exception(lastError, $"LogonUser failed for {targetDomain}\\{username} (Win32 Error {lastError})");
            }

            try
            {
                // Authenticate UNC share inside impersonated user token context
                if (ImpersonateLoggedOnUser(hToken))
                {
                    AuthenticateUncShare(file, domain, username, password);
                    if (!string.IsNullOrWhiteSpace(workDir))
                        AuthenticateUncShare(workDir, domain, username, password);
                    RevertToSelf();
                }

                var si = new STARTUPINFO();
                si.cb = Marshal.SizeOf(si);

                bool isBatchScript = file.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase);
                bool isCmdExe = file.Equals("cmd.exe", StringComparison.OrdinalIgnoreCase) || file.EndsWith(@"\cmd.exe", StringComparison.OrdinalIgnoreCase);

                string cmdLine;
                string cwd = null;

                if (!string.IsNullOrWhiteSpace(workDir) && workDir.StartsWith(@"\\"))
                {
                    string targetCall;
                    if (isCmdExe)
                    {
                        targetCall = args;
                    }
                    else
                    {
                        string relFile = file;
                        if (!string.IsNullOrWhiteSpace(workDir) && file.StartsWith(workDir, StringComparison.OrdinalIgnoreCase))
                        {
                            relFile = file.Substring(workDir.Length).TrimStart('\\');
                        }
                        if (!relFile.Contains("\\") && !relFile.Contains(":") && !relFile.StartsWith("."))
                        {
                            relFile = @".\" + relFile;
                        }
                        targetCall = (relFile.Contains(" ") ? $"\"{relFile}\"" : relFile) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args);
                    }

                    cmdLine = $"cmd.exe /c \"pushd \"{workDir}\" && {targetCall}\"";
                    cwd = null;
                }
                else if (isBatchScript)
                {
                    string batchCall = (file.Contains(" ") ? $"\"{file}\"" : file) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args);
                    cmdLine = $"cmd.exe /c \"{batchCall}\"";
                    cwd = string.IsNullOrWhiteSpace(workDir) ? null : workDir;
                }
                else
                {
                    if (isCmdExe)
                    {
                        cmdLine = $"cmd.exe {args}";
                    }
                    else
                    {
                        cmdLine = (file.Contains(" ") ? $"\"{file}\"" : file) + (string.IsNullOrWhiteSpace(args) ? "" : " " + args);
                    }
                    cwd = string.IsNullOrWhiteSpace(workDir) ? null : workDir;
                }

                Logger.WriteLog($"UserProcessLauncher: launching cmdLine '{cmdLine}' (cwd: {(cwd ?? "null")}) for user {targetDomain}\\{username}", Logger.TypeLog.both);

                PROCESS_INFORMATION pi;
                bool created = CreateProcessAsUser(hToken, null, cmdLine, IntPtr.Zero, IntPtr.Zero, false, CREATE_NO_WINDOW, IntPtr.Zero, cwd, ref si, out pi);

                if (!created)
                {
                    created = CreateProcessWithTokenW(hToken, LOGON_WITH_PROFILE, null, cmdLine, CREATE_NO_WINDOW, IntPtr.Zero, cwd, ref si, out pi);
                }

                if (!created)
                {
                    created = CreateProcessWithTokenW(hToken, 0, null, cmdLine, CREATE_NO_WINDOW, IntPtr.Zero, cwd, ref si, out pi);
                }

                if (!created)
                {
                    int err = Marshal.GetLastWin32Error();
                    throw new System.ComponentModel.Win32Exception(err, $"CreateProcessAsUser/WithTokenW failed for '{cmdLine}' (Win32 Error {err})");
                }

                CloseHandle(pi.hProcess);
                CloseHandle(pi.hThread);
            }
            finally
            {
                if (hToken != IntPtr.Zero) CloseHandle(hToken);
            }
        }
    }
}
