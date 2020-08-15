using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace WpfSibenice {
    class Slovo {
        public string TextSlova { get; set; }
        public int PocetPismen { get; private set; }
        char[] TextSlovaMasked;
        
        public Slovo(string slovo) {
            TextSlova = slovo;
            PocetPismen = slovo.Length;
            TextSlovaMasked = MaskujSlovo();
        }
        public override string ToString() {
            return Maskovane();
        }
        public string Maskovane() {
            string s="";
            foreach (char c in TextSlovaMasked) {
                s += c + " ";
            }
            return s;
        }
        public char[] MaskujSlovo() {
            char[] maskedSlovo = new char[PocetPismen];
            for (int n=0; n<PocetPismen; n++) {
                maskedSlovo[n] = '_';
            }
            return maskedSlovo;
        }
        public int OdmaskujSlovo(char pismeno) {
            int spravneTipy = 0;
            for (int n=0;n<PocetPismen;n++) {
                if (pismeno == TextSlova[n]) {
                    TextSlovaMasked[n] = pismeno;
                    spravneTipy++;
                }
                //TextSlovaMasked[n] = (pismeno == TextSlova[n]) ? pismeno : TextSlovaMasked[n];
            }
            return spravneTipy;
        }
    }
}
