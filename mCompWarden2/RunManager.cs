using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace mCompWarden2 {
    class RunManager {
        public Logger runLogger;
        public CommandsManager commandsManager;
        public DateTime LastRun { get; private set; }
        private DateTime LastCheck { get; set; }
        public long LastPing { get; private set; }
        private string ServerName { get; set; }
        public RunManager(Logger logger,CommandsManager cmdMan,string serverName = "aadyn") {
            ServerName = serverName;
            runLogger = logger;
            commandsManager = cmdMan;
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
        
        public bool DoRun() {
            if ((DateTime.Now - LastCheck).TotalSeconds < 10) return false;
            commandsManager.LoadLocalCommands();
            if (IsHostAvailable(ServerName) && LastRun < DateTime.Today) {
                commandsManager.LoadRemoteCommands();
                LastRun = DateTime.Now;
                return true;
            }
            commandsManager.RunCommands();
            return false;
        }
    }
}
