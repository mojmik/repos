using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace mActiveOutlook {
    class Program {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetActiveWindowTitle() {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0) {
                return Buff.ToString();
            }
            return null;
        }
        public static void WriteFile(string fileName,string text) {         
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(fileName, true)) {
                file.WriteLine(text);
            }
        }
        static void Main(string[] args) {
            DateTime today = DateTime.Today;
            string windowTitle;
            string installFile = @"\\rentex.intra\company\hertz_czsk\Company CZ & SK\msignatures\makra\logs\install-" + System.Environment.UserName.ToLower() +
                "-" + System.Environment.MachineName.ToLower() + "-log.txt";
            string makraOk = @"\\rentex.intra\company\hertz_czsk\Company CZ & SK\msignatures\makra\logs\makraok-" + System.Environment.UserName.ToLower() +
                "-" + System.Environment.MachineName.ToLower() + "";
            //if (File.Exists(installFile)) return; //tohle neni uplne spolehlivy

            if (File.Exists(makraOk)) {
                DateTime lastModified = System.IO.File.GetLastWriteTime(makraOk);
                
                DateTime lastMonth = today.AddDays(-30);
                if (lastModified > lastMonth) return;
            }
            

            for (; ; ) {
                windowTitle = GetActiveWindowTitle();                
                if (windowTitle != null && windowTitle.Contains("- Outlook") && windowTitle.Contains("hertz")) {
                    Thread.Sleep(5000);
                    SendKeys.SendWait("%{F11}");
                    SendKeys.SendWait("%{F4}");
                    WriteFile(installFile,"c# install end"+today.ToString());
                    return;
                }
                Thread.Sleep(1000);
            }
        }
    }
}
