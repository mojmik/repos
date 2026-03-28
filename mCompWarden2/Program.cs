using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace mCompWarden2
{

    class Program
    {




        //na pozadi to bezi diky Project > Properties> Application tab > change Output type to "Windows application".
        public static string mainPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\";
        public static string outPath = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\out\";
        public static string archiveUNC = @"\\rentex.intra\company\data\Company\mkavan_upravy\scripts\mCompWarden2\arc";
        public static string localPath = @"c:\it\compwarden\";
        public static string commandsLocalPath = localPath + @"cmd\";
        public static string commandsArcLocalPath = localPath + @"cmd\arc\";
        public static string archiveLocal = commandsArcLocalPath;
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

        static System.Threading.Mutex singleton = new Mutex(true, "mCompWarden2-" + System.Environment.UserName);
        static AutoResetEvent _wakeUp = new AutoResetEvent(false);

        public static string GetVer()
        {
            return "v4.6";
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (!singleton.WaitOne(TimeSpan.Zero, true))
            {
                //there is already another instance running!                
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                try { Logger.WriteLog("FATAL UnhandledException: " + e.ExceptionObject, Logger.TypeLog.both); } catch { }
            };
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                try { Logger.WriteLog("UnobservedTaskException: " + e.Exception, Logger.TypeLog.both); } catch { }
                e.SetObserved();
            };

            try
            {
                //Logger logger = new Logger(localPath + "log.txt", mainPath + "log.txt");
                Logger.logFilePath = localPath + "log.txt";
                Logger.remoteInfoPath = @"\\aavm2\data2\";
                Logger.remoteLogPath = mainPath + "log.txt";
                Logger.remoteUrl = "http://aavm2/intra/mlogs/mlogs.php?action=putlog";

                CommandsManager commandsManager = new CommandsManager();
                NetworkTools netTools = new NetworkTools();
                RunManager runMan = new RunManager(commandsManager, serverPingName, netTools);
                System.IO.Directory.CreateDirectory(localPath);
                System.IO.Directory.CreateDirectory(commandsLocalPath);
                System.IO.Directory.CreateDirectory(commandsArcLocalPath);
                Logger.WriteLog($"mcompwarden2 {GetVer()} starting", Logger.TypeLog.both);
                Logger.WriteLog("networks: " + NetworkTools.GetIPs(), Logger.TypeLog.both);

                // Set up FileSystemWatchers for immediate command detection
                FileSystemWatcher localWatcher = null;
                try
                {
                    localWatcher = new FileSystemWatcher(commandsLocalPath, "*.*");
                    localWatcher.Created += (s, e) => _wakeUp.Set();
                    localWatcher.Changed += (s, e) => _wakeUp.Set();
                    localWatcher.Renamed += (s, e) => _wakeUp.Set();
                    localWatcher.Deleted += (s, e) => _wakeUp.Set();
                    localWatcher.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Could not start local watcher: " + ex.Message, Logger.TypeLog.both);
                }

                FileSystemWatcher remoteWatcher = null;
                try
                {
                    remoteWatcher = new FileSystemWatcher(mainPath, "*.*");
                    remoteWatcher.Created += (s, e) => _wakeUp.Set();
                    remoteWatcher.Changed += (s, e) => _wakeUp.Set();
                    remoteWatcher.Renamed += (s, e) => _wakeUp.Set();
                    remoteWatcher.Deleted += (s, e) => _wakeUp.Set();
                    remoteWatcher.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Could not start remote watcher: " + ex.Message, Logger.TypeLog.both);
                }

                for (; ; )
                {
                    bool signaled = _wakeUp.WaitOne(5000);
                    if (signaled || runMan.IsTime("load", 100, "s"))
                    {
                        if (signaled)
                        {
                            Logger.WriteLog("Command file change detected - triggering immediate load.", Logger.TypeLog.both);
                            runMan.IsTime("load", 0, "s"); // Force reset the polling timer
                        }
                        runMan.LoadCommands();
                    }

                    runMan.DoRun();
                    if (runMan.LastPing > -1)
                    {
                        if (runMan.IsTime("ping", 5, "m"))
                        {
                            if (System.Environment.UserName == "SYSTEM") Logger.WriteRemoteInfo("ping", runMan.LastPing.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("FATAL: Error in Main loop: " + ex.ToString(), Logger.TypeLog.both);
            }
        }
    }

    public static class IoSafe
    {
        public static void AppendAllTextRetry(string filePath, string content, int maxWaitMs = 1500)
        {
            var start = DateTime.Now;
            while (true)
            {
                try
                {
                    File.AppendAllText(filePath, content);
                    return;
                }
                catch (IOException)
                {
                    if ((DateTime.Now - start).TotalMilliseconds > maxWaitMs) throw;
                    Thread.Sleep(50);
                }
                catch (UnauthorizedAccessException)
                {
                    if ((DateTime.Now - start).TotalMilliseconds > maxWaitMs) throw;
                    Thread.Sleep(100);
                }
            }
        }
    }
}
