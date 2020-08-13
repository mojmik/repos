using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladNarozeniny
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Zadej datum narozenin: ");
            string datum=Console.ReadLine();
            Narozeniny nar = new Narozeniny(datum);
            Console.WriteLine("narozky: " + nar + " za " + nar.ZaKolikDni()+"vek: "+nar.Vek());
            Console.ReadKey();
        }
    }
}
