using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladNarozeniny
{
    class Narozeniny
    {
        public DateTime DenNarozenin;
        public Narozeniny(string dt)
        {

            DenNarozenin =  DateTime.Parse(dt);
        }
        public override string ToString()
        {
            return DenNarozenin.ToString();
        }
        public double ZaKolikDni()
        {
            DateTime dnes = DateTime.Now;
            DateTime pristiNarozky = new DateTime(dnes.Year, DenNarozenin.Month, DenNarozenin.Day);
            if (pristiNarozky < dnes) pristiNarozky = pristiNarozky.AddYears(1);
            TimeSpan vek = pristiNarozky - dnes;
            Console.WriteLine("narozky tento rok: "+ pristiNarozky + " dnes: " + dnes);
            
            return (vek).TotalDays;
        }
        public int Vek()
        {
            int roku=0;
            DateTime dnes = DateTime.Now;
            DateTime den = DenNarozenin;
            roku = dnes.Year - den.Year;
            if ((dnes.Month < DenNarozenin.Month) || ((dnes.Month == DenNarozenin.Month) && (dnes.Day < DenNarozenin.Day))) roku--;
            return roku;
        }
    }
}
