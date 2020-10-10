﻿using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace mCompWarden2 {

    class Program {

      


        //na pozadi to bezi diky Project > Properties> Application tab > change Output type to "Windows application".
        public static string mainPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\";
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
         */

        static System.Threading.Mutex singleton = new Mutex(true, "mCompWarden2-"+System.Environment.UserName);

        

        static void Main(string[] args) {
            if (!singleton.WaitOne(TimeSpan.Zero, true)) {
                //there is already another instance running!                
                return;
            }

            Logger logger = new Logger(localPath + "log.txt", mainPath + "log.txt");
            CommandsManager commandsManager = new CommandsManager(logger);
            NetworkTools netTools = new NetworkTools();
            RunManager runMan = new RunManager(commandsManager, serverPingName,netTools);
            System.IO.Directory.CreateDirectory(localPath);
            System.IO.Directory.CreateDirectory(commandsLocalPath);
            System.IO.Directory.CreateDirectory(commandsArcLocalPath);
            logger.WriteLog("mcompwarden2 starting",Logger.TypeLog.both);
            logger.WriteLog("networks: " + netTools.GetIPs("full"), Logger.TypeLog.both);
            for (; ; ) {
                Thread.Sleep(5000);
                if (runMan.IsTime("load", 100, "s")) runMan.LoadCommands();
                runMan.DoRun();
                if (runMan.LastPing > -1) {
                    if (runMan.IsTime("ping", 5, "m")) {
                        logger.WriteRemoteInfo("ping", runMan.LastPing.ToString());
                    }
                }
                    
                        

            }
        }
    }
}
