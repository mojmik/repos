using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security.Cryptography;

namespace mCompWarden2 {
    class RunManager {        
        public CommandsManager commandsManager;
        public NetworkTools netTools;

        public long LastPing { get; private set; }
        private string ServerName { get; set; }
        
        private DateTime PingTick { get; set; }
        private DateTime LoadTick { get; set; }

        private Dictionary<string, DateTime> timer=new Dictionary<string, DateTime>();
        public RunManager(CommandsManager cmdMan,string serverPingName,NetworkTools networkTools) {
            ServerName = serverPingName;            
            commandsManager = cmdMan;
            netTools = networkTools;
        }
        
    

        public bool IsTime(string what,double cnt,string type) {
            if (!timer.ContainsKey(what)) {
                timer[what] = DateTime.Now;
                return true;
            }
            DateTime thatTime = timer[what];
            double ranDiff=0;
            if (thatTime == null) {
                
            }
            if (type== "s") ranDiff = (DateTime.Now - thatTime).TotalSeconds;
            if (type== "m") ranDiff = (DateTime.Now - thatTime).TotalMinutes;
            if (type== "h") ranDiff = (DateTime.Now - thatTime).TotalHours;
            if (type== "d") ranDiff = (DateTime.Now - thatTime).TotalDays;

            if (ranDiff < cnt) {
                return false;                
            }

            timer[what] = DateTime.Now;
            return true;
        }

        public void LoadCommands() {            
            commandsManager.LoadLocalCommands();
            LastPing = netTools.IsHostAvailable(ServerName);
            if (LastPing > -1) {
                commandsManager.LoadRemoteCommands();
            }
        }
        public bool DoRun() {           
            commandsManager.RunCommands((LastPing>-1) ? true : false);
            bool doSaveXml = false;
            if (doSaveXml) commandsManager.SaveIntoXML();
            return false;
        }
    }
}
