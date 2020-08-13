using System;
using System.Collections.Generic;
using System.Text;

namespace PrikladNaradi
{
    class Kladivo : Naradi
    {
        public bool Obourucni { get; set; }

        public Kladivo(string nazev,int vaha,bool obourucni=false)
        {
            Nazev = nazev;
            Vaha = vaha;
            Obourucni = obourucni;
        }

        public override string Pracuj()
        {
            return "Zatloukam";
        }
    }
}
