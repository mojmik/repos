using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladUdalostiPokrocily
{
    class DigiBudik : Budik
    {
        
        public DigiBudik()
        {
            parametryUdalosti = new ParametryUdalostiZvoneni();
            parametryUdalosti.Hlasitost = 10;
        }
    }
}
