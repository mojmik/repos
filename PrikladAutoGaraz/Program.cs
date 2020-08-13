using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladAutoGaraz
{
    class Program
    {
        static void Main(string[] args)
        {
            Auto a = new Auto("123ABC");
            Garaz g = new Garaz();
            a.Zaparkuj(g);
            Console.WriteLine(g.parkovaneAuta);
            Console.ReadKey();

        }
    }
}
