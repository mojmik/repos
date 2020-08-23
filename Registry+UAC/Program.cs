using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Registry_UAC {
    class Program {
        static void Main(string[] args) {
            string cestaKaplikaci = @"J:\mik\prg\repos\WpfZIPaXMLzamestnanci\bin\Debug\WpfZIPaXMLzamestnanci.exe";
            RegistracePriponSouboru zamestnanecPripona = new RegistracePriponSouboru("zamestnanec", cestaKaplikaci);
            zamestnanecPripona.PripravIkonu();
            if (!zamestnanecPripona.JeZaregistrovana()) {
                zamestnanecPripona.Registruj();
            }
        }
    }
}
