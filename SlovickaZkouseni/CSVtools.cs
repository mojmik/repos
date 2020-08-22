using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Windows.Shapes;
using System.Xml;

namespace SlovickaZkouseni {
    class CSVtools {
        
        List<string> lines;
        public int Rows { get; set; }
        public int Cols { get; set; }
        public CSVtools() {
            Rows = 0;
            Cols = 0;
        }
        public void ReadLines(string path) {
            string txt;
            lines = new List<string>(); ;
            using (StreamReader sr = new StreamReader(path)) {
                while ((txt = sr.ReadLine()) != null)
                    lines.Add(txt);
            }

        }
        public string[,] ReadCSVarray(string path, char[] delimiter) {
            List<List<string>> csvLines = new List<List<string>>();
            string[,] outCSV;

            string s;
            List<string> l;

            using (StreamReader sr = new StreamReader(path)) {
                while ((s= sr.ReadLine()) != null) {                
                    l = new List<string>(s.Split(delimiter));
                    csvLines.Add(l);
                    Rows++;
                    if (Cols < l.Count()) Cols = l.Count();
                }
            }
            outCSV = new string[Rows, Cols];
            int y = 0;
            int x = 0;
            foreach (List<string> a in csvLines) {
                x = 0;
                foreach (string t in a) {
                    outCSV[y, x] = t;
                    x++;
                }                
                y++;
            }
            return outCSV;
        }
        public void RepairCSV(string path, string newpath, string oriDelim, string newDelim) {
            List<string> newLines = new List<string>();
            ReadLines(path);

            foreach (string line in lines) {
                newLines.Add(line.Replace(oriDelim, newDelim));
            }
            lines = newLines;
            WriteCSV(newpath);
        }
        public void WriteCSV(string path) {
            using (StreamWriter sw = new StreamWriter(path)) {
                foreach (string s in lines) {
                    sw.WriteLine(s);
                }
                sw.Flush();
            }
        }
        public void CreateCSV(List<string> lines,string path) {
            this.lines = lines;
            WriteCSV(path);
        }
    }

}
