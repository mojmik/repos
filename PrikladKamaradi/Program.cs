using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladKamaradi
{
    class Program
    {
        static void Main(string[] args)
        {
            Clovek c = new Clovek("Karel Novák");            
            c.vek = 36;
            Clovek d = new Clovek("Josef Nový");
            c.Skamaradit(d);
            d.Skamaradit(c);
            d.vek = 38;
            Console.WriteLine(c);
            Console.WriteLine(d);
            Console.ReadKey();
        }
    }
}
