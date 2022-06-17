using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace keybso {
     

        class Program {
            static void Main(string[] args) {
                string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
                string onScreenKeyboardPath = System.IO.Path.Combine(progFiles, "TabTip.exe");
                Process.Start(onScreenKeyboardPath);
            Thread.Sleep(2000);

                var uiHostNoLaunch = new UIHostNoLaunch();
                var tipInvocation = (ITipInvocation)uiHostNoLaunch;
                tipInvocation.Toggle(GetDesktopWindow());
                Marshal.ReleaseComObject(uiHostNoLaunch);

            /*
            Process[] oskProcessArray = Process.GetProcessesByName("TabTip");
            foreach (Process onscreenProcess in oskProcessArray) {
                onscreenProcess.Kill();
                onscreenProcess.Dispose();
            }
            */
        }

            [ComImport, Guid("4ce576fa-83dc-4F88-951c-9d0782b4e376")]
            class UIHostNoLaunch {
            }

            [ComImport, Guid("37c994e7-432b-4834-a2f7-dce1f13b834b")]
            [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
            interface ITipInvocation {
                void Toggle(IntPtr hwnd);
            }

            [DllImport("user32.dll", SetLastError = false)]
            static extern IntPtr GetDesktopWindow();

         
        }    
}
