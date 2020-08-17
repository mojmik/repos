using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfNakupniSeznam {
    public class Polozka {        
        
        public string Nazev { get; set; } //setter musi byt public, jinak by neslo deserializovat

        Polozka() {

        }

        public Polozka(string nazev) {
            Nazev = nazev;
        }

        public override string ToString() {
            return Nazev;
        }
    }
}
