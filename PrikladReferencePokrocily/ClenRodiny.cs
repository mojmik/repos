using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladReferencePokrocily
{
    public class ClenRodiny
    {
        public ClenRodiny matka;
        public ClenRodiny otec;
        public string jmeno;
        public ClenRodiny(string jmeno, ClenRodiny otec, ClenRodiny matka)
        {
            this.jmeno = jmeno;
            this.matka = matka;
            this.otec = otec;
        }
        public override string ToString()
        {            
            return "" + this.jmeno + "" + (otec is null ? "" : "\n" + otec.ToString()) + "" + (matka is null ? "" : "\n" + matka.ToString());
        }
    }
}
