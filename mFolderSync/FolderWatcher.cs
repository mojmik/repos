using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mFolderSync {


    class FolderWatcher {
        DateTime lastRead = DateTime.MinValue;

        public FileSystemWatcher watcher = new FileSystemWatcher();
        private string folderSrc, folderDst;
        public bool filesEqual(string f1, string f2) {
            bool f1Ex = File.Exists(f1);
            bool f2Ex = File.Exists(f2);
            if (!f1Ex && !f2Ex) return true;
            if (!f1Ex || !f2Ex) return false;
            FileInfo f1Info = new FileInfo(f1);
            FileInfo f2Info = new FileInfo(f2);
            if (f1Info.Length != f2Info.Length) return false;
            if (f1Info.Attributes != f2Info.Attributes) return false;
            if (f1Info.LastWriteTime != f2Info.LastWriteTime) return false;
            return true;
        }
        public void initialFullSync(string src, string dst) {
            if (src.EndsWith("\\")) src = src.Substring(0, src.Length - 1);
            if (dst.EndsWith("\\")) dst = dst.Substring(0, dst.Length - 1);

            //remove everything in dst
            System.IO.DirectoryInfo dirDst = new System.IO.DirectoryInfo(dst);

            foreach (System.IO.FileInfo s in dirDst.GetFiles("*.*", System.IO.SearchOption.AllDirectories)) {
                File.Delete(s.FullName);
            }

            foreach (System.IO.DirectoryInfo s in dirDst.GetDirectories("*", SearchOption.AllDirectories)) {
                Directory.Delete(s.FullName);
            }


            System.IO.DirectoryInfo dirSrc = new System.IO.DirectoryInfo(src);
            

            foreach (System.IO.DirectoryInfo s in dirSrc.GetDirectories("*", SearchOption.AllDirectories)) {
                string relPath = s.FullName.Substring(src.Length + 1);
                string fullPathDst = dst + "\\" + relPath;
                Directory.CreateDirectory(fullPathDst);
            }

            foreach (System.IO.FileInfo s in dirSrc.GetFiles("*.*", System.IO.SearchOption.AllDirectories)) {
                string relPath = s.FullName.Substring(src.Length+1);
                string fullPathDst = dst + "\\" + relPath;

                if (!filesEqual(s.FullName, fullPathDst)) {
                    syncFile(s.FullName, src, dst);
                }
            }
            
        }
     
        public void syncFiles(string src, string dst) {
            if (src.EndsWith("\\")) src = src.Substring(0, src.Length - 1);
            if (dst.EndsWith("\\")) dst = dst.Substring(0, dst.Length - 1);
            folderSrc = src;
            folderDst = dst;

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
            watcher.NotifyFilter =
                                NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastWrite
                               ;

            if (!File.Exists(src)) Directory.CreateDirectory(src);
            if (!File.Exists(dst)) Directory.CreateDirectory(dst);
            watcher.IncludeSubdirectories = true;
            watcher.Path = src;
            watcher.Filter = "*.*";
            watcher.Changed += OnChanged;
            watcher.Renamed += OnRenamed;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.EnableRaisingEvents = true;
            WriteLog($"sync job started " + DateTime.Now.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"));
        }
        private void WriteLog(string log) {
            //File.AppendAllText(Path.Combine(watcher.Path, $"watcherlog{jobName}.txt"), log);
            try {
                File.AppendAllText(Path.Combine(Program.ProgramPath, $"watcherlogsync.txt"), log + Environment.NewLine);
            } catch {

            }
            
        }
        private void OnChanged(object source, FileSystemEventArgs e) {
            DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);
            if (lastWriteTime != lastRead) {
                //Copies file to another directory.            
                CopyFile(e.FullPath, "changed");
                lastRead = lastWriteTime;
            }

        }
        private void OnDeleted(object source, FileSystemEventArgs e) {
            //Copies file to another directory.
            DeleteFile(e.FullPath, "deleted");
        }
        public void createPath(string relPath, string folderDst) {
            string[] relFolders = relPath.Split('\\');
            string relFolderPath = folderDst;
            for (int n = 0; n < relFolders.Length - 1; n++) {
                relFolderPath = relFolderPath + "\\" + relFolders[n];
                if (!Directory.Exists(relFolderPath)) Directory.CreateDirectory(relFolderPath);
            }
        }
        public void syncFile(string fullPathSrc, string folderSrc, string folderDst) {
            string relPath = fullPathSrc.Substring(folderSrc.Length + 1);
            string fullPathDst = folderDst + "\\" + relPath;

            if (!filesEqual(fullPathSrc, fullPathDst)) {
                //check dst path
                createPath(relPath, folderDst);
                try {
                    if (File.Exists(fullPathDst)) File.Delete(fullPathDst);
                    File.Copy(fullPathSrc, fullPathDst);
                }
                catch {
                    WriteLog($"fail to copy {fullPathSrc} to {fullPathDst} ");
                }
            }
        }
        public void CopyFile(string fullPathSrc, string debugInfo = "") {
            string log = "";
            WriteLog($"file event {debugInfo} src: {fullPathSrc}");            
            syncFile(fullPathSrc, folderSrc,folderDst);
        }
        public void DeleteFile(string fullPathSrc, string debugInfo = "") {
            string log = "";
            WriteLog($"file event {debugInfo} src: {fullPathSrc}");
            string relPath = fullPathSrc.Substring(folderSrc.Length + 1);
            string fullPathDst = folderDst + "\\" + relPath;
            if (File.Exists(fullPathDst)) {
                //is file
                try {
                    File.Delete(fullPathDst);
                }
                catch {
                    WriteLog($"fail to delete file {fullPathDst} ");
                }
            }
            else if (Directory.Exists(fullPathDst)) {
                //is file
                try {
                    Directory.Delete(fullPathDst);
                }
                catch {
                    WriteLog($"fail to delete dir {fullPathDst} ");
                }
            }
        }
        public void RenameFile(string oldFullPathSrc, string fullPathSrc, string debugInfo = "") {

            string relPath = fullPathSrc.Substring(folderSrc.Length + 1);
            string fullPathDst = folderDst + "\\" + relPath;

            string relPathOld = oldFullPathSrc.Substring(folderSrc.Length + 1);
            string fullPathDstOld = folderDst + "\\" + relPathOld;

            WriteLog($"file event {debugInfo} src: {fullPathSrc}");

            createPath(relPath, folderDst);

            if (File.Exists(fullPathDstOld)) {
                try {
                    File.Move(fullPathDstOld, fullPathDst);
                }
                catch {
                    WriteLog($"fail to move file {fullPathDstOld} ");
                }

            }
            else if (Directory.Exists(fullPathDstOld)) {
                try {
                    Directory.Move(fullPathDstOld, fullPathDst);
                }
                catch {
                    WriteLog($"fail to move dir {fullPathDstOld} ");
                }
            }

        }
        private void OnRenamed(object source, RenamedEventArgs e) {
            RenameFile(e.OldFullPath, e.FullPath, "renamed");
        }
        private void OnCreated(object source, FileSystemEventArgs e) {
            string relPath = e.FullPath.Substring(folderSrc.Length + 1);
            if (Directory.Exists(e.FullPath)) {
                createPath(relPath + "\\", folderDst);
            }
        }

        public void Dispose() {
            // avoiding resource leak
            watcher.Changed -= OnChanged;
            watcher.Renamed -= OnRenamed;
            watcher.Created -= OnCreated;
            watcher.Deleted -= OnDeleted;
            this.watcher.Dispose();
        }
    }
}
