using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektUdalostiEasy
{
    class Budik
    {
        public event EventHandler Zvoneni;

        protected void PriBuzeni(EventArgs e)
        {            
                Zvoneni(this, e);
        }

        public void Zazvon()
        {
            Console.WriteLine("zvonim");
            PriBuzeni(EventArgs.Empty);
            
            
        }
    }
}
