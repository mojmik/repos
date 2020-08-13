using System;
using System.Collections.Generic;
using System.Text;

namespace PrikladEshop
{
    class Zakaznik
    {
        public string jmeno;
        public string prijmeni;
        public int id;
        public Zakaznik(int id, string jmeno, string prijmeni)
        {
            this.jmeno = jmeno;
            this.id = id;
            this.prijmeni = prijmeni;

        }
    }
}
