using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace mWinColors {
    class WinColors {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetSysColors(int cElements, int[] lpaElements, int[] lpaRgbValues);
        public const int COLOR_DESKTOP = 1;

        public void SetColor() {
            //example color
            System.Drawing.Color sampleColor = System.Drawing.Color.Lime;

            //array of elements to change
            int[] elements = { 15 };
            Random rnd = new Random();

            //array of corresponding colors
            int[] colors = { System.Drawing.ColorTranslator.ToWin32(sampleColor) };
            int[] mColors = { 0, 0, 0 };
            //set the desktop color using p/invoke
            for (int n=1;n<30;n++) {
                elements[0] = n;
                mColors[0] = rnd.Next(0,50) + 256* rnd.Next(0, 255) + 256*256*rnd.Next(0, 255);
                mColors[1] = rnd.Next(0,255);
                mColors[2] = rnd.Next(0,255);
                
                Console.WriteLine(elements[0] + " elem " + mColors[0] + " " + mColors[1] + " " + mColors[2]);
                SetSysColors(elements.Length, elements, mColors);
                Thread.Sleep(100);
            }
            
            bool writeReg = false;
            if (writeReg) {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Control Panel\\Colors", true);
                key.SetValue(@"Background", string.Format("{0} {1} {2}", sampleColor.R, sampleColor.G, sampleColor.B));
            }
            //save value in registry so that it will persist
            
        }

        
    }
}
