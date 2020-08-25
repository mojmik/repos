using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlovickaSQL {
    /// <summary>
    /// connection stringu muzeme vzit z SQL Server Object Exploreru-> properties, nebo kodem StringBuilderem
    /// Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = master; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = False; ApplicationIntent = ReadWrite; MultiSubnetFailover = False
    /// </summary>
    class Program {
        static void Main(string[] args) {
            DbManager dbMan = new DbManager();
            Slovicka slov = new Slovicka(dbMan);
            //dbMan.TestConnection();
            dbMan.OpenConnection();
            slov.Vypis();            
            string command = "";           

            while (command!="end") {
                Console.WriteLine("Co chces? pridej, vypis, update, delete, end?");
                command = Console.ReadLine();
                if (command == "vypis") slov.Vypis();
                if (command == "update") slov.DoUpdate();
                if (command == "delete") slov.DoDelete();
                if (command == "pridej") slov.DoAdd();
            }
            dbMan.CloseConnection();
        }
    }
}

