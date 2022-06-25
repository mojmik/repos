using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mFolderSyncAsync {
    class SyncOp {
        private string source;
        private string dst;
        private string folderDst;
        private string folderSrc;
        public int Status { get; set; }
        public SyncOp(string s, string d, string f) {
            source = s;
            folderSrc = d;
            folderDst = f;
            Status = 0;
        }
        public override string ToString() {
            return "op src: " + source + " folderSec: " + folderSrc + " folderDst: " + folderDst;
        }
        public bool isEqual(string src,string fSrc,string fDst) {
            if ((src == source) && (fSrc == folderSrc) && (fDst == folderDst)) return true;
            else return false;
        }

    }
}
