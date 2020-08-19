using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDataGrid {
    class SpravceOsob {

        public ObservableCollection<Osoba> osoby = new ObservableCollection<Osoba>();

        public SpravceOsob() {
            osoby.Add(new Osoba("Jan", "Novák", 31, Bydliste.Praha));
            osoby.Add(new Osoba("Jana", "Nová", 22, Bydliste.Brno));
            osoby.Add(new Osoba("Josef", "Nový", 17, Bydliste.Liberec));
            osoby.Add(new Osoba("Tomáš", "Marný", 42, Bydliste.Ostrava));
            osoby.Add(new Osoba("Ota", "Veselý", 31, Bydliste.Praha));
        }

    }
}
