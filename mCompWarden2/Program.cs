using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                for (; ; )
                {
                    Thread.Sleep(5000);
                    if (runMan.IsTime("load", 100, "s")) runMan.LoadCommands();
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
                try { Logger.WriteLog("FATAL top-level: " + ex, Logger.TypeLog.both); } catch { }
            }
        }
        public static class IoSafe
        {
            public static string[] ReadAllLinesRetry(string path, int retries = 5, int delayMs = 200)
            {
                for (int i = 0; ; i++)
                {
                    try { return File.ReadAllLines(path, Encoding.UTF8); }
                    catch (IOException) when (i < retries) { Thread.Sleep(delayMs); }
                }
            }

            public static void AppendAllTextRetry(string path, string text, int retries = 5, int delayMs = 150)
            {
                for (int i = 0; ; i++)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
                        File.AppendAllText(path, text, new UTF8Encoding(false));
                        return;
                    }
                    catch (IOException) when (i < retries) { Thread.Sleep(delayMs); }
                    catch (UnauthorizedAccessException) when (i < retries) { Thread.Sleep(delayMs); }
                }
            }

            public static void MoveOrCopyThenDeleteRetry(string src, string dst, int retries = 5, int delayMs = 200)
            {
                for (int i = 0; ; i++)
                {
                    try
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(dst) ?? "");
                        File.Move(src, dst);
                        return;
                    }
                    catch (IOException) when (i < retries)
                    {
                        // try copy-as-fallback if locked
                        try { File.Copy(src, dst, true); } catch { }
                        Thread.Sleep(delayMs);
                        try { File.Delete(src); } catch { }
                        return;
                    }
                }
            }
        }
    }
}
