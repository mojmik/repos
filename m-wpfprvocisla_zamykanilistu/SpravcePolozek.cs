using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WpfNakupniSeznam {
    class SpravcePolozek  {
        // get; set; je tam dulezity, jinak by to nebyla vlastnost, ale jen atribut a nefungoval by listbox
        public ObservableCollection<Polozka> Polozky { get; set; } = new ObservableCollection<Polozka>();
        private string cesta = "polozky.xml";

        public SpravcePolozek() {
            Nacti();
        }

        public void Pridat(string nazev) {
            Polozky.Add(new Polozka(nazev));
            Uloz();
        }
        public void Odebrat(Polozka p) {
            Polozky.Remove(p);
            Uloz();
        }
        public void Uloz() {
            XmlSerializer ser = new XmlSerializer(Polozky.GetType());
            using (StreamWriter sw = new StreamWriter(cesta)) {
                ser.Serialize(sw, Polozky);
            }
        }
        public void Nacti() {
            XmlSerializer ser = new XmlSerializer(Polozky.GetType());
            if (File.Exists(cesta)) {
                using (StreamReader sr = new StreamReader(cesta)) {
                    Polozky = (ObservableCollection<Polozka>)ser.Deserialize(sr);
                }
            }
        }
    }
}
