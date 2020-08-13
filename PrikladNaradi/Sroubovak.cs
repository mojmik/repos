using System;
using System.Collections.Generic;
using System.Text;

namespace PrikladNaradi
{
    class Sroubovak : Naradi
    {
        public Sroubovak(string nazev, int vaha)
        {
            Nazev = nazev;
            Vaha = vaha;        
        }

        public override string Pracuj()
        {
            return "Sroubuju";
        }
    }

    class ElektrickySroubovak : Sroubovak
    {
        public int KapacitaBaterie { get; private set; }

        public ElektrickySroubovak(string nazev, int vaha, int kap) : base(nazev,vaha)
        {
            Nazev = nazev;
            Vaha = vaha;
            KapacitaBaterie = kap;
        }
     
    }
}
