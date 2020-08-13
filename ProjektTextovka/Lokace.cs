using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektTextovka
{
    public class Lokace
    {
        public string jmeno;
        public string popis;
        public IDictionary<string,Lokace> okolniLokace=new Dictionary<string,Lokace>();
        public Lokace(string jmeno, string popis)
        {
            this.jmeno = jmeno;
            this.popis = popis;
        }
        public void pridejOkolniLokace(Lokace s, Lokace j, Lokace v, Lokace z)
        {
            okolniLokace["s"] = s;
            okolniLokace["j"] = j;
            okolniLokace["v"] = v;
            okolniLokace["z"] = z;
        }
        public Lokace JdiNa(string kam)
        {
           return okolniLokace[kam];
        }
        public override string ToString()
        {
            return string.Format("{0} \n {1}", jmeno, popis);
        }
    }
}
