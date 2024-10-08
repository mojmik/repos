using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;

namespace mCompWarden2guiCreator {
    class CommandFile {
        public List<string> CommandLines { get; set; } = new List<string>();
        public DateTime LastRun { get; set; }
        public string RunAt { get; set; }
        public string SourceFilePath { get; set; }
        public string SourceFileName { get; set; }
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

        public CommandFile() {

        }
        public CommandFile SetRepeating(double rep, string repType) {
            IsRepeating = true;
            RepeatingInterval = rep;
            RepeatingType = repType;
            return this;
        }
        public CommandFile SetRemote(bool remote) {
            IsRemote = remote;            
            return this;
        }
        public CommandFile SetNetwork(bool network) {
            NeedsNetwork = network;
            return this;
        }
        public CommandFile SetRunAt(string inStr) {
            RunAt = inStr;
            return this;
        }
        public CommandFile SetMachineName(string inStr) {
            MachineName = inStr;
            return this;
        }
        public CommandFile SetUserName(string inStr) {
            UserName = inStr;
            return this;
        }
        public CommandFile SetUser(bool network) {
            NeedsUser = network;
            return this;
        }
        public CommandFile SetSystem(bool network) {
            NeedsSystem = network;
            return this;
        }
        public CommandFile SetExcluded(string str) {
            ExcludedComputers = str.Split(',');
            return this;
        }
        public CommandFile SetExcludedRegex(string str) {
            ExcludedComputersRegex = str;
            return this;
        }
        public CommandFile SetCommandLines(List <string> lines)  {
            CommandLines = new List<string>();
            foreach (string line in lines) {
                line.Replace("\r", "");
                line.Replace("\n", "");
                CommandLines.Add(line + "\n");
            }            
            return this;
        }
        public string boolToStr(bool b, bool c=false) {
            if (c == true) return "-1";
            if (b == true) return "1";
            return "0";
        }
        public bool Save(string filePath) {            
            if (filePath == "") {
                return false;
            }

            if (System.IO.File.Exists(filePath)) {
                System.IO.File.Delete(filePath);
            }

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filePath, true)) {
                /*
                file.WriteLine($"@cmdsettings:{RepeatingType}{long.Parse(RepeatingInterval.ToString())};{boolToStr(IsRemote)};{boolToStr(NeedsNetwork)};{boolToStr(NeedsUser,NeedsSystem)};{String.Join(',',ExcludedComputers)};{ExcludedComputersRegex}");
                */
                file.WriteLine($"@cmdsettingsV2");
                if (MachineName != "") file.WriteLine($"MachineName:" + MachineName);
                if (UserName != "") file.WriteLine($"UserName:" + UserName);
                file.WriteLine($"IsRepeating:" + IsRepeating.ToString());
                file.WriteLine($"IsRemote:" + IsRemote.ToString());
                file.WriteLine($"NeedsNetwork:" + NeedsNetwork.ToString());
                file.WriteLine($"NeedsSystem:" + NeedsSystem.ToString());
                file.WriteLine($"NeedsUser:" + NeedsUser.ToString());
                if (RunAt != null) {
                    file.WriteLine($"RunAt:" + RunAt.ToString());
                }
                
                file.WriteLine($"RepeatingType:" + RepeatingType.ToString());
                file.WriteLine($"RepeatingInterval:" + long.Parse(RepeatingInterval.ToString()));
                file.WriteLine($"ExcludedComputers:" + String.Join(',', ExcludedComputers));
                file.WriteLine($"ExcludedComputersRegex:" + ExcludedComputersRegex);
                file.WriteLine($"commands:");
                foreach (string cmd in CommandLines) {
                    file.Write(cmd);
                }
                

            }
            return true;
        }

    }
}
