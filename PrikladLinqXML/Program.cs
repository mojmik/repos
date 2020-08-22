using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PrikladLinqXML {
    class Program {
        static void Main(string[] args) {
            XDocument dokument = XDocument.Load("hw.xml");
            var dotaz = from p in dokument.Element("pocitace").Elements("pocitac").Elements("procesor")
                        select p;
            double a=0;
            int i = 0;
            double x = 0;
            foreach (var d in dotaz) {
                a = double.Parse(d.Element("frekvence").Value.Replace(" GHz", "").Replace(" MHz", "").Replace(".", ","));
                i = int.Parse(d.Element("jader").Value);
                x += a*i;                
            }
            
            Console.WriteLine($"celkova freq: {x}");

            double a2 = 0;
            var dotaz2 = from p in dokument.Element("pocitace").Elements("pocitac").Elements("ram")
                        select p;
            int n = 0;
            foreach (string d in dotaz2) {                
                string s=d.Replace(".", ",");
                if (s.Contains("GB")) {
                    a2 += double.Parse(d.Replace(" GB", ""))*1024;
                }
                else {
                    a2 += double.Parse(d.Replace(" MB", ""));
                }
                n++;
            }
            a2 = a2 / n;

            Console.WriteLine($"prumerna ram: {a2}");

            var dotaz3 = from p in dokument.Element("pocitace").Elements("pocitac")
                         select (p.Element("procesor").Element("jader").Value,p.Element("procesor").Element("frekvence").Value, p.Attribute("nazev").Value);

        

            double a3=0;
            double max = 0;
            string maxName = "";
            foreach ( (string d1,string d2,string d3) in dotaz3) {
                //a2 += double.Parse(d.Replace(" MB", "").Replace(" GB", "").Replace(".", ","));
                a3 = double.Parse(d2.Replace(" GHz", "").Replace(" MHz", "").Replace(".", ","))*(int.Parse(d1));
                if (max<a3) {
                    max = a3;
                    maxName = d3;
                }
            }
            Console.WriteLine($"max: {maxName}");

            Console.WriteLine("zmackni neco pro reseni ukazkovy");
            Console.ReadKey();
            
            //ukazkove reseni

            XDocument dokument2 = XDocument.Load("hw.xml");
            // Celkový výkon všech jader všech počítačů
            var vykony = from p in dokument2.Element("pocitace").Elements("pocitac").Elements("procesor")
                             // Vynásobíme počet jader a frekvenci
                         select int.Parse(p.Element("jader").Value) *
                                double.Parse(p.Element("frekvence").Value.ToLower().Replace("ghz", "").Trim(), CultureInfo.InvariantCulture);
            Console.WriteLine("Celkový součet výkonů všech jader všech počítačů je {0} GHz", vykony.Sum());

            // Průměrná hodnota operační paměti
            var velikosti = from p in dokument2.Element("pocitace").Elements("pocitac")
                                // Vynásobíme počet jader a frekvenci
                            select int.Parse(p.Element("ram").Value.ToLower().Replace("gb", "").Trim());
            Console.WriteLine("Průměrná velikost operační paměti je {0} GB", velikosti.Average());

            // Nejvýkonnější počítač
            var nejvykonnejsi = from p in dokument2.Element("pocitace").Elements("pocitac")
                                    // Vynásobíme počet jader a frekvenci
                                orderby (int.Parse(p.Element("procesor").Element("jader").Value) *
                                         double.Parse(p.Element("procesor").Element("frekvence").Value.ToLower().Replace("ghz", "").Trim(), CultureInfo.InvariantCulture) +
                                         // Frekvence GK
                                         double.Parse(p.Element("grafika").Element("frekvence").Value.ToLower().Replace("mhz", "").Trim(), CultureInfo.InvariantCulture) / 1000) descending
                                select p.Attribute("nazev").Value;
            Console.WriteLine("Nejvýkonnější počítač je {0}", nejvykonnejsi.First());
            Console.ReadKey();
        }
    }
}
