using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoeHelper2
{
    class ColorCheck
    {
        string colorStr;
        double precision;
        bool positive;
        int[] line;
        public ColorCheck(string col, double prec, bool posit = true)
        {
            colorStr = col;
            precision = prec;
            positive = posit;
        }
        public ColorCheck(int xStart, int xEnd, int y) {
            /*
            line = new int[xEnd - xStart];
            for (int x=xStart;x< xEnd; x++) {
                line[x-xStart] = PixelTools.GetColorIntAt(x, y);
            }
            */
            line = PixelTools.GetHorizontalLineColors2(xStart, xEnd, y);
        }
        public bool Check(int x,int y)
        {
            return (positive == PixelTools.CompareColors(colorStr, PixelTools.GetColorAt(x, y),precision));
        }
        public bool CheckLine(ColorCheck colCheck2) {
            for (int x=0;x<line.Length;x++) {
                if (line[x] != colCheck2.line[x]) return false;
            }
            return true;
        }
    }
}
