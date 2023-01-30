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

        public FileSystemWatcher watcherSrc = new FileSystemWatcher();
        public FileSystemWatcher watcherDst = new FileSystemWatcher();
        private string folderSrc, folderDst;
        public bool filesEqual(string f1, string f2) {
            bool f1Ex = File.Exists(f1);
            bool f2Ex = File.Exists(f2);
            if (!f1Ex && !f2Ex) return true;
            if (!f1Ex || !f2Ex) return false;
            FileInfo f1Info = new FileInfo(f1);
            FileInfo f2Info = new FileInfo(f2);
            if (f1Info.Length != f2Info.Length) return false;
            if (f1Info.LastWriteTime != f2Info.LastWriteTime) return false;
            return true;
        }
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
                    syncFile(src + "\\" + relPath, dst + "\\" + relPath,false);
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
                syncFile(src + "\\" + s, dst + "\\" + s,false);
            }

        }
     
        public void startWatcher(string src, string dst) {
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
            watcherSrc.NotifyFilter =
                                NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastWrite
                               ;

            watcherDst.NotifyFilter =
                                NotifyFilters.CreationTime
                               | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               | NotifyFilters.LastWrite
                               ;

            if (!File.Exists(src)) Directory.CreateDirectory(src);
            if (!File.Exists(dst)) Directory.CreateDirectory(dst);
            watcherSrc.IncludeSubdirectories = true;
            watcherSrc.Path = src;
            watcherSrc.Filter = "*.*";
            watcherSrc.Changed += OnChanged;
            watcherSrc.Renamed += OnRenamed;
            watcherSrc.Created += OnCreated;
            watcherSrc.Deleted += OnDeleted;
            watcherSrc.EnableRaisingEvents = true;

            watcherDst.IncludeSubdirectories = true;
            watcherDst.Path = dst;
            watcherDst.Filter = "*.*";
            watcherDst.Changed += OnChanged;
            watcherDst.Renamed += OnRenamed;
            watcherDst.Created += OnCreated;
            watcherDst.Deleted += OnDeleted;
            watcherDst.EnableRaisingEvents = true;

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
        private void OnRenamed(object source, RenamedEventArgs e) {
            RenameFile(e.OldFullPath, e.FullPath, "renamed");
        }
        private void OnCreated(object source, FileSystemEventArgs e) {
            string relPath = e.FullPath.Substring(folderSrc.Length + 1);
            if (Directory.Exists(e.FullPath)) { //jestli to je folder
                createPath(relPath + "\\", folderDst);
            }
            else {
                //vytvoreni prazdneho souboru
                CopyFile(e.FullPath, "changed");
            }

        }
        private void OnDeleted(object source, FileSystemEventArgs e) {
            //Copies file to another directory.
            DeleteFile(e.FullPath, "deleted");
        }
        public void createPath(string relPath, string dstPath) {
            string[] relFolders = relPath.Split('\\');
            string relFolderPath = dstPath;
            for (int n = 0; n < relFolders.Length - 1; n++) {
                relFolderPath = relFolderPath + "\\" + relFolders[n];
                if (!Directory.Exists(relFolderPath)) Directory.CreateDirectory(relFolderPath);
            }
        }
        public void createPath(string fullPath) {
            fullPath = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
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
        public void syncFile(string fullPathSrc, string fullPathDst, bool checkPath=true) {
            if (!filesEqual(fullPathSrc, fullPathDst)) {
                //check dst path
                if (checkPath) createPath(fullPathDst);
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
            if (fullPathSrc.StartsWith(folderSrc)) syncFile(fullPathSrc, folderSrc, folderDst);
            else syncFile(fullPathSrc, folderDst, folderSrc);
        }
        public void DeleteFile(string fullPathSrc, string debugInfo = "") {
            string log = "",relPath,fullPathDst;
            WriteLog($"file event {debugInfo} src: {fullPathSrc}");
            if (fullPathSrc.StartsWith(folderSrc)) {
                relPath = fullPathSrc.Substring(folderSrc.Length + 1);
                fullPathDst = folderDst + "\\" + relPath;
            } else {
                relPath = fullPathSrc.Substring(folderDst.Length + 1);
                fullPathDst = folderSrc + "\\" + relPath;
            }
            

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
            string srcPath,dstPath;
            if (oldFullPathSrc.StartsWith(folderSrc)) {
                dstPath = folderDst;
                srcPath = folderSrc;
            }
            else {
                dstPath = folderSrc;
                srcPath = folderDst;
            }

            string relPath = fullPathSrc.Substring(srcPath.Length + 1);
            string fullPathDst = dstPath+ "\\" + relPath;

            string relPathOld = oldFullPathSrc.Substring(srcPath.Length + 1);
            string fullPathDstOld = dstPath + "\\" + relPathOld;

            WriteLog($"file event {debugInfo} src: {fullPathSrc}");

            createPath(relPath, dstPath);

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
       

        public void Dispose() {
            // avoiding resource leak
            watcherSrc.Changed -= OnChanged;
            watcherSrc.Renamed -= OnRenamed;
            watcherSrc.Created -= OnCreated;
            watcherSrc.Deleted -= OnDeleted;
            this.watcherSrc.Dispose();

            watcherDst.Changed -= OnChanged;
            watcherDst.Renamed -= OnRenamed;
            watcherDst.Created -= OnCreated;
            watcherDst.Deleted -= OnDeleted;
            this.watcherDst.Dispose();
        }
    }
}
