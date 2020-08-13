using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektUdalostiEasy
{
    class Clovek
    {
        public void VzbudSe(object sender, EventArgs e)
        {
            Console.WriteLine("Jsem vzhuru");
        }
        public void pridejBudik(Budik b)
        {
            b.Zvoneni += VzbudSe;

        }
    }
}
