using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace mCompWarden2 {
    class Program {
        public static string mainPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\";
        static void Main(string[] args) {                                           
            Logger logger = new Logger();
            CommandsManager commandsManager = new CommandsManager();
            RunManager runMan = new RunManager(logger, commandsManager);
            for (; ; ) {
                Thread.Sleep(5000);
                if (runMan.DoRun()) logger.WriteLog("i'm run, last ping " + runMan.LastPing);
                else logger.WriteLog("not yet time to run, last ping " + runMan.LastPing);
                if (runMan.LastPing > -1) logger.WriteRemoteInfo("ping", runMan.LastPing.ToString());
            }
        }
    }
}
