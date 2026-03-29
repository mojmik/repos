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
        private static string _lastChangedFile = "";

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
                    localWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                    localWatcher.Created += OnWatcherEvent;
                    localWatcher.Changed += OnWatcherEvent;
                    localWatcher.Renamed += OnWatcherEvent;
                    localWatcher.Deleted += OnWatcherEvent;
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
                    remoteWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                    remoteWatcher.Created += OnWatcherEvent;
                    remoteWatcher.Changed += OnWatcherEvent;
                    remoteWatcher.Renamed += OnWatcherEvent;
                    remoteWatcher.Deleted += OnWatcherEvent;
                    remoteWatcher.EnableRaisingEvents = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Could not start remote watcher: " + ex.Message, Logger.TypeLog.both);
                }

                for (; ; )
                {
                    bool signaled = _wakeUp.WaitOne(5000);
                    if (signaled)
                    {
                        // Debounce: wait 1s for file operations (like from Notepad or network save) to settle
                        System.Threading.Thread.Sleep(1000);
                        // Consume any redundant signals that arrived during the wait
                        while (_wakeUp.WaitOne(0)) ;
                    }

                    if (signaled || runMan.IsTime("load", 100, "s"))
                    {
                        if (signaled)
                        {
                            Logger.WriteLog($"Command file change detected ({_lastChangedFile}) - triggering immediate load.", Logger.TypeLog.both);
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

        private static void OnWatcherEvent(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.Name)) return;

                // Ignore the log file to prevent infinite loops (remote log is in the watched mainPath)
                string remoteLogFile = Path.GetFileName(Logger.remoteLogPath);
                if (e.Name.Equals(remoteLogFile, StringComparison.OrdinalIgnoreCase)) return;

                // Only signal for actual command/config files
                string ext = Path.GetExtension(e.Name).ToLower();
                if (ext == ".txt" || ext == ".cwd" || ext == ".xml")
                {
                    _lastChangedFile = e.Name;
                    _wakeUp.Set();
                }
            }
            catch (Exception ex)
            {
                // Never allow watcher events to crash the app
                try { Logger.WriteLog("Watcher event error: " + ex.Message, Logger.TypeLog.both); } catch { }
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
