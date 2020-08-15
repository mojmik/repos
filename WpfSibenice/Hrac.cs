using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WpfSibenice {
    public class Hrac {
       
        public string Jmeno { get; set; }
        public string Skore { get; set; }

        public Hrac() {

        }
        public override string ToString() {
            return Jmeno + " pocet chyb" + Skore;
        }
    }
}
