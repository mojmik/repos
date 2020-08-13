using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladAutoGaraz
{
    public class Auto
    {
        string spz;
        public Auto(string spz)
        {
            this.spz = spz;
        }
        public void Zaparkuj(Garaz g)
        {
            g.parkovaneAuta = this.spz;
        }

    }
}
