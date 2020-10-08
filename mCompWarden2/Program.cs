using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace mCompWarden2 {
    class Program {
        static void Main(string[] args) {                               
            RunManager runMan = new RunManager();
            Logger logger = new Logger();
            for (; ; ) {
                Thread.Sleep(5000);
                if (runMan.DoRun()) logger.WriteLog("i'm run, last ping " + runMan.LastPing);
                else logger.WriteLog("not yet time to run, last ping " + runMan.LastPing);
                if (runMan.LastPing > -1) logger.WriteRemoteLog("ping", runMan.LastPing.ToString());
            }
        }
    }
}
