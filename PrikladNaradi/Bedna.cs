using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrikladNaradi
{
    class Bedna
    {
        int maxNosnost;
        int aktNosnost;
        List<Naradi> naradi = new List<Naradi>();

        /*
        public IEnumerable<Naradi> Naradi
        {
            get { return naradi.ToArray(); }
        }
        */
        public Array GetNaradi()
        {
            return naradi.ToArray();
        }

        public Bedna(int nosnost)
        {
            maxNosnost = nosnost;

        }
        public void VlozNaradi(Naradi n)
        {
            if (n.Vaha + aktNosnost < maxNosnost)
            {
                naradi.Add(n);
                aktNosnost += n.Vaha;
            }
            else Console.WriteLine($"kapacita prekrocena aktnosnost: {aktNosnost}, nejde vlozit: {n.Nazev}");
        }
        public void VypisNaradi()
        {
            foreach (Naradi n in naradi)
            {
                if (!naradi.First().Equals(n)) Console.WriteLine($", {n.Nazev}");
                else Console.WriteLine($"{n.Nazev}");
            }
        }
        public void VahaKladiv()
        {
            int vaha = 0;
            foreach (Naradi n in naradi)
            {
                if (n is Kladivo)
                {
                    if (((Kladivo)n).Obourucni) vaha += n.Vaha;
                    
                }                
            }
            Console.WriteLine($"Vaha obourucnich kladiv: {vaha}");
        }
    }
}
