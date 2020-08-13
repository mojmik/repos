using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladReferencePokrocily
{
    class Program
    {
        static void Main(string[] args)
        {
            ClenRodiny abr = new ClenRodiny("Abraham Simpson", null, null);
            ClenRodiny pen = new ClenRodiny("Penelope Olsen", null, null);
            ClenRodiny pan = new ClenRodiny("Pan Bouvier", null, null);
            ClenRodiny jac = new ClenRodiny("Jackie Bouvier", null, null);
            ClenRodiny her = new ClenRodiny("Herb Powers", abr, pen);
            ClenRodiny hom = new ClenRodiny("Homer Simpson", abr, pen);
            ClenRodiny mar = new ClenRodiny("Marge Bouvier", pan, jac);
            ClenRodiny sel = new ClenRodiny("Selma Bouvier", pan, jac);
            ClenRodiny bar = new ClenRodiny("Bart", hom, mar);
            Console.WriteLine("Rodokmen bart: " + bar.ToString());
            Console.WriteLine("Rodokmen homie: " + hom.ToString());
            Console.ReadKey();
        }
    }
}
