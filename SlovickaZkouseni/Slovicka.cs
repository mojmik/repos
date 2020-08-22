using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace SlovickaZkouseni {
    class Slovicka {
        Dictionary<string, string> slovicka = new Dictionary<string, string>();
        public string info="";

        public Slovicka() {
            Init();      
        }
        private void Init() {
            CSVtools csvTools = new CSVtools();
            string[,] csvAr;
            try {
                csvAr = csvTools.ReadCSVarray("slovicka.csv", new char[] { ';' });
                for (int y = 0; y < csvTools.Rows; y++) {
                    for (int x = 0; x < csvTools.Cols; x++) {
                        if (x != 0) info += ",";
                        slovicka[csvAr[y, 0]] = csvAr[y, 1];
                    }
                    
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());

            }
        }
        
        public List<string> GetRandomPair() {
            Random r = new Random();
            List<string> outList = new List<string>();
            //var txt = slovicka.Skip(r.Next(slovicka.Count)).Take(1);
            var txt = (from s in slovicka select(s.Key,s.Value)).Skip(r.Next(slovicka.Count)).Take(1).ToArray();
            var txt2 = (from s in slovicka select (s.Key, s.Value)).Skip(r.Next(slovicka.Count)).First();
            outList.Add(txt2.Key);
            outList.AddRange(txt2.Value.Split(','));          
            return outList;
        }

        private void InfoArray() {
            CSVtools csvTools = new CSVtools();
            string[,] csvAr;
            try {
                csvAr = csvTools.ReadCSVarray("slovicka.csv", new char[] { ';' });
                for (int y = 0; y < csvTools.Rows; y++) {
                    for (int x = 0; x < csvTools.Cols; x++) {
                        if (x != 0) info += ",";
                        info += csvAr[y, x];
                    }
                    info += "\n";
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());

            }
        }
        private void Repair() {
            CSVtools csvTools = new CSVtools();
            csvTools.RepairCSV("slovicka.csv", "slovicka.csv", "    ", ";");   
        }
    }
}
