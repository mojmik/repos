using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladEshop
{
    class Program
    {
        static void Main(string[] args)
        {
            Zakaznik zakaznik = new Zakaznik(1, "Tomáš", "Marný");
            Adresa adresa = new Adresa("Ve Svahu", 10, 2, "Praha", "10000");
            Produkt produkt = new Produkt("Body pro ITnetwork.cz", "Body pro zpřístupnění prémiového obsahu a dosažení vašeho vysněného zaměstnání v IT.", 239);
            Objednavka objednavka = new Objednavka(1, produkt, zakaznik, adresa);
            // Vytvoření brány a předání objednávky bráně
            Gateway brana = new Gateway();
            brana.ProcessOrder(objednavka);

            Console.ReadKey();
        }
    }
}
