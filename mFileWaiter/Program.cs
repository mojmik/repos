﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mFileWaiter {
    class Program {
        public static string ProgramPath;
        static System.Threading.Mutex singleton = new Mutex(true, "FileWaiterApp");
        static void Main(string[] args) {
            //var directory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            //ProgramPath = new Uri(directory).LocalPath + "\\"; 
            ProgramPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            if (!singleton.WaitOne(TimeSpan.Zero, true)) {
                //there is already another instance running!
                return;
            }
            FolderWatcher folderWatcherKSC= new FolderWatcher();
            FolderWatcher folderWatcherBTS = new FolderWatcher();
            //aby nam to nezabijel GC
            List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
            watchers.Add(folderWatcherKSC.watcher);
            watchers.Add(folderWatcherBTS.watcher);
            folderWatcherKSC.watchForFiles(@"\\rentex.intra\company\data\erp\KSC\in\mtemp\","KSC");
            folderWatcherBTS.watchForFiles(@"\\rentex.intra\company\data\erp\BTS\in\mtemp\","BTS");
            
            //folderWatcher.ReplaceFile(@"G:\erp\KSC\in\mtemp\blocek.txt");
            for (; ; ) {
                Thread.Sleep(100);
                GC.KeepAlive(folderWatcherKSC.watcher);
                GC.KeepAlive(folderWatcherBTS.watcher);
            }
        }
    }
}
