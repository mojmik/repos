using System;
using System.Collections.Generic;
using System.Text;

namespace PrikladEshop
{
    class Produkt
    {
        public string nazev, popis;
        public int cena;
        public Produkt(string nazev, string popis,int cena)
        {
            this.nazev = nazev;
            this.popis = popis;
            this.cena = cena;
        }
    }
}
