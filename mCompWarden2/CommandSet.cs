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

namespace mCompWarden2 {
    public class CommandSet {
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

        public CommandSet() {

        }
        public CommandSet(string filePath) {
            MakeFromFile(filePath);
        }


        public void MakeFromFile(string filePath) {
            string line;
            CommandLines = new List<string>();
            SourceFilePath = filePath;
            IsArchived = false;
            SourceFileName = Path.GetFileName(SourceFilePath);
            FileLastModified = System.IO.File.GetLastWriteTime(filePath);
            string settingsHashCodeV1 = "@cmdsettings:";
            string settingsHashCodeV2 = "@cmdsettingsV2";
            int lineNum = 0;
            int settingsVersion = 0;
            bool commandsLines = false;
            using (System.IO.StreamReader reader = new System.IO.StreamReader(filePath)) {
                while ((line = reader.ReadLine()) != null) {
                    if (line == "") continue;
                    if (lineNum == 0) {
                        if (line.Contains(settingsHashCodeV1)) {
                            line = (line.Length > settingsHashCodeV1.Length) ? line.Substring(settingsHashCodeV1.Length) : "";

                            try {
                                string[] settingStr = line.Split(';');
                                IsRepeating = false;
                                IsRemote = false;
                                NeedsNetwork = false;
                                NeedsSystem = false;
                                if (ExcludedComputers != null) Array.Clear(ExcludedComputers, 0, ExcludedComputers.Length);
                                ExcludedComputersRegex = "";

                                if (settingStr[0] != "") {
                                    if (settingStr[0] == "0") {
                                        IsRepeating = false;
                                    }
                                    else {
                                        IsRepeating = true;
                                        RepeatingType = settingStr[0].Substring(0, 1);
                                        RepeatingInterval = (settingStr[0].Substring(1) == "") ? 0 : double.Parse(settingStr[0].Substring(1));
                                    }
                                }
                                if (settingStr[1] == "1") IsRemote = true;  //not used
                                if (settingStr[2] == "1") NeedsNetwork = true;
                                if (settingStr[3] == "1") NeedsUser = true;
                                if (settingStr[3] == "-1") NeedsSystem = true;
                                ExcludedComputers = settingStr[4].Split(',');
                                ExcludedComputersRegex = settingStr[5];
                            }
                            catch (Exception e) {
                                throw new Exception("Failed to load command settings", e);
                            }

                            settingsVersion = 1;
                        }
                        if (line.Contains(settingsHashCodeV2)) {
                            settingsVersion = 2;
                            if (ExcludedComputers != null) Array.Clear(ExcludedComputers, 0, ExcludedComputers.Length);
                            ExcludedComputersRegex = "";
                        }
                    }
                    else {
                        if (settingsVersion == 1) {
                            CommandLines.Add(line);
                        }
                        if (settingsVersion == 2) {
                            try {
                                if (!commandsLines) {
                                    string[] settingStr = line.Split(new string[] { ":" }, 2, StringSplitOptions.None);
                                    switch (settingStr[0]) {
                                        case "MachineName":
                                            MachineName = settingStr[1];
                                            break;
                                        case "UserName":
                                            UserName = settingStr[1];
                                            break;
                                        case "RunAt":
                                            if (settingStr[1] != "") {
                                                RunAt = DateTime.Parse(settingStr[1]);
                                                RunAtTime = true;
                                            }
                                            break;
                                        case "IsRepeating":
                                            IsRepeating = bool.Parse(settingStr[1]);
                                            break;
                                        case "IsRemote":
                                            IsRemote = bool.Parse(settingStr[1]);
                                            break;
                                        case "NeedsNetwork":
                                            NeedsNetwork = bool.Parse(settingStr[1]);
                                            break;
                                        case "NeedsSystem":
                                            NeedsSystem = bool.Parse(settingStr[1]);
                                            break;
                                        case "NeedsUser":
                                            NeedsUser = bool.Parse(settingStr[1]);
                                            break;
                                        case "ExcludedComputers":

                                            ExcludedComputers = settingStr[1].Split(',');
                                            break;
                                        case "ExcludedComputersRegex":
                                            ExcludedComputersRegex = settingStr[1];
                                            break;
                                        case "RepeatingType":
                                            RepeatingType = settingStr[1];
                                            break;
                                        case "RepeatingInterval":
                                            try {
                                                RepeatingInterval = double.Parse(settingStr[1]);
                                            } catch {
                                                RepeatingInterval = 0;
                                            }
                                            break;
                                        case "commands":
                                            commandsLines = true;
                                            break;
                                    }
                                }
                                else {
                                    CommandLines.Add(line);
                                }
                            }
                            catch (Exception e) {
                                throw new Exception("Failed to load command settings", e);
                            }
                        }

                    }
                    lineNum++;
                }
            }
            if (RunAtTime && IsRunEnvironment()) {
                if (IsRepeating && RepeatingType == "d") {

                    if (RunAt < DateTime.Now) RunAt = (DateTime.Today + RunAt.TimeOfDay).AddDays(RepeatingInterval);
                    Logger.WriteLog($"run scheduled {SourceFilePath}: {RunAt.ToShortDateString()} {RunAt.ToShortTimeString()} ", Logger.TypeLog.both);
                }
            }
        }



