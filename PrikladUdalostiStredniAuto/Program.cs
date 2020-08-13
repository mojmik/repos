using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladUdalostiStredniAuto
{
    class Program
    {
        static void Main(string[] args)
        {
            Semafor s = new Semafor();
            Auto a = new Auto();
            a.Jed(100);
            a.pridejSemafor(s);
            s.prepniSvetlo(Semafor.Svetlo.Oranzova);
            s.prepniSvetlo(Semafor.Svetlo.Cervena);
            Console.ReadKey();
        }
    }
}
