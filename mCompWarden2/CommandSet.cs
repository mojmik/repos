using System;
using System.IO;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Policy;

namespace mCompWarden2 {
    public class CommandSet {
        public List<string> CommandLines { get; set; } = new List<string>();
        public DateTime LastRun { get; set; }
        public string SourceFilePath { get; set; }
        public string SourceFileName { get; set; }
        public string RepeatingType { get; set; }
        public double RepeatingInterval { get; set; }
        public string[] ExcludedComputers { get; set; }
        
        public string ExcludedComputersRegex { get; set; }

        public bool IsRepeating { get; set; }
        public bool IsArchived { get; set; }
        public bool IsRemote { get; set; }
        public bool NeedsNetwork { get; set; }
        public bool NeedsUser { get; set; }
        public CommandSet() {

        }
        public CommandSet(string filePath) {
            string line;
            SourceFilePath = filePath;
            SourceFileName = Path.GetFileName(SourceFilePath);
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
                            RepeatingInterval = double.Parse(settingStr[0].Substring(1));
                        }
                        if (settingStr[1] == "1") IsRemote = true;
                        if (settingStr[2] == "1") NeedsNetwork = true;
                        if (settingStr[3] == "1") NeedsUser = true;
                        ExcludedComputers = settingStr[4].Split(',');
                        ExcludedComputersRegex = settingStr[5];
                    }
                    catch {
                    }
                }
                return true;
            }
            else return false;
        }
        public bool Run(bool isOnline) {
            double ranDiff=0;
            if (!isOnline && NeedsNetwork) return false;
            if (IsArchived) return false;
            if ((System.Environment.UserName == "SYSTEM") && NeedsUser) return false;
            string compName = System.Environment.MachineName;
            if (ExcludedComputersRegex != null) {
                var match = System.Text.RegularExpressions.Regex.Match(compName, ExcludedComputersRegex, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success) return false;
            }

            if (IsRepeating) {
                if (RepeatingType == "s") ranDiff = (DateTime.Now - LastRun).TotalSeconds;
                if (RepeatingType == "m") ranDiff = (DateTime.Now - LastRun).TotalMinutes;
                if (RepeatingType == "h") ranDiff = (DateTime.Now - LastRun).TotalHours;
                if (RepeatingType == "d") ranDiff = (DateTime.Now - LastRun).TotalDays;
                if (ranDiff < RepeatingInterval) return false;
            }
            //string[] cmd = ParseMultiSpacedArguments(command);
            foreach (string command in CommandLines) {
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
