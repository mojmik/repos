using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbEntityFramework {
    class Program {
        static void Main(string[] args) {
            Database1Entities db = new Database1Entities(); //Database1, protoze se ta databaze jmenuje Database1

            Person newPerson = new Person();
            newPerson.Name = "Jméno";
            newPerson.Surname = "Příjmení";
            newPerson.Street = "Ulice";
            newPerson.City = "Město";
            newPerson.PSC = 73601;
            newPerson.ICO = 78321456;
            newPerson.DIC = "CZ78321456";
            newPerson.Email = "email@email.cz";

            db.Person.Add(newPerson);
            db.SaveChanges();

            Console.WriteLine("Persons: ");
            foreach (Person p in db.Person) {
                Console.WriteLine(p.Surname + " " + p.Name + ", city: " + p.City);
            }
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Invoices: ");
            foreach (Invoice i in db.Invoice) {
                Console.WriteLine("Price: " + i.Price + ", date: " + i.Date);
            }
            Console.WriteLine(Environment.NewLine + "Search Pepa: ");

            Person foundPerson = db.Person.FirstOrDefault(p => p.Name.Contains("pepa"));
            Console.WriteLine(foundPerson.Surname + " " + foundPerson.Name + ", city: " + foundPerson.City);

            Console.ReadKey();
        }
    }
}
