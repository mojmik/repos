using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektUdalostiEasy
{
    class Program
    {
        static void Main(string[] args)
        {
            Clovek c = new Clovek();
            Budik b = new Budik();
            c.pridejBudik(b);
            b.Zazvon();
            Console.ReadKey();
        }
    }
}
