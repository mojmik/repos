using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FoeHelper2
{
    class FoeTools
    {
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

        public FoeTools(MainWindow w)
        {
            window = w;            
            string[] seq = { "2B1708", "B78642", "8D4B1D" };
            string[] seqEnd = { "904C1D", "935321", "935321", "261608" };

            string[] sipkaSeqStart = { "160D06", "757E90", "4B566E", "4B566E", "7E869A" };
            string[] sipkaSeqEnd = { "757C8D", "414B60", "4C576E", "4C576E", "160E07" };

            
            
            
        }
        public int initScreen()
        {
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
            int roztecX2k = 666;
            int roztecY2k = 113;            
            int roztecX = p2.X - p1.X; 
            int roztecY = p2.Y - p1.Y;
            ratioX = ((float)roztecX / (float)roztecX2k);
            ratioY = ((float)roztecY / (float)roztecY2k);
            MouseTools.MoveMouse(p1.X, p1.Y, 0, 0);
            MouseTools.MoveMouse(p2.X, p2.Y, 0, 0);            
            
            return 1;
        }
        private int[,] applyAspect(int[,] coords)
        {
            for (int n = 0; n < coords.GetLength(0); n++)
            {
                for (int y = 0; y < coords.GetLength(1); y++)
                {
                    if (y==0) coords[n, y] = (int) (coords[n, y] * ratioX);
                    if (y == 1) coords[n, y] = (int) (coords[n, y] * ratioY);
                }
            }
            return coords;
        }
        public void goHelp()
        {
            window.Mout = "neco";
            
            //int[,,] coords = new int[5, 1, 1];
            //coords ={ { 2,2},{ 3,3} };
            int[,] coordsKnajpa = { {384,1000}, { 500, 1000 }, {619,1000 }, { 734, 1000 },{ 853,1000} };
            coordsKnajpa = applyAspect(coordsKnajpa);
            int[,] coordsKnajpaCheck = { { 391, 992 }, { 509, 993 }, { 627, 994}, { 745, 994 }, { 860, 994 } };
            coordsKnajpaCheck = applyAspect(coordsKnajpaCheck);
            string knajpaCheck = "94845D";
            

            //MouseTools.MoveMouse(helpButtonStart.X, helpButtonStart.Y, 0, 0);            
            //MouseTools.MoveMouse(helpButtonEnd.X, helpButtonEnd.Y, 0, 0);
            
            for (int x = 0; x < 5;x++)
            {
                string check1 = PixelTools.GetColorAt(coordsKnajpaCheck[x, 0], coordsKnajpaCheck[x, 1]);
                bool debug = true;
                if (knajpaCheck == check1 || debug)
                {
                    MouseTools.MoveMouse(coordsKnajpa[x, 0], coordsKnajpa[x, 1], 0, 0);
                    Thread.Sleep(shortWait);
                }
            }
        }
    }
}
