using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//syncs one folder to another and then keep this the first one synced
namespace mFolderSync {
    class Program {
        public static string ProgramPath;
        static System.Threading.Mutex singleton = new Mutex(true, "mFolderSyncApp");
        static void Main(string[] args) {
            //var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            //ProgramPath = new Uri(directory).LocalPath + "\\"; 
            ProgramPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            if (!singleton.WaitOne(TimeSpan.Zero, true)) {
                //there is already another instance running!
                return;
            }
            FolderWatcher folderWatcher = new FolderWatcher();
            string sharedPath = @"c:\it\locationshared2\";
            string localPath = @"c:\it\locationshared\";
            folderWatcher.initialFullSync(sharedPath, localPath);
            
            //aby nam to nezabijel GC
            List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
            watchers.Add(folderWatcher.watcher);
            folderWatcher.syncFiles(localPath, sharedPath);

            //folderWatcher.ReplaceFile(@"G:\erp\KSC\in\mtemp\blocek.txt");
            for (; ; ) {
                Thread.Sleep(100);
                GC.KeepAlive(folderWatcher.watcher);
            }
        }

    }
}
