using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace PrikladEshop
{
    class Objednavka : IOrder
    {
        int id;
        Zakaznik z;
        Produkt p;
        Adresa a;

        public Objednavka(int id, Produkt p, Zakaznik z, Adresa a)
        {
            this.id = id;
            this.p = p;
            this.z = z;
            this.a = a;
            this.Number = id;
            this.FirstName = z.jmeno;
            this.LastName = z.prijmeni;
            this.Prices = new double[] { p.cena };
            this.Quantities = new int[] { 1 };
            this.Products = new string[] { p.nazev };
            this.HouseNumber = a.cp;
            this.Zip = a.psc;
        }

        public int Number { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public string Street { get; private set; }

        public int HouseNumber { get; private set; }

        public int RegistryNumber { get; private set; }

        public string City { get; private set; }

        public string Zip { get; private set; }

        public string Country { get; private set; }

        public string[] Products { get; private set; }

        public int[] Quantities { get; private set; }

        public double[] Prices { get; private set; }
    }
}
