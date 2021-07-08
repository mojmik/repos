using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace FoeHelper2
{
    static class PixelTools
    {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        static int maxX=Screen.PrimaryScreen.Bounds.Width;
        static int maxY=Screen.PrimaryScreen.Bounds.Height;
        static Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        static Bitmap wholeScreen = new Bitmap(maxX, maxY, PixelFormat.Format32bppArgb);
        static Graphics mGraph;
        static int[,] mScreenColors;
        public static string GetColorAt(int x, int y)
        {
            using (Graphics gdest = Graphics.FromImage(screenPixel))
            {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero))
                {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, x, y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return ColorTranslator.ToHtml(screenPixel.GetPixel(0, 0));
        }
        public static void CaptureScreen()
        {
            mGraph = Graphics.FromImage(wholeScreen as Image);
            mGraph.CopyFromScreen(0, 0, 0, 0, wholeScreen.Size);
            mScreenColors = new int[maxX,maxY];
            int x = 0;
            int y = 0;
            for (y = 0; y < maxY; y++)
            {
                for (x = 0; x < maxX; x++)
                {
                    Color mColor = wholeScreen.GetPixel(x, y);
                    mScreenColors[x, y] = mColor.ToArgb();
                }
            }
        }

        public static Point? FindPixelSequence(string[] seq)
        {
            
            int x = 0;
            int y = 0;            
            int color;
            int seqPos = 0;
            int seqSize = seq.Length;

            int[] colorSeq = new int[seq.Length];
            for (x=0;x<seq.Length;x++)
            {
                colorSeq[x]= ColorTranslator.FromHtml("#"+seq[x]).ToArgb();
            }
            
            for (y = 0; y < maxY; y++)
            {
                for (x = 0; x < maxX;x++)
                {
                    color=mScreenColors[x, y];
                    if (colorSeq[seqPos] ==color)
                    {
                        if (seqPos + 1 == seqSize) return new Point(x, y);
                        seqPos++;
                    } else
                    {
                        seqPos = 0;
                        if (colorSeq[seqPos] == color) seqPos = 1;
                    }
                }
            }

            return null;
        }

        private static string GetPixelAtCoordsSlow(int x, int y)
        {
            Bitmap mScr = new Bitmap(1, 1);
            Graphics mGraph = Graphics.FromImage(mScr as Image);
            //mGraph.CopyFromScreen(x, y, x+1, y+1, mScr.Size);
            mGraph.CopyFromScreen(x, y, x, y, mScr.Size);
            Color mColor = mScr.GetPixel(x, y);
            return ColorTranslator.ToHtml(mColor);
        }
    }
}
