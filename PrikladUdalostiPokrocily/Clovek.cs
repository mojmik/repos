using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladUdalostiPokrocily
{
    class Clovek
    {
         public virtual void Buzeni(object sender, ParametryUdalostiZvoneni par)
        {
            Console.WriteLine("clovek- jsem vzhuru");
        }
        public void PridejBudik(Budik b) => b.Zvoneni += Buzeni;
    }
}
