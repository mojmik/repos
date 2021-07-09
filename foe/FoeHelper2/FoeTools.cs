using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoeHelper2 {
    class FoeTools {
        int maxResX = 3840;
        int maxResY = 2100;
        int shortWait = 100;
        int longWait = 500;
        List<Point> points;
        Point p1;
        Point p2;
        public string foeOut;
        int status;
        MainWindow window;
        float ratioX, ratioY;

        public FoeTools(MainWindow w) {
            window = w;
            string[] seq = { "2B1708", "B78642", "8D4B1D" };
            string[] seqEnd = { "904C1D", "935321", "935321", "261608" };

            string[] sipkaSeqStart = { "160D06", "757E90", "4B566E", "4B566E", "7E869A" };
            string[] sipkaSeqEnd = { "757C8D", "414B60", "4C576E", "4C576E", "160E07" };




        }
        public int initScreen() {
            window.Mout = "Init screen start";
            string[] cudlikPrvni = { "1D100C", "716E68" };
            string[] cudlikPosledni = { "412411", "362621" };
            Object o;
            PixelTools.CaptureScreen();
            o = PixelTools.FindPixelSequence(cudlikPrvni);
            if (o == null) return 0;
            p1 = (Point)o;
            o = PixelTools.FindPixelSequence(cudlikPosledni);
            if (o == null) return 0;
            p2 = (Point)o;
            status = 1;
            //porovname velikost helper baru, takhle se pozna zooming
            int roztecX2k = 666;
            int roztecY2k = 113;
            int roztecX = p2.X - p1.X;
            int roztecY = p2.Y - p1.Y;
            ratioX = ((float)roztecX / (float)roztecX2k);
            ratioY = ((float)roztecY / (float)roztecY2k);

            //porovname pozici cudliku, takhle se pozna res
            int spodniCudlikX2k = 909;
            int spodniCudlikY2k = 1029;
            ratioX = ((float)spodniCudlikX2k / (float)p2.X);
            ratioY = ((float)spodniCudlikY2k / (float)p2.Y);

            MouseTools.MoveMouse(p1.X, p1.Y, 0, 0);
            MouseTools.MoveMouse(p2.X, p2.Y, 0, 0);
            window.Mout = "Init screen done";
            return 1;
        }
        private int[,] applyAspect(int[,] coords) {
            for (int n = 0; n < coords.GetLength(0); n++) {
                for (int y = 0; y < coords.GetLength(1); y++) {
                    if (y == 0) coords[n, y] = (int)(coords[n, y] * ratioX);
                    if (y == 1) coords[n, y] = (int)(coords[n, y] * ratioY);
                }
            }
            return coords;
        }
        private int[] applyAspect(int[] coords) {
            coords[0] = (int)(coords[0] * ratioX);
            coords[1] = (int)(coords[1] * ratioY);
            return coords;
        }
        private void klikRightHelperBar() {
            int[] coordsSipka = { 912, 974 };
            coordsSipka = applyAspect(coordsSipka);
            MouseTools.ClickMouse(coordsSipka);
            Wait(2000);
        }
        private void Wait(int howLong = 0) {
            if (howLong == 0) howLong = shortWait;
            Thread.Sleep(howLong);
        }
        public void goHelp() {
            window.Mout = "neco";

            //int[,,] coords = new int[5, 1, 1];
            //coords ={ { 2,2},{ 3,3} };
            int[,] coordsKnajpa = { { 350, 1003 }, { 457, 1003 }, { 563, 1003 }, { 669, 1003 }, { 776, 1003 } };
            coordsKnajpa = applyAspect(coordsKnajpa);
            int[,] coordsKnajpaCheck = { { 356, 998 }, { 462, 999 }, { 570, 998 }, { 677, 998 }, { 785, 997 } };
            coordsKnajpaCheck = applyAspect(coordsKnajpaCheck);
            string knajpaCheck = "94845D";

            int[,] coordsHelp = { { 285, 1028 }, { 384, 1028 }, { 496, 1028 }, { 605, 1028 }, { 710, 1028 } };
            coordsHelp = applyAspect(coordsHelp);
            ColorCheck helpCheck = new ColorCheck("331C0A", 1.2, false);
            //string[] helpCheck = { "331C0A", ""



            //MouseTools.MoveMouse(helpButtonStart.X, helpButtonStart.Y, 0, 0);            
            //MouseTools.MoveMouse(helpButtonEnd.X, helpButtonEnd.Y, 0, 0);
            bool debug = false;

            //klikej na sipku, dokud se meni obrazky
            bool picsChanged = true;
            while (picsChanged) {
                for (int n = 0; n < 5; n++) {
                    //help
                    if (helpCheck.Check(coordsHelp[n, 0], coordsHelp[n, 1])) window.Mout += " help " + n + "ok";
                    else window.Mout += " help " + n + "not ok";

                    //knajpa
                    string check1 = PixelTools.GetColorAt(coordsKnajpaCheck[n, 0], coordsKnajpaCheck[n, 1]);
                    if (PixelTools.CompareColors(knajpaCheck, check1) || debug) {
                        MouseTools.MoveMouse(coordsKnajpa[n, 0], coordsKnajpa[n, 1], 0, 0);
                        window.Mout += " tavern" + n + " ok";
                        Wait();
                    }
                    else {
                        window.Mout += " tavern" + n + " not ok";
                    }
                }
                //uloz si caru
                ColorCheck lineCheck = new ColorCheck(263, 896, 971);
                //klikni na sipku
                klikRightHelperBar();
                ColorCheck lineCheck2 = new ColorCheck(263, 896, 971);
                if (lineCheck.CheckLine(lineCheck2)) picsChanged = false;
            }
            window.Mout = "End of help bar reached";
        }
    }
}
