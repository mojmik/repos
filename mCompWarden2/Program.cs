using System;
using System.Collections.Generic;

using System.Linq;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace mCompWarden2 {

    class Program {

      


        //na pozadi to bezi diky Project > Properties> Application tab > change Output type to "Windows application".
        public static string mainPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\";
        public static string outPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\out\";
        public static string localPath = @"c:\it\compwarden\";
        public static string commandsLocalPath = localPath + @"cmd\";
        public static string commandsArcLocalPath = localPath + @"cmd\arc\";
        public static string serverPingName = "aadyn";

        /*
          zdroje prikazu: 
            remote:
                SingleCommandSetFromFile(Program.mainPath + "all.txt");
                SingleCommandSetFromFile(Program.mainPath + System.Environment.MachineName + "-" + System.Environment.UserName + ".txt");
                SingleCommandSetFromFile(Program.mainPath + System.Environment.MachineName + ".txt");
            local:
                MultipleCommandSetsFromFile(Program.commandsLocalPath, "*.txt");
         */

        /*
         * kazdej prikaz muze mit na radce @cmdsettings:opakovani;isRemote;NeedsNetwork;NeedsUser;ExcludedComputers
         * 
         */

        static System.Threading.Mutex singleton = new Mutex(true, "mCompWarden2-"+System.Environment.UserName);

        public static string GetVer() {
            return "v2.4";
        }

        static void Main(string[] args) {
            if (!singleton.WaitOne(TimeSpan.Zero, true)) {
                //there is already another instance running!                
                return;
            }

            //Logger logger = new Logger(localPath + "log.txt", mainPath + "log.txt");
            Logger.logFilePath = localPath + "log.txt";
            Logger.remoteInfoPath = @"\\aavm2\data2\";
            Logger.remoteLogPath = mainPath + "log.txt";
            Logger.remoteUrl = "http://aavm2/intra/mlogs/mlogs.php?action=putlog";

            CommandsManager commandsManager = new CommandsManager();
            NetworkTools netTools = new NetworkTools();
            RunManager runMan = new RunManager(commandsManager, serverPingName,netTools);
            System.IO.Directory.CreateDirectory(localPath);
            System.IO.Directory.CreateDirectory(commandsLocalPath);
            System.IO.Directory.CreateDirectory(commandsArcLocalPath);
            Logger.WriteLog($"mcompwarden2 {GetVer()} starting",Logger.TypeLog.both);
            Logger.WriteLog("networks: " + NetworkTools.GetIPs(), Logger.TypeLog.both);
            for (; ; ) {
                Thread.Sleep(5000);
                if (runMan.IsTime("load", 100, "s")) runMan.LoadCommands();
                runMan.DoRun();
                if (runMan.LastPing > -1) {
                    if (runMan.IsTime("ping", 5, "m")) {
                        if (System.Environment.UserName == "SYSTEM") Logger.WriteRemoteInfo("ping", runMan.LastPing.ToString());
                    }
                }
                    
                        

            }
        }
    }
}
