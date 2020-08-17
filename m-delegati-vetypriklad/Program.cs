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
        public delegate string OperaceMik(string s);

        public static string metodaString(string s) {
            return s.ToLower();
        }
        
        static void Main(string[] args)
        {
            String sInit = "Delegát v C# .NET funguje jako odkaz na metodu.";
            
            Veta v = new Veta("Delegát v C# .NET funguje jako odkaz na metodu.");

            //varianta0 - pojmenovany delegat pripravena metoda
            OperaceMik opMik0 = metodaString;
            Console.WriteLine(opMik0("AHOJ 0"));

            //varianta1 - pojmenovany delegat nova metoda
            OperaceMik opMik1 = nejakejString => {
                return nejakejString.ToLower();
            };
            Console.WriteLine(opMik1("AHOJ 1"));

            //varianta2 - pojmenovany delegat nova metoda zkracena lambda
            OperaceMik opMik2 = nejakejString => nejakejString.ToLower();
            Console.WriteLine(opMik2("AHOJ 2"));

            //varianta3 - predpripraveny delegat nova metoda zkracena lambda
            Func<string, string> opMik3 = nejakejString => nejakejString.ToLower();
            Console.WriteLine(opMik3("AHOJ 3"));

            //varianta4 - predpripraveny delegat nova metoda lambda
            Func<string, string> opMik4 = nejakejString => {
                return nejakejString.ToLower();
            };
            Console.WriteLine(opMik4("AHOJ 4"));


            /*
            Veta v = new Veta("Delegát v C# .NET funguje jako odkaz na metodu.");
            v.Mapuj(s => s.ToUpper());
            Console.WriteLine(v);
            */


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
            v.Mapuj(operace3);
            Console.WriteLine(v);

            v.InitZeStringu(sInit);
            Func<string, string> operaceTest = s => {
                if (s.Length > 5) return s.ToUpper();
                else return s;
            };
            v.Mapuj(operace3);


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
