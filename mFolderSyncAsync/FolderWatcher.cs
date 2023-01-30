using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mFolderSyncAsync {


    class FolderWatcher {
        DateTime lastRead = DateTime.MinValue;

        public FileSystemWatcher watcherLocal = new FileSystemWatcher();
        public FileSystemWatcher watcherRemote = new FileSystemWatcher();
        private string folderLocal, folderRemote;
        
        public void initialFullSync(string src, string dst) {
            if (src.EndsWith("\\")) src = src.Substring(0, src.Length - 1);
            if (dst.EndsWith("\\")) dst = dst.Substring(0, dst.Length - 1);
            if (!Directory.Exists(dst)) Directory.CreateDirectory(dst);

            List<string> srcDirs=new List<string>();
            List<string> srcFiles = new List<string>();
            System.IO.DirectoryInfo dirSrc = new System.IO.DirectoryInfo(src);
            foreach (System.IO.DirectoryInfo s in dirSrc.GetDirectories("*", SearchOption.AllDirectories)) {
                string relPath = s.FullName.Substring(src.Length + 1);
                /*                
                string fullPathDst = dst + "\\" + relPath;
                Directory.CreateDirectory(fullPathDst);
                */
                srcDirs.Add(relPath);
            }

            foreach (System.IO.FileInfo s in dirSrc.GetFiles("*.*", System.IO.SearchOption.AllDirectories)) {
                string relPath = s.FullName.Substring(src.Length+1);
                /*
                string fullPathDst = dst + "\\" + relPath;
                if (!filesEqual(s.FullName, fullPathDst)) {
                    syncFile(s.FullName, src, dst);
                }
                */
                srcFiles.Add(relPath);
            }

            bool deleteLocalFilesAndDirs = false;

            System.IO.DirectoryInfo dirDst = new System.IO.DirectoryInfo(dst);
            foreach (System.IO.FileInfo s in dirDst.GetFiles("*.*", System.IO.SearchOption.AllDirectories)) {
                string relPath = s.FullName.Substring(dst.Length + 1);
                if (srcFiles.Contains(relPath)) {
                    FilesTools.syncFile(src + "\\" + relPath, dst + "\\" + relPath,false);
                    srcFiles.Remove(relPath);
                } else {
                    //lokalni soubory nebudeme odstranovat
                    if (deleteLocalFilesAndDirs) File.Delete(dst + "\\" + relPath);
                }
            }

            foreach (System.IO.DirectoryInfo s in dirDst.GetDirectories("*", SearchOption.AllDirectories)) {
                string relPath = s.FullName.Substring(dst.Length + 1);
                if (srcDirs.Contains(relPath)) {
                    srcDirs.Remove(relPath);
                } else {
                    if (deleteLocalFilesAndDirs) Directory.Delete(dst + "\\" + relPath);
                }
            }

            foreach (string s in srcDirs) {
                Directory.CreateDirectory(dst + "\\" + s);
            }
            
            foreach (string s in srcFiles) {
                FilesTools.syncFile(src + "\\" + s, dst + "\\" + s,false);
            }

        }
     
        public void startWatcher(string src, string dst) {
            if (src.EndsWith("\\")) src = src.Substring(0, src.Length - 1);
            if (dst.EndsWith("\\")) dst = dst.Substring(0, dst.Length - 1);
            folderLocal = src;
            folderRemote = dst;

            /*
            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size; 
            */
          

            if (!File.Exists(src)) Directory.CreateDirectory(src);
            if (!File.Exists(dst)) Directory.CreateDirectory(dst);
            watcherLocal.NotifyFilter =
                              NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             ;
            watcherLocal.IncludeSubdirectories = true;
            watcherLocal.Path = src;
            watcherLocal.Filter = "*.*";
            watcherLocal.Changed += OnChanged;
            watcherLocal.Renamed += OnRenamed;
            watcherLocal.Created += OnCreated;
            watcherLocal.Deleted += OnDeleted;
            watcherLocal.EnableRaisingEvents = true;

            watcherRemote.NotifyFilter =
                              NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastWrite
                             ;
            watcherRemote.IncludeSubdirectories = true;
            watcherRemote.Path = dst;
            watcherRemote.Filter = "*.*";
            watcherRemote.Changed += OnChanged;
            watcherRemote.Renamed += OnRenamed;
            watcherRemote.Created += OnCreated;
            watcherRemote.Deleted += OnDeleted;
            watcherRemote.EnableRaisingEvents = true;
            Program.WriteLogFile($"sync job started " + DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"));
        }
        
        private void OnChanged(object source, FileSystemEventArgs e) {
            string srcPath, dstPath;
            FileSystemWatcher dstWatcher;
            DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime != lastRead && !Directory.Exists(e.FullPath)) {                
                //Copies file to another directory.
                if (e.FullPath.StartsWith(folderLocal)) {
                    srcPath = folderLocal;
                    dstPath = folderRemote;
                    dstWatcher = watcherRemote;
                } else {
                    srcPath = folderRemote;
                    dstPath = folderLocal;
                    dstWatcher = watcherLocal;
                }
                Program.syncMan.addOp(e.FullPath, srcPath, dstPath, null, SyncOp.OpType.Copy, dstWatcher);
                lastRead = lastWriteTime;
            }
            

        }
        private void OnDeleted(object source, FileSystemEventArgs e) {
            string srcPath, dstPath;
            FileSystemWatcher dstWatcher;
            //Copies file to another directory.
            //DeleteFile(e.FullPath, "deleted");
            if (e.FullPath.StartsWith(folderLocal)) {
                srcPath = folderLocal;
                dstPath = folderRemote;
                dstWatcher = watcherRemote;
            }
            else {
                srcPath = folderRemote;
                dstPath = folderLocal;
                dstWatcher = watcherLocal;
            }
            Program.syncMan.addOp(e.FullPath, srcPath, dstPath, null, SyncOp.OpType.Delete, dstWatcher);
        }
        
        private void OnRenamed(object source, RenamedEventArgs e) {
            string srcPath, dstPath;
            FileSystemWatcher dstWatcher;
            //RenameFile(e.OldFullPath, e.FullPath, "renamed");
            if (e.FullPath.StartsWith(folderLocal)) {
                srcPath = folderLocal;
                dstPath = folderRemote;
                dstWatcher = watcherRemote;
            }
            else {
                srcPath = folderRemote;
                dstPath = folderLocal;
                dstWatcher = watcherLocal;
            }
            Program.syncMan.addOp(e.OldFullPath,srcPath, dstPath, e.FullPath, SyncOp.OpType.Rename, dstWatcher);
        }
        private void OnCreated(object source, FileSystemEventArgs e) {
            string srcPath, dstPath;
            FileSystemWatcher dstWatcher;
            if (e.FullPath.StartsWith(folderLocal)) {
                srcPath = folderLocal;
                dstPath = folderRemote;
                dstWatcher = watcherRemote;
            }
            else {
                srcPath = folderRemote;
                dstPath = folderLocal;
                dstWatcher = watcherLocal;
            }
            string relPath = e.FullPath.Substring(srcPath.Length + 1);
            Program.syncMan.addOp(e.FullPath, relPath, dstPath, null, SyncOp.OpType.Create, dstWatcher);
        }

        public void Dispose() {
            // avoiding resource leak
            watcherLocal.Changed -= OnChanged;
            watcherLocal.Renamed -= OnRenamed;
            watcherLocal.Created -= OnCreated;
            watcherLocal.Deleted -= OnDeleted;
            this.watcherLocal.Dispose();
        }
    }
}
