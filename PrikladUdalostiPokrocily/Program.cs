using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladUdalostiPokrocily
{
    class Program
    {
        static void Main(string[] args)
        {
            Babicka b = new Babicka();
            Clovek c = new Clovek();
            Budik budik = new Budik();
            DigiBudik dbudik = new DigiBudik();
            c.PridejBudik(budik);
            c.PridejBudik(dbudik);
            b.PridejBudik(budik);
            b.PridejBudik(dbudik);
            budik.Zazvon();
            dbudik.Zazvon();
            Console.ReadKey();
        }
    }
}
