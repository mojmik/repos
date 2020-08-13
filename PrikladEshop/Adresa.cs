using System;
using System.Collections.Generic;
using System.Text;

namespace PrikladEshop
{
    class Adresa
    {
        public string ulice, mesto, psc;
        public int cp, co;

        public Adresa(string ulice, int cp, int co, string mesto, string psc)
        {
            this.ulice = ulice;
            this.cp = cp;
            this.co = co;
            this.mesto = mesto;
            this.psc = psc;
        }
    }
}
