using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlovickaZkouseni {
    class ZkouseniSlovicek {
        Slovicka slovicka = new Slovicka();
        Random r = new Random();
        Hrac h;
        List<string> aktualniZkouska;
        int kolikMax = 5;
        int kolikAkt = 0;

        public ZkouseniSlovicek() {
            h = new Hrac();
        }
       
        public void Losuj() {
            kolikAkt++;
            aktualniZkouska=slovicka.GetRandomPair();
        }
        public void zapisVysledky() {            
            List<string> vysledky = new List<string>();
            vysledky.Add($"{h.Jmeno};{h.Skore}");
            CSVtools csvTools = new CSVtools();
            try {
                csvTools.CreateCSV(vysledky, "vysledky.csv");
            }
            catch (IOException e) {
                //osetreni vyjmky
            }
        }
        public bool isKonec() {
            if (kolikAkt >kolikMax) {
                return true;
            }
            return false;
            //zapsani skore
        }
        public string VypisSkore() {
            return "Skore: " + h.Skore;
        }
    
        public string VypisOri() {
            return aktualniZkouska[0];
        }
        public string VypisPreklad() {
            string s="";
            for (int n = 1; n < aktualniZkouska.Count; n++) {
                if (n > 1) s += ",";
                s += aktualniZkouska[n];
            }
            return s;
        }
        public bool CheckPreklad(string slovo) {
            string moznyPreklad;
            for (int n=1;n<aktualniZkouska.Count;n++) {
                moznyPreklad = aktualniZkouska[n];
                if (RemoveDiacritics(aktualniZkouska[n]) == RemoveDiacritics(slovo)) return true;
            }
            return false;
        }
        public string RemoveDiacritics(string s) {
            // oddělení znaků od modifikátorů (háčků, čárek, atd.)
            s = s.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < s.Length; i++) {
                // do řetězce přidá všechny znaky kromě modifikátorů
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(s[i]) != System.Globalization.UnicodeCategory.NonSpacingMark) {
                    sb.Append(s[i]);
                }
            }

            // vrátí řetězec bez diakritiky
            return sb.ToString();
        }
    }
   
}
