using System;
using System.Collections.Generic;
using System.Text;

namespace UpominacNarozenin {
    public class Osoba {
        public string Jmeno { get; set; }
        public DateTime Narozeniny { get; set; }
        public Osoba() {

        }
        public Osoba(string jmeno, DateTime narozeniny) {
            Jmeno = jmeno;
            Narozeniny = narozeniny;
        }
        public int Vek {
            get {
                DateTime dnes = DateTime.Today;
                int vek = dnes.Year - Narozeniny.Year;
                if (dnes < Narozeniny.AddYears(vek))
                    vek--;
                return vek;
            }
        }
        public int ZbyvaDni {
            get {
                DateTime dnes = DateTime.Today;
                DateTime dalsiNarozeniny = Narozeniny.AddYears(Vek + 1);

                TimeSpan rozdil = dalsiNarozeniny - DateTime.Today;

                return Convert.ToInt32(rozdil.TotalDays);
            }
        }
        public override string ToString() {
            return Jmeno;
        }
    }
}
