using System;
using System.Collections.Generic;
using System.Text;

namespace PrikladNaradi
{
    class Pila : Naradi
    {
        public Pila(string nazev, int vaha)
        {
            Nazev = nazev;
            Vaha = vaha;
        }
        public override string Pracuj()
        {
            return "Rezu";
        }
    }
}
