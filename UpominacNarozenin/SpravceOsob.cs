using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace UpominacNarozenin {
    public class SpravceOsob {
        public ObservableCollection<Osoba> Osoby { get; set; }
        public Osoba NejblizsiOsoba { get; set; }

        public DateTime DnesniDatum {
            get {
                return DateTime.Now;
            }
        }

        private void NajdiNejblizsi() {
            var serazeneOsoby = Osoby.OrderBy(o => o.ZbyvaDni);
            if (serazeneOsoby.Count() > 0)
                NejblizsiOsoba = serazeneOsoby.First();
            else
                NejblizsiOsoba = null;
        }
    }
}
