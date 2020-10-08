using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mCompWarden2 {
    class CommandsManager {
        public List<string> CommandsList { get; set; } = new List<string>();

        public void LoadRemoteCommands() {
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
        public void RunCommand(string command, string arguments = "") {
            var proc = new System.Diagnostics.Process {
                StartInfo = new System.Diagnostics.ProcessStartInfo {
                    FileName = @command,
                    Arguments = @arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
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
