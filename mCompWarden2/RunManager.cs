using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace mCompWarden2 {
    class RunManager {
        public DateTime LastRun { get; private set; }
        private DateTime LastCheck { get; set; }
        public long LastPing { get; private set; }
        private string ServerName { get; set; }
        public RunManager(string serverName = "aadyn") {
            ServerName = serverName;
        }
        public bool IsHostAvailable(string nameOrAddress) {
            bool pingable = false;
            Ping pinger = new Ping();
            try {
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
                LastPing = reply.RoundtripTime;
            }
            catch (PingException) {
                LastPing = -1;
                // Discard PingExceptions and return false;
            }
            return pingable;
        }
        public List<string> GetRemoteCommands() {
            string fileName = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\all.txt";
            List<string> remoteCommands=new List<string>();
            using (System.IO.StreamReader reader = new System.IO.StreamReader(fileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    remoteCommands.Add(line);
                }
            }
            
            fileName = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\" + System.Environment.MachineName + "-" + System.Environment.UserName + ".txt";
            using (System.IO.StreamReader reader = new System.IO.StreamReader(fileName)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    remoteCommands.Add(line);
                }
            }
            return remoteCommands;
        }
        public void RunCommand(string command,string arguments="") {
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
        public void RunCommands(List <string> commandList) {
            foreach (string cmd in commandList) {
                RunCommand(cmd);
            }            
        }
        public bool DoRun() {
            if ((DateTime.Now - LastCheck).TotalSeconds < 10) return false;

            if (IsHostAvailable(ServerName) && LastRun < DateTime.Today) {
                RunCommands(GetRemoteCommands());
                LastRun = DateTime.Now;
                return true;
            }
            return false;
        }
    }
}
