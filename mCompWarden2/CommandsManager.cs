using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mCompWarden2 {
    class CommandsManager {        
        public List<string> CommandsList { get; set; } = new List<string>();        

        public void LoadRemoteCommands() {
            //run daily
            string fileName = @"" + Program.mainPath + "all.txt";
            using (System.IO.StreamReader reader = new System.IO.StreamReader(fileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    CommandsList.Add(line);
                }
            }

            fileName = @"" + Program.mainPath + System.Environment.MachineName + "-" + System.Environment.UserName + ".txt";
            using (System.IO.StreamReader reader = new System.IO.StreamReader(fileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    CommandsList.Add(line);
                }
            }
        }
        public void LoadLocalCommands() {
            //run immediately                        
            string line;
            string arcFile;
            DirectoryInfo d = new DirectoryInfo(Program.commandsLocalPath);
            FileInfo[] Files = d.GetFiles("*.txt");
            foreach (FileInfo file in Files) {                
                using (System.IO.StreamReader reader = new System.IO.StreamReader(file.FullName)) {                 
                    while ((line = reader.ReadLine()) != null) {
                        CommandsList.Add(line);
                    }
                }
                arcFile = Program.commandsArcLocalPath + file.Name;
                if (File.Exists(arcFile)) File.Delete(arcFile);
                File.Move(file.FullName, arcFile);
            }
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

        public void RunCommand(string command) {
            string arguments="";
            //string[] cmd = ParseMultiSpacedArguments(command);
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
        public void RunCommands() {
            CommandsList = CommandsList.Distinct().ToList();
            foreach (string cmd in CommandsList.ToList()) {
                RunCommand(cmd);
                CommandsList.Remove(cmd);
            }
        }
    }
}
