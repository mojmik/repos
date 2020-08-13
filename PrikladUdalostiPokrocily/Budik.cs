using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladUdalostiPokrocily
{
    public class Budik
    {
        public event EventHandler<ParametryUdalostiZvoneni> Zvoneni;
        public ParametryUdalostiZvoneni parametryUdalosti;

        public Budik()
        {
            this.parametryUdalosti = new ParametryUdalostiZvoneni();
            this.parametryUdalosti.Hlasitost = 1;            
        }
        protected void PriBuzeni(ParametryUdalostiZvoneni e)
        {
            Zvoneni?.Invoke(this, e);
        } 
        public void Zazvon()
        {
            Console.WriteLine(this.GetType() + " zvonim");
            PriBuzeni(this.parametryUdalosti);
        }
    }
}
