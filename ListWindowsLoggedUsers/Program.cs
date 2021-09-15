using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace ListWindowsLoggedUsers {
    class Program {
        private static string getUserNames() {
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
            return String.Join(",",userList.ToArray());
        }

        static void Main(string[] args) {
            Console.WriteLine(getUserNames());
            Console.ReadLine();
        }
    }
}
