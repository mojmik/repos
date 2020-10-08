using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace mCompWarden2 {
    class Logger {
        string logFile,remoteLogPath;
        public string UserName { get; set; }
        public string ComputerName { get; set; }

        public Logger() {
            logFile = @"C:\it\mcompwarden.txt";
            remoteLogPath = @"\\aavm2\data2\";
        }
        public void WriteLog(string txt) {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@logFile, true)) {
                file.WriteLine(txt);
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
        public void WriteRemoteInfo(string widget, string value) {
            //  \\aavm2\data2\mlog_%random%%random%_%COMPUTERNAME%_%USERNAME%.txt

            string outLog = $"{ComputerName};{UserName};{widget};{value}";
            string fileName = $"{remoteLogPath}mlog_{GetRnd()}_{GetComputerName()}_{GetUserName()}.txt";
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@fileName, true)) {
                    file.WriteLine(outLog);
            }
        }
        public void WriteRemoteLog(string logTxt) {
            string fileName = @"" + Program.mainPath + "mainlog.txt";
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@fileName, true)) {
                file.WriteLine(logTxt);
            }
        }

    }
}

