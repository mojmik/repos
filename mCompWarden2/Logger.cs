using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;

namespace mCompWarden2 {
    public static class Logger {
        public static string logFilePath, remoteLogPath, remoteInfoPath,remoteUrl;
        public enum TypeLog { local, remote, both }
       
        private static void WriteLogFile(string txt, string filePath) {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filePath, true)) {
                file.WriteLine(txt);
            }
        }
        private static string PostLogRecord(System.Collections.Specialized.NameValueCollection data) {
            using (var wb = new System.Net.WebClient()) {
                /*
                var data = new System.Collections.Specialized.NameValueCollection();
                data["username"] = "myUser";
                data["password"] = "myPassword";
                */
                try {
                    var response = wb.UploadValues(remoteUrl, "POST", data);
                    return Encoding.UTF8.GetString(response);
                } catch {
                    return "upload failed";
                }
                
                
            }            
        }
        private static string NowDt() {
            string dt=DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            dt = dt.Replace(". ", "-");
            return dt;
        }
        public static void WriteLog(string txt, TypeLog typLogu) {
            txt = $"{NowDt()} {GetComputerName()} {GetUserName()} {txt}";
            try {
                if (typLogu == TypeLog.local || typLogu == TypeLog.both) WriteLogFile(txt, logFilePath);
                if (typLogu == TypeLog.remote || typLogu == TypeLog.both) WriteLogFile(txt, remoteLogPath);
            }
            catch {

            }
            
        }
        private static string GetRnd() {
            Random rnd = new Random();
            return rnd.Next(10000, 99999) + "-" + rnd.Next(10000, 99999);
        }
        public static string GetComputerName() {
            return System.Environment.MachineName;
        }
        private static string GetUserNames() {
            List<string> userList = new List<string>();
            //SelectQuery query = new SelectQuery(@"Select * from Win32_Process");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "Select * from Win32_Process")) {
                foreach (System.Management.ManagementObject Process in searcher.Get()) {
                    if (Process["ExecutablePath"] != null &&
                        string.Equals(Path.GetFileName(Process["ExecutablePath"].ToString()), "explorer.exe", StringComparison.OrdinalIgnoreCase)) {
                        string[] OwnerInfo = new string[2];
                        Process.InvokeMethod("GetOwner", (object[])OwnerInfo);
                        userList.Add(OwnerInfo[0]);
                    }
                }
            }
            return String.Join("-", userList.ToArray());
        }
        public static string GetUserName() {
            return System.Environment.UserName;
        }
        public static void WriteRemoteInfo(string widget, string value, bool postLog=true) {
            //  \\aavm2\data2\mlog_%random%%random%_%COMPUTERNAME%_%USERNAME%.txt
            if (postLog) {
                var data = new System.Collections.Specialized.NameValueCollection();
                data["comp"] = GetComputerName();
                data["user"] = GetUserNames();
                data["opt"] = widget;
                data["val"] = value;
                data["dt"] = NowDt();
                string resp=PostLogRecord(data);
                //WriteLog("remote info posted " + resp, TypeLog.local);
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

