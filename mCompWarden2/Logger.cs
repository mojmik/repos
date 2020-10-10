using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mCompWarden2 {
    class Logger {
        string logFilePath, remoteLogPath, remoteInfoPath,remoteUrl;
        public enum TypeLog { local, remote, both }
        public Logger(string outFile, string remoteLog) {
            logFilePath = outFile;
            remoteInfoPath = @"\\aavm2\data2\";
            remoteLogPath = remoteLog;
            remoteUrl = "http://aavm2/intra/mlogs/mlogs.php?action=putlog";
        }
        private void WriteLogFile(string txt, string filePath) {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filePath, true)) {
                file.WriteLine(txt);
            }
        }
        private string PostLogRecord(System.Collections.Specialized.NameValueCollection data) {
            using (var wb = new System.Net.WebClient()) {
                /*
                var data = new System.Collections.Specialized.NameValueCollection();
                data["username"] = "myUser";
                data["password"] = "myPassword";
                */
                var response = wb.UploadValues(remoteUrl, "POST", data);
                return Encoding.UTF8.GetString(response);
            }            
        }
        private string NowDt() {
            return DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }
        public void WriteLog(string txt, TypeLog typLogu) {
            txt = $"{NowDt()} {GetComputerName()} {GetUserName()} {txt}";
            try {
                if (typLogu == TypeLog.local || typLogu == TypeLog.both) WriteLogFile(txt, logFilePath);
                if (typLogu == TypeLog.remote || typLogu == TypeLog.both) WriteLogFile(txt, remoteLogPath);
            }
            catch {

            }
            
        }
        private string GetRnd() {
            Random rnd = new Random();
            return rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999);
        }
        public string GetComputerName() {
            return System.Environment.MachineName;
        }
        public string GetUserName() {
            return System.Environment.UserName;
        }
        public void WriteRemoteInfo(string widget, string value, bool postLog=true) {
            //  \\aavm2\data2\mlog_%random%%random%_%COMPUTERNAME%_%USERNAME%.txt
            if (postLog) {
                var data = new System.Collections.Specialized.NameValueCollection();
                data["comp"] = GetComputerName();
                data["user"] = GetUserName();
                data["opt"] = widget;
                data["val"] = value;
                data["dt"] = NowDt();
                string resp=PostLogRecord(data);
                WriteLog("remote info posted", TypeLog.local);
            }
            else {
                string outLog = $"{GetComputerName()};{GetUserName()};{widget};{value}";
                string fileName = $"{remoteInfoPath}mlog_{GetRnd()}_{GetComputerName()}_{GetUserName()}.txt";
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@fileName, true)) {
                    file.WriteLine(outLog);
                }
            }
            
        }


    }
}

