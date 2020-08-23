using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Registry_UAC {
    static class UAC {
        const UInt32 BCM_SETSHIELD = 0x160C;

        [DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int SendMessage(IntPtr hWnd, UInt32 Msg, int wParam, IntPtr lParam);
        static void Nacti() {
            SendMessage((IntPtr)656444, BCM_SETSHIELD, 0, (IntPtr)1);
        }
        public static void RestartujSPozadavkemNaZvyseniPrav() {
            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = true;
            info.WorkingDirectory = Environment.CurrentDirectory;
            info.FileName = Environment.GetCommandLineArgs()[0];
            info.Verb = "runas";

            try {
                Process.Start(info);
            }
            catch {

            }
            Process.GetCurrentProcess().Kill();
        }
    }
}
