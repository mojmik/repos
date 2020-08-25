using DbEntityFramework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace View {
    public class Manager {
        private Database1Entities db = new Database1Entities();
        private ObservableCollection<Person> persons = new ObservableCollection<Person>();
        private ObservableCollection<Invoice> invoices = new ObservableCollection<Invoice>();

        public ObservableCollection<Person> Persons { get { return persons; } }
        public ObservableCollection<Invoice> Invoices { get { return invoices; } }


        public Manager() {
            db.Person.ToList().ForEach(p => persons.Add(p));
            db.Invoice.ToList().ForEach(i => invoices.Add(i));
        }
    }
}
