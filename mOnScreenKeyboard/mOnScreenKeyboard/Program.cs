using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace mOnScreenKeyboard {

    
    static class Program {

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, String lpszClass, String lpszWindow);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern bool PostMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		/// <summary>
		/// Show the On Screen Keyboard
		/// </summary>
		#region ShowOSK
		public static void ShowOnScreenKeyboard() {
			string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
			string onScreenKeyboardPath = System.IO.Path.Combine(progFiles, "TabTip.exe");
			Process.Start(onScreenKeyboardPath);
		}
		#endregion ShowOSK

		/// <summary>
		/// Hide the On Screen Keyboard
		/// </summary>
		#region HideOSK
		public static void HideOnScreenKeyboard() {

			Process[] oskProcessArray = Process.GetProcessesByName("TabTip");
			foreach (Process onscreenProcess in oskProcessArray) {
				onscreenProcess.Kill();
				onscreenProcess.Dispose();
			}

			uint WM_SYSCOMMAND = 0x0112;
			UIntPtr SC_CLOSE = new UIntPtr(0xF060);
			IntPtr y = new IntPtr(0);
			IntPtr KeyboardWnd = FindWindow("IPTip_Main_Window", null);
			PostMessage(KeyboardWnd, WM_SYSCOMMAND, SC_CLOSE, y);
		}
		#endregion HideOSK
		static void Main(string[] args) {
			if (args.Length == 0) {
				HideOnScreenKeyboard();
				ShowOnScreenKeyboard();
				Console.WriteLine("show");
			} else {
				if (args[0] == "close") {
					HideOnScreenKeyboard();
					Console.WriteLine("hide");
				}
            }
			


		}
    }
}
