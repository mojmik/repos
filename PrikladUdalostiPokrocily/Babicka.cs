using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladUdalostiPokrocily
{
    class Babicka : Clovek
    {
        public override void Buzeni(object sender, ParametryUdalostiZvoneni par)
        {
            if (par.Hlasitost > 1)
            {
                Console.WriteLine("babicka - jsem vzhuru");
            }
        }
    }
}
