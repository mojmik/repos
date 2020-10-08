using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace mCompWarden2 {
    class Program {
        public static string mainPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\";
        public static string localPath = @"c:\it\compwarden\";        
        public static string commandsLocalPath = localPath + @"cmd\";
        public static string commandsArcLocalPath = localPath + @"cmd\arc\";
        static void Main(string[] args) {                                           
            Logger logger = new Logger(localPath + "log.txt", mainPath + "log.txt");
            CommandsManager commandsManager = new CommandsManager();
            RunManager runMan = new RunManager(logger, commandsManager);
            System.IO.Directory.CreateDirectory(localPath);
            System.IO.Directory.CreateDirectory(commandsLocalPath);
            System.IO.Directory.CreateDirectory(commandsArcLocalPath);
            for (; ; ) {
                Thread.Sleep(5000);
                if (runMan.DoRun()) logger.WriteLog("i'm run, last ping " + runMan.LastPing);
                else logger.WriteLog("not yet time to run, last ping " + runMan.LastPing);
                if (runMan.LastPing > -1) logger.WriteRemoteInfo("ping", runMan.LastPing.ToString());
            }
        }
    }
}