        public bool CommandIsRunnable(string command) {
            command = command.Trim();
            command = command.Replace("\r", "");
            command = command.Replace("\n", "");
            string[] splitCmd = command.Split(' ');
            if (splitCmd.Length > 1) {
                if (splitCmd[0]=="wscript") {
                    string scriptPath = splitCmd[1];
                    if (!File.Exists(scriptPath)) return false;
                }
            }

            if (command == "") return false;
            if (command.Substring(0, 1) == ";") return false;
            return true;
        }
        public bool SpecialCommand(string command) {
            if (command.Length < 1) return false;
            if (command.Substring(0, 1) == "&") {
                string specialCommand = command.Substring(1);
                string cmdParams = "";

                if (specialCommand.IndexOf(":") > 0) {
                    cmdParams = specialCommand.Substring(specialCommand.IndexOf(":")).TrimStart(':');
                    specialCommand = specialCommand.Substring(0, specialCommand.IndexOf(":"));
                };

                if (specialCommand == "scr") {
                    Random rnd = new Random();
                    string outFile = Program.outPath + "scr-" + System.Environment.MachineName + "-" + rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999) + ".jpg";
                    MiscCommands.SaveScreenshot(outFile);
                }
                if (specialCommand == "msg") {
                    MiscCommands.ShowMessage(cmdParams, "message from admin");
                }
                if (specialCommand == "list") {
                    MiscCommands.ListCommands();
                }
                if (specialCommand == "clear") {
                    MiscCommands.ClearCommands();
                }
                if (specialCommand == "post") {
                    string[] cmdMultiParams;
                    cmdMultiParams = cmdParams.Split('|');
                    if (cmdMultiParams.Length == 2) MiscCommands.PostMessage(cmdMultiParams[0], cmdMultiParams[1]);
                }
                return true;
            }
            return false;
        }
        public bool IsRemoved(bool isOnline) {
            if ((isOnline && NeedsNetwork) || (!NeedsNetwork)) {
                if (!File.Exists(SourceFilePath)) return true;
            }
            return false;
        }
        public bool IsRunEnvironment() {
            if ((System.Environment.UserName == "SYSTEM") && NeedsUser) return false;
            if ((System.Environment.UserName != "SYSTEM") && NeedsSystem) return false;
            if (!string.IsNullOrEmpty(UserName)) {
                if (UserName.ToLower() != Environment.UserName.ToLower()) return false;
            }
            string compName = System.Environment.MachineName;
            if (ExcludedComputersRegex != null && ExcludedComputersRegex != "") {
                var match = System.Text.RegularExpressions.Regex.Match(compName, ExcludedComputersRegex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success) return false;
            }

            return true;
        }
        public bool Run(bool isOnline) {
            double ranDiff = 0;
            if (!isOnline && NeedsNetwork) return false;
            if (IsArchived) return false;
            if (!IsRunEnvironment()) return false;

            if (RunAtTime) {
                if (RunAt < DateTime.Now) {
                    if (IsRepeating) {
                        if (RepeatingType == "s") RunAt = DateTime.Now.AddSeconds(RepeatingInterval);
                        if (RepeatingType == "m") RunAt = DateTime.Now.AddMinutes(RepeatingInterval);
                        if (RepeatingType == "h") RunAt = DateTime.Now.AddHours(RepeatingInterval);
                        if (RepeatingType == "d") {
                            RunAt = (DateTime.Today + RunAt.TimeOfDay).AddDays(RepeatingInterval);
                            //RunAt = DateTime.Now.AddDays(RepeatingInterval);
                        }
                        if (RepeatingType == "!") return false; //! means never! 
                        Logger.WriteLog($"next run {SourceFilePath}: {RunAt.ToShortDateString()} {RunAt.ToShortTimeString()} ", Logger.TypeLog.both);
                    }
                    else {
                        //nema se opakovat, spusti se jen jednou
                        if (RunAtRan) return false;
                        RunAtRan = true;
                    }
                }
                else {
                    //jeste neprisel datum a cas
                    return false;
                }
            }
            else {
                //opakujeme v intervalu od te doby, kdy se to pustilo
                if (IsRepeating) {
                    if (RepeatingType == "s") ranDiff = (DateTime.Now - LastRun).TotalSeconds;
                    if (RepeatingType == "m") ranDiff = (DateTime.Now - LastRun).TotalMinutes;
                    if (RepeatingType == "h") ranDiff = (DateTime.Now - LastRun).TotalHours;
                    if (RepeatingType == "d") ranDiff = (DateTime.Now - LastRun).TotalDays;
                    if (RepeatingType == "!") return false; //! means never!
                    if (ranDiff < RepeatingInterval) return false;
                }
            }


            //string[] cmd = ParseMultiSpacedArguments(command);
            foreach (string command in CommandLines) {
                if (SpecialCommand(command)) {
                    ;
                }
                else if (CommandIsRunnable(command)) {
                    var proc = new System.Diagnostics.Process {
                        StartInfo = new System.Diagnostics.ProcessStartInfo {
                            FileName = "cmd.exe",
                            Arguments = "/C " + command,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                            CreateNoWindow = true
                            //WorkingDirectory = @"C:\MyAndroidApp\"
                        }
                    };
                    proc.Start();
                }

            }
            LastRun = DateTime.Now;
            return true;
        }

        public void ArchiveSource() {
            if (IsArchived) return;
            if (!File.Exists(SourceFilePath)) return;

            string arcFile = Program.commandsArcLocalPath + SourceFileName;
            if (File.Exists(arcFile)) File.Delete(arcFile);
            File.Move(SourceFilePath, arcFile);
            IsArchived = true;
        }
        public string[] ParseMultiSpacedArguments(string commandLine) {
            var isLastCharSpace = false;
            char[] parmChars = commandLine.ToCharArray();
            bool inQuote = false;
            for (int index = 0; index < parmChars.Length; index++) {
                if (parmChars[index] == '"')
                    inQuote = !inQuote;
                if (!inQuote && parmChars[index] == ' ' && !isLastCharSpace)
                    parmChars[index] = '\n';

                isLastCharSpace = parmChars[index] == '\n' || parmChars[index] == ' ';
            }

            return (new string(parmChars)).Split('\n');
        }
    }
}
