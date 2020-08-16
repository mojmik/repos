using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace WpfSibenice {
    public class Hrac : IComparable {
       
        public string Jmeno { get; set; }
        public string Skore { get; set; }

        public Hrac() {

        }
        public override string ToString() {
            return Jmeno + " pocet chyb" + Skore;
        }
        public int CompareTo(object obj) {
            Hrac h = (Hrac)obj;
            
            return string.Compare(this.Jmeno,h.Jmeno);
        }
    }
}
