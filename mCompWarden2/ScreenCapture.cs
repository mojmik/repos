using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace mCompWarden2 {
    class ScreenCapture {
        public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hObjectSource,
            int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(
              IntPtr hdcDest,      // handle to destination DC
              int nXOriginDest, // x-coord of destination upper-left corner
              int nYOriginDest, // y-coord of destination upper-left corner
              int nWidthDest,   // width of destination rectangle
              int nHeightDest,  // height of destination rectangle
              IntPtr hdcSrc,       // handle to source DC
              int nXOriginSrc,  // x-coord of source upper-left corner
              int nYOriginSrc,  // y-coord of source upper-left corner
              int nWidthSrc,    // width of source rectangle
              int nHeightSrc,   // height of source rectangle
              int dwRop       // raster operation code
            );


        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
            int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);



        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);

        public static void SaveScreenShot(string filePath) {
            /*
            Rectangle rc = new Rectangle();
            Image img = ScreenToImage(ref rc, false);
            img.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
            img.Dispose();
            */

            Bitmap bmp = TakingScreenshotEx2();
            bmp.Save(filePath, ImageFormat.Png);
        }


        private static Image ScreenToImage(ref Rectangle rcDest, bool IsPapaer) {
            IntPtr desktophWnd = GetDesktopWindow();
            
            // get te hDC of the target window
            IntPtr hdcSrc = GetWindowDC(desktophWnd);
            // get the size
            RECT windowRect = new RECT();
            GetWindowRect(desktophWnd, ref windowRect);
            int nWidth = windowRect.right - windowRect.left;
            int nHeight = windowRect.bottom - windowRect.top;

            if (IsPapaer) {
                float fRate = (float)rcDest.Width / nWidth;
                //float fHeight = nHeight * fRate;
                //rcDest.Height = (int)(nHeight * fRate);
                //rcDest.Width = (int)(rcDest.Width);// * fRate);
                rcDest.X = 0;
                rcDest.Y = 0;
                rcDest.Height = (int)(nHeight * fRate);
                //rcDest.Width = (int)(nWidth * fRate);
            }
            else {
                rcDest.X = 0;
                rcDest.Y = 0;
                rcDest.Height = nHeight;
                rcDest.Width = nWidth;
            }

            // create a device context we can copy to
            IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, rcDest.Width, rcDest.Height);
            // select the bitmap object
            IntPtr hOld = SelectObject(hdcDest, hBitmap);
            // bitblt over
            StretchBlt(hdcDest, rcDest.X, rcDest.Y, rcDest.Width, rcDest.Height, hdcSrc, 0, 0, nWidth, nHeight, SRCCOPY);
            // restore selection
            SelectObject(hdcDest, hOld);
            // clean up 
            DeleteDC(hdcDest);
            ReleaseDC(desktophWnd, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            DeleteObject(hBitmap);
            return img;
        }

        public class WinAPI {
            [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            public static extern IntPtr BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

            [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
            public static extern IntPtr GetDesktopWindow();
        }

        private static Bitmap TakingScreenshotEx2() {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            Bitmap bmpScreenshot = new Bitmap(screenWidth, screenHeight);
            Graphics g = Graphics.FromImage(bmpScreenshot);

            IntPtr dc1 = WinAPI.GetDC(WinAPI.GetDesktopWindow());
            IntPtr dc2 = g.GetHdc();

            //Main drawing, copies the screen to the bitmap
            //last number is the copy constant
            WinAPI.BitBlt(dc2, 0, 0, screenWidth, screenHeight, dc1, 0, 0, 13369376);

            //Clean up
            ReleaseDC(WinAPI.GetDesktopWindow(), dc1);
            g.ReleaseHdc(dc2);
            g.Dispose();

            return bmpScreenshot;
        }
    }
}
