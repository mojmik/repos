using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ColorMine;
using System.Diagnostics;
using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;

namespace FoeHelper2 {
    static class PixelTools {
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);
        static int maxX = Screen.PrimaryScreen.Bounds.Width;
        static int maxY = Screen.PrimaryScreen.Bounds.Height;
        static Bitmap screenPixel = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
        static Bitmap wholeScreen = new Bitmap(maxX, maxY, PixelFormat.Format32bppArgb);
        static Graphics mGraph;
        static int[,] mScreenColors;
        public static int StrColorToInt(string color) {
            if (!color.Substring(0, 1).Equals("#")) color = "#" + color;
            return ColorTranslator.FromHtml(color).ToArgb();
        }
        public static string GetColorAt(int x, int y) {
            using (Graphics gdest = Graphics.FromImage(screenPixel)) {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero)) {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, x, y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }

            return ColorTranslator.ToHtml(screenPixel.GetPixel(0, 0));
        }
        public static int GetColorIntAt(int x, int y) {
            using (Graphics gdest = Graphics.FromImage(screenPixel)) {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero)) {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDC = gdest.GetHdc();
                    int retval = BitBlt(hDC, 0, 0, 1, 1, hSrcDC, x, y, (int)CopyPixelOperation.SourceCopy);
                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            return screenPixel.GetPixel(0, 0).ToArgb();
        }
        public static int[] GetHorizontalLineColors(int xStart, int xEnd, int y) {
            int[] line = new int[xEnd - xStart];
            Bitmap lineBmp = new Bitmap(xEnd - xStart, 1, PixelFormat.Format32bppArgb);
            using (Graphics gdest = Graphics.FromImage(lineBmp)) {
                using (Graphics gsrc = Graphics.FromHwnd(IntPtr.Zero)) {
                    IntPtr hSrcDC = gsrc.GetHdc();
                    IntPtr hDestDC = gdest.GetHdc();
                    int retval = BitBlt(hDestDC, 0, 0, xEnd - xStart, 1, hSrcDC, xStart, y, (int)CopyPixelOperation.SourceCopy);
                    for (int x = xStart; x < xEnd; x++) {
                        int cislo = lineBmp.GetPixel(665, 0).ToArgb();
                        line[x - xStart] = lineBmp.GetPixel(x - xStart, 0).ToArgb();
                    }

                    gdest.ReleaseHdc();
                    gsrc.ReleaseHdc();
                }
            }
            return line;
        }
        public static int[] GetHorizontalLineColors2(int xStart, int xEnd, int y) {
            int[] line = new int[xEnd - xStart];
            Bitmap lineBmp = new Bitmap(xEnd - xStart, 1, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(lineBmp);
            var size = new Size(xEnd - xStart, 1);
            g.CopyFromScreen(xStart, y, 0, 0, size, CopyPixelOperation.SourceCopy);
            mScreenColors = new int[xEnd - xStart, 1];            
            for (int n = 0; n < xEnd - xStart; n++) {
                Color mColor = lineBmp.GetPixel(n, 0);
                line[n] = mColor.ToArgb();
            }
            return line;
        }
        public static void CaptureScreen() {
            mGraph = Graphics.FromImage(wholeScreen as Image);
            mGraph.CopyFromScreen(0, 0, 0, 0, wholeScreen.Size);
            mScreenColors = new int[maxX, maxY];
            int x = 0;
            int y = 0;
            for (y = 0; y < maxY; y++) {
                for (x = 0; x < maxX; x++) {
                    Color mColor = wholeScreen.GetPixel(x, y);
                    mScreenColors[x, y] = mColor.ToArgb();
                }
            }
        }

        public static Point? FindPixelSequence(string[] seq) {

            int x = 0;
            int y = 0;
            int color;
            int seqPos = 0;
            int seqSize = seq.Length;

            int[] colorSeq = new int[seq.Length];
            for (x = 0; x < seq.Length; x++) {
                colorSeq[x] = StrColorToInt(seq[x]);
            }

            for (y = 0; y < maxY; y++) {
                for (x = 0; x < maxX; x++) {
                    color = mScreenColors[x, y];
                    if (colorSeq[seqPos] == color) {
                        if (seqPos + 1 == seqSize) return new Point(x, y);
                        seqPos++;
                    }
                    else {
                        seqPos = 0;
                        if (colorSeq[seqPos] == color) seqPos = 1;
                    }
                }
            }

            return null;
        }
        public static bool CompareColors(string color1, string color2, double precision = 0) {
            if (!color1.Substring(0, 1).Equals("#")) color1 = "#" + color1;
            if (!color1.Substring(0, 1).Equals("#")) color2 = "#" + color2;
            if (precision == 0) {
                if (color1.Equals(color2)) return true;
            }
            else {
                int r = int.Parse(color1.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(color1.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(color1.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                var myRgb = new Rgb { R = r, G = g, B = b };
                r = int.Parse(color2.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                g = int.Parse(color2.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                b = int.Parse(color2.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                var myRgb2 = new Rgb { R = r, G = g, B = b };
                double deltaE = myRgb.Compare(myRgb2, new Cie1976Comparison());
                if (deltaE < precision) return true;
                return false;
            }

            return false;
        }

        private static string GetPixelAtCoordsSlow(int x, int y) {
            Bitmap mScr = new Bitmap(1, 1);
            Graphics mGraph = Graphics.FromImage(mScr as Image);
            //mGraph.CopyFromScreen(x, y, x+1, y+1, mScr.Size);
            mGraph.CopyFromScreen(x, y, x, y, mScr.Size);
            Color mColor = mScr.GetPixel(x, y);
            return ColorTranslator.ToHtml(mColor);
        }
    }
}
