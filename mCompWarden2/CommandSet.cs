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
            using (System.IO.StreamReader reader = new System.IO.StreamReader(filePath)) {
                while ((line = reader.ReadLine()) != null) {
                    if (!UpdateSettings(line)) CommandLines.Add(line);
                }
            }
        }


        public bool UpdateSettings(string settings) {
            string settingsHashCode = "@cmdsettings:";
            if (settings.Contains(settingsHashCode)) {
                settings = (settings.Length > settingsHashCode.Length) ? settings.Substring(settingsHashCode.Length) : "";
                if (settings != "") {
                    try {
                        string[] settingStr = settings.Split(';');
                        if (settingStr[0] != "") {
                            IsRepeating = true;
                            RepeatingType = settingStr[0].Substring(0, 1);                            
                            RepeatingInterval = (settingStr[0].Substring(1) == "") ? 0 : double.Parse(settingStr[0].Substring(1));
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
                }
                return true;
            }
            else return false;
        }
        public bool CommandIsRunnable(string command) {
            command = command.Trim();
            command = command.Replace("\r", "");
            command = command.Replace("\n", "");
            if (command == "") return false;
            if (command.Substring(0, 1) == ";") return false;
            return true;
        }
        public bool SpecialCommand(string command) {
            if (command.Length < 1) return false;
            if (command.Substring(0, 1) == "&") {
                string specialCommand = command.Substring(1);
                string cmdParams="";

                if (specialCommand.IndexOf(":") > 0) {
                    cmdParams = specialCommand.Substring(specialCommand.IndexOf(":")).TrimStart(':');
                    specialCommand = specialCommand.Substring(0,specialCommand.IndexOf(":"));                    
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
                    if (cmdMultiParams.Length==2) MiscCommands.PostMessage(cmdMultiParams[0],cmdMultiParams[1]);
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
        public bool Run(bool isOnline) {
            double ranDiff = 0;
            if (!isOnline && NeedsNetwork) return false;
            if (IsArchived) return false;
            if ((System.Environment.UserName == "SYSTEM") && NeedsUser) return false;
            if ((System.Environment.UserName != "SYSTEM") && NeedsSystem) return false;

            string compName = System.Environment.MachineName;
            if (ExcludedComputersRegex != null && ExcludedComputersRegex != "") {
                var match = System.Text.RegularExpressions.Regex.Match(compName, ExcludedComputersRegex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success) return false;
            }

            if (IsRepeating) {
                if (RepeatingType == "s") ranDiff = (DateTime.Now - LastRun).TotalSeconds;
                if (RepeatingType == "m") ranDiff = (DateTime.Now - LastRun).TotalMinutes;
                if (RepeatingType == "h") ranDiff = (DateTime.Now - LastRun).TotalHours;
                if (RepeatingType == "d") ranDiff = (DateTime.Now - LastRun).TotalDays;
                if (RepeatingType == "!") return false; //! means never!
                if (ranDiff < RepeatingInterval) return false;
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
