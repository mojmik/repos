using System;
using System.Collections.Generic;

using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SlovickaSQL {
    class Slovicka {
        DbManager dbMan;
        public string[] colNames = { "English", "Czech"};
        public string tabName = "Word";
        public Slovicka(DbManager db) {
            dbMan = db;
        }
        public void DoAdd() {
            Console.WriteLine("Zadej nové slovíčko anglicky");
            string anglicky = Console.ReadLine();
            Console.WriteLine("Zadej nové slovíčko česky");
            string cesky = Console.ReadLine();
            string[] values = { anglicky, cesky };
            
            dbMan.DoInsert(tabName, values, colNames);
        }
        public void Vypis() {
            dbMan.PrintRows(tabName);            
        }
        public void DoDelete() {
            Console.WriteLine("Zadej anglické slovíčko, které chceš vymazat");
            string anglicky = Console.ReadLine();
            dbMan.DoDelete(tabName, colNames[0], anglicky);
        }
        public void DoUpdate() {
            Console.WriteLine("Zadej anglické slovíčko, u kterého chceš upravit překlad");
            string anglicky = Console.ReadLine();
            Console.WriteLine("Zadej nový překlad");
            string cesky = Console.ReadLine();
            dbMan.DoUpdate(tabName,anglicky, colNames[0], cesky,colNames[1]);
        }
    }
}
