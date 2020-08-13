using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VetyPriklad
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Veta v = new Veta("Delegát v C# .NET funguje jako odkaz na metodu.");
            v.Mapuj(s => s.ToUpper());
            Console.WriteLine(v);
            */
            String sInit = "Delegát v C# .NET funguje jako odkaz na metodu.";
            Veta v = new Veta("Delegát v C# .NET funguje jako odkaz na metodu.");
            
            //all caps
            /*
            Func<string, string> operace2 = s => {
                return s.ToUpper();                
            };
            
            v.Mapuj(operace2);
            Console.WriteLine(v);
            */

            //caps vetsi nez 5 pismen
            Func<string, string> operace3 = s => {
                if (s.Length > 5) return s.ToUpper();
                else return s;
            };

            //v.InitZeStringu(sInit);
            v.Mapuj(operace3);
            Console.WriteLine(v);
            
            Console.WriteLine("Agregáty:");

            //oddelena mezerou
            //v.InitZeStringu(sInit);
            Console.WriteLine(v.Agreguj((a, b) => a + " " + b));

            //ozavorkovana 
            //v.InitZeStringu(sInit);
            Console.WriteLine(v.Agreguj((a, b) => "(" + a + " " + b + ")"));

            //zkratky
            //caps vetsi nez 5 pismen
            Func<string, string, string> operaceZkratky = (s, s2) => {
                if (s2.Length > 4) return s + " " + s2.Substring(0, 3) + ".";
                else return s + " " + s2;
            };
            //v.InitZeStringu(sInit);
            Console.WriteLine(v.Agreguj(operaceZkratky));
            /*
            

            v.Mapuj(Func<string, string> operace = s => {
                s.ToUpper();
                return s;
            }
            );
            */

            //s pomlckama
            Veta v2 = new Veta("Delegát v C# .NET funguje jako odkaz na metodu.");
            Console.WriteLine(v2.Agreguj((a, b) => a + "-" + b));
            Console.ReadKey();
        }
    }
}
