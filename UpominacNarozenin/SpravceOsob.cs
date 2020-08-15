using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using System.Text;

namespace UpominacNarozenin {
    public class SpravceOsob : INotifyPropertyChanged { 
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Osoba> Osoby { get; set; }
        private string cesta = "osoby.xml";
        public SpravceOsob() {
            Osoby = new ObservableCollection<Osoba>();            
        }

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
            //VyvolejZmenu("NejblizsiOsoba");
            //lepsi je tohle:
            VyvolejZmenu(nameof(NejblizsiOsoba));
        }

        public void Pridej(string jmeno, DateTime? datumNarozeni) {
            if (jmeno.Length < 3)
                throw new ArgumentException("Jméno je příliš krátké");
            if (datumNarozeni == null)
                throw new ArgumentException("Nebylo zadané datum narození");
            if (datumNarozeni.Value.Date > DateTime.Today)
                throw new ArgumentException("Datum narození nesmí být v budoucnosti");
            Osoba osoba = new Osoba(jmeno, datumNarozeni.Value.Date);
            Osoby.Add(osoba);
            NajdiNejblizsi();
        }
        public void Odeber(Osoba osoba) {
            Osoby.Remove(osoba);
            NajdiNejblizsi();
        }
        protected void VyvolejZmenu(string vlastnost) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(vlastnost));
        }
        public void Uloz() {
            XmlSerializer serializer = new XmlSerializer(Osoby.GetType());
            using (StreamWriter sw = new StreamWriter(cesta)) {
                serializer.Serialize(sw, Osoby);
            }
        }
        public void Nacti() {
            XmlSerializer serializer = new XmlSerializer(Osoby.GetType());
            if (File.Exists(cesta)) {
                using (StreamReader sr = new StreamReader(cesta)) {
                    Osoby = (ObservableCollection<Osoba>)serializer.Deserialize(sr);
                }
            }
            else
                Osoby = new ObservableCollection<Osoba>();
            NajdiNejblizsi();
        }
    }
}
