using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladUdalostiStredniAuto
{
    public class Auto
    {
        private int rychlost;

        public void Jed(int rychlost)
        {
            Console.WriteLine("jedu");
            this.rychlost = rychlost;
        }
        public void Stuj(object sender, EventArgs e)
        {
            if (sender is Semafor)
            {
                if ( ((Semafor)sender).Sviti == Semafor.Svetlo.Cervena)
                {
                    this.rychlost = 0;
                    Console.WriteLine("stojim");
                    return;
                }
            }
            Console.WriteLine("jedu");
        }
        public void pridejSemafor(Semafor s)
        {
            s.Ehandler += Stuj;

        }

    }
}
