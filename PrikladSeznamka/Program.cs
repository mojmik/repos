using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladSeznamka
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Uzivatel> uzivatele=new List<Uzivatel>();            

            Uzivatel a= new Uzivatel("Karel", "Novák", new DateTime(1958,6,25),Uzivatel.Pohlavi.Muz, Uzivatel.BarvaOci.Zelena, Uzivatel.BarvaVlasu.Blond);
            uzivatele.Add(a);
            uzivatele.Add(new Uzivatel("Josef", "Nový", new DateTime(1978, 3, 19), Uzivatel.Pohlavi.Muz, Uzivatel.BarvaOci.Hneda, Uzivatel.BarvaVlasu.Hneda));
            uzivatele.Add(new Uzivatel("Jan", "Marek", new DateTime(1968, 4, 28), Uzivatel.Pohlavi.Muz, Uzivatel.BarvaOci.Cerna & Uzivatel.BarvaOci.Hneda, Uzivatel.BarvaVlasu.Neuvedena));
            uzivatele.Add(new Uzivatel("Karel", "Novák", new DateTime(1958, 6, 25), Uzivatel.Pohlavi.Muz, Uzivatel.BarvaOci.Zelena, Uzivatel.BarvaVlasu.Blond));
            uzivatele.Add(new Uzivatel("Marie", "Nová", new DateTime(1988, 8, 15), Uzivatel.Pohlavi.Zena, Uzivatel.BarvaOci.Modra, Uzivatel.BarvaVlasu.Cerna));
            uzivatele.Add(new Uzivatel("Věra", "Nováková", new DateTime(1978, 10, 2), Uzivatel.Pohlavi.Zena, Uzivatel.BarvaOci.Modrozelena, Uzivatel.BarvaVlasu.Blond));
            uzivatele.Add(new Uzivatel("Simona", "Mladá", new DateTime(1968, 1, 8), Uzivatel.Pohlavi.Zena, Uzivatel.BarvaOci.Neuvedena, Uzivatel.BarvaVlasu.Hneda));
            uzivatele.Add(new Uzivatel("Michaela", "Marná", new DateTime(1958, 12, 6), Uzivatel.Pohlavi.Zena, Uzivatel.BarvaOci.Modra, Uzivatel.BarvaVlasu.Zrzava));
            uzivatele = uzivatele.Distinct().ToList();                            
            foreach (Uzivatel u in uzivatele)
            {
                Console.WriteLine(u.ToString());
            }
            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}
