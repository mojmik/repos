using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfDataGrid {
    public enum Bydliste { Praha, Brno, Ostrava, Liberec };
    class Osoba {
        public int Id { get; set; }
        public string Jmeno { get; set; }
        public string Prijmeni { get; set; }
        public int Vek { get; set; }
        
        public Bydliste Bydliste { get; set; }
        private static int posledni = 0;
       

        public Osoba(string jmeno, string prijmeni, int vek, Bydliste bydliste) {
            Jmeno = jmeno;
            Prijmeni = prijmeni;
            Vek = vek;
            Bydliste = bydliste;
            Id = posledni;
            posledni++;
        }
    }
}
