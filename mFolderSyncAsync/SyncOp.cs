using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace mFolderSyncAsync {
    class SyncOp {
        private string source;
        private string dst;
        private string folderDst;
        private string folderSrc;
        public enum OpType { Copy, Delete, Rename,Create };
        private OpType operationType;
        public int Status { get; set; }
        FileSystemWatcher dstWatcher;
        public SyncOp(string s, string d, string f, string dstFull, OpType opTyp, FileSystemWatcher watcher) {
            source = s;
            folderSrc = d;
            folderDst = f;
            Status = 0;
            dst = dstFull;
            operationType = opTyp;
            dstWatcher = watcher;
        }
        public override string ToString() {
            return "op " + operationType.ToString() + ", src: " + source + ", folderSec: " + folderSrc + ", folderDst: " + folderDst;
        }
        public bool isEqual(string src,string fSrc,string fDst,string fullDst, OpType opType, FileSystemWatcher watcher) {
            if ((src == source) && (fSrc == folderSrc) && (fDst == folderDst) && (fullDst == dst) && (operationType == opType) && (dstWatcher == watcher) ) return true;
            else return false;
        }
        public bool process() {
            dstWatcher.EnableRaisingEvents = false;
            if (operationType == OpType.Rename) FilesTools.RenameFile(source, dst,folderSrc,folderDst);
            if (operationType == OpType.Delete) FilesTools.DeleteFile(source,folderSrc,folderDst);
            if (operationType == OpType.Create) FilesTools.Create(source, folderSrc,folderDst);
            if (operationType == OpType.Copy) FilesTools.SyncFile(source, folderSrc, folderDst);
            dstWatcher.EnableRaisingEvents = true;
            if (FilesTools.errCode != 0) return false;            
            return true;
        }
        
   
        
    }
}
