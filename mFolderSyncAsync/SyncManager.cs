using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace mFolderSyncAsync {
     class SyncManager {
        private List<SyncOp> operation = new List<SyncOp>();
        public void addOp(string src, string folderSrc, string dstFolder) {
            foreach (SyncOp op in operation) {
                if (op.Status == 0 && op.isEqual(src, folderSrc, dstFolder)) return; //we already have such task pending
            }
            operation.Add(new SyncOp(src, folderSrc, dstFolder));
            Program.WriteLogFile("added, pending cnt: " + getPendingCnt() + "; " +operation.Last().ToString());
        }
        public void keepProcessing() {
            for (; ; ) {
                Thread.Sleep(350);
                processAll();
            }
        }
        public int getPendingCnt() {
            int pending = 0;
            foreach (SyncOp op in operation) {
                if (op.Status == 0) {
                    pending++;
                }
            }
            return pending;
        }
        public void processAll() {
            for (int n=0;n<operation.Count;n++) {
                if (operation[n].Status == 0) {
                    processOp(operation[n]);
                    operation[n].Status = 1;
                }
            }
        }
        public void processOp(SyncOp op) {
            Program.WriteLogFile("processed " + op.ToString());
            Thread.Sleep(5000);
        }
    }
}
