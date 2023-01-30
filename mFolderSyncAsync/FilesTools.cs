using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mFolderSyncAsync {
    class FilesTools {
        public static int errCode = 0;
        public static bool filesEqual(string f1, string f2) {
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
        public static void SyncFile(string fullPathSrc, string folderSrc, string folderDst) {
            string relPath = fullPathSrc.Substring(folderSrc.Length + 1);
            string fullPathDst = folderDst + "\\" + relPath;

            if (!FilesTools.filesEqual(fullPathSrc, fullPathDst)) {
                //check dst path
                createPath(relPath, folderDst);
                try {
                    if (File.Exists(fullPathDst)) File.Delete(fullPathDst);
                    File.Copy(fullPathSrc, fullPathDst);
                    errCode = 0;
                }
                catch {
                   
                    if (errCode == 0) Program.WriteLogFile($"fail to copy {fullPathSrc} to {fullPathDst} ");
                    errCode = 1;
                }
            }
        }
        public static void syncFile(string fullPathSrc, string fullPathDst, bool checkPath = true) {
            if (!FilesTools.filesEqual(fullPathSrc, fullPathDst)) {
                //check dst path
                if (checkPath) createPath(fullPathDst);
                try {
                    if (File.Exists(fullPathDst)) File.Delete(fullPathDst);
                    File.Copy(fullPathSrc, fullPathDst);
                    errCode = 0;
                }
                catch {
                    
                    if (errCode == 0) Program.WriteLogFile($"fail to copy {fullPathSrc} to {fullPathDst} ");
                    errCode = 1;
                }
            }
        }
        public static void createPath(string relPath, string folderDst) {
            string[] relFolders = relPath.Split('\\');
            string relFolderPath = folderDst;
            for (int n = 0; n < relFolders.Length - 1; n++) {
                relFolderPath = relFolderPath + "\\" + relFolders[n];
                if (!Directory.Exists(relFolderPath)) Directory.CreateDirectory(relFolderPath);
            }
        }

        public static void createPath(string fullPath) {
            fullPath = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(fullPath)) Directory.CreateDirectory(fullPath);
        }
        public static void DeleteFile(string fullPathSrc, string folderSrc,string folderDst, string debugInfo = "") {
            string log = "";
            string relPath = fullPathSrc.Substring(folderSrc.Length + 1);
            string fullPathDst = folderDst + "\\" + relPath;
            if (File.Exists(fullPathDst)) {
                //is file
                try {
                    File.Delete(fullPathDst);
                    errCode = 0;
                }
                catch {
                    
                    if (errCode == 0) Program.WriteLogFile($"fail to delete file {fullPathDst} ");
                    errCode = 1;
                }
            }
            else if (Directory.Exists(fullPathDst)) {
                //is file
                try {
                    Directory.Delete(fullPathDst);
                    errCode = 0;
                }
                catch {
                    
                    if (errCode == 0) Program.WriteLogFile($"fail to delete dir {fullPathDst} ");
                    errCode = 1;
                }
            }
        }
        public static void RenameFile(string oldFullPathSrc, string fullPathSrc,string folderSrc,string folderDst, string debugInfo = "") {

            string relPath = fullPathSrc.Substring(folderSrc.Length + 1);
            string fullPathDst = folderDst + "\\" + relPath;

            string relPathOld = oldFullPathSrc.Substring(folderSrc.Length + 1);
            string fullPathDstOld = folderDst + "\\" + relPathOld;


            FilesTools.createPath(relPath, folderDst);

            if (File.Exists(fullPathDstOld)) {
                try {
                    File.Move(fullPathDstOld, fullPathDst);
                    errCode = 0;
                }
                catch {
                    
                    if (errCode == 0) Program.WriteLogFile($"fail to move file {fullPathDstOld} ");
                    errCode = 1;
                }

            }
            else if (Directory.Exists(fullPathDstOld)) {
                try {
                    Directory.Move(fullPathDstOld, fullPathDst);
                    errCode = 0;
                }
                catch {
                    
                    if (errCode==0) Program.WriteLogFile($"fail to move dir {fullPathDstOld} ");
                    errCode = 1;
                }
            }
        }
        public static void Create(string src,string relPath, string folderDst) {
            if (Directory.Exists(src)) { //jestli to je folder
                FilesTools.createPath(relPath + "\\", folderDst);
            }
            else {
                //vytvoreni prazdneho souboru
                SyncFile(src,relPath,folderDst);
            }
        }
    }
}
