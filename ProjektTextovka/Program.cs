using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektTextovka
{
    class Program
    {
        static void Main(string[] args)
        {
            bool konecHry = false;
            string prikaz;
            List<Lokace> seznamLokaci = new List<Lokace>();
            seznamLokaci.Add(new Lokace("Hrad", "Stojíš před okovanou branou gotického hradu, která je zřejmě jediným vchodem do pevnosti. Klíčová dírka je pokryta pavučinami, což vzbuzuje dojem, že je budova opuštěná."));
            seznamLokaci.Add(new Lokace("Les", "Jsi na lesní cestě, která se klikatí až za obzor, kde mizí v siluetě zapadajícího slunce. Ticho podvečerního lesa občas přeruší zpěv posledních ptáků."));
            seznamLokaci.Add(new Lokace("Lesní rozcestí", "Nacházíš se na lesním rozcestí."));
            seznamLokaci.Add(new Lokace("Les", "Jsi na lesní cestě, která se klikatí až za obzor, kde mizí v siluetě zapadajícího slunce. Ticho podvečerního lesa občas přeruší zpěv posledních ptáků."));
            seznamLokaci.Add(new Lokace("Rybník", "Došel jsi ke břehu malého rybníka. Hladina je v bezvětří jako zrcadlo. Kousek od tebe je dřevěná plošina se stavidlem."));
            seznamLokaci.Add(new Lokace("Les", "Jsi na lesní cestě, která se klikatí až za obzor, kde mizí v siluetě zapadajícího slunce. Ticho podvečerního lesa občas přeruší zpěv posledních ptáků."));
            seznamLokaci.Add(new Lokace("Dům", "Stojíš před svým rodným domem, cítíš vůni čerstvě nasekaného dřeva, která se line z hromady vedle vstupních dveří."));
            seznamLokaci[0].pridejOkolniLokace(null, null, seznamLokaci[1], null);
            seznamLokaci[1].pridejOkolniLokace(null, null, seznamLokaci[2], seznamLokaci[0]);
            seznamLokaci[2].pridejOkolniLokace(null, seznamLokaci[6], seznamLokaci[3], seznamLokaci[2]);
            seznamLokaci[3].pridejOkolniLokace(null, null, seznamLokaci[4], seznamLokaci[2]);
            seznamLokaci[4].pridejOkolniLokace(null, null, null, seznamLokaci[3]);
            seznamLokaci[5].pridejOkolniLokace(seznamLokaci[2], null, seznamLokaci[6], null);
            seznamLokaci[6].pridejOkolniLokace(null, null, null, seznamLokaci[5]);
            Lokace aktLokace = seznamLokaci.Last();

            while (!konecHry)
            {
                Console.WriteLine(aktLokace.ToString());
                prikaz = Console.ReadLine();
                if (prikaz == "konec") konecHry = true;
                else if (prikaz.StartsWith("jdi na "))
                {
                    string kam = prikaz.Replace("jdi na ", "");
                    Lokace newLokace = aktLokace.JdiNa(kam);
                    if (newLokace == null) Console.WriteLine("nelze");
                    else aktLokace = newLokace;

                }
                else Console.WriteLine("error");
            }   
        }
    }
}
