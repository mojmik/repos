using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladKamaradi
{
    public class Clovek
    {
        public String Jmeno { get; set; }
        public int vek;
        public Clovek kamarad;
        public Clovek(String jmeno)
        {
            Jmeno = jmeno;
        }
        public void Skamaradit(Clovek c)
        {
            this.kamarad = c;
        }
        public override string ToString()
        {
            string s = "";
            //s = String.Format("{0} {1} {2}", Jmeno, vek, kamarad);
            //s = Jmeno + " " + vek + " " + kamarad;
            s = Jmeno + " " + vek + " " + kamarad.Jmeno;
            return s;
        }
    }
}
