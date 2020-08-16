using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WpfSibenice {
    public class SpravceHracu {
        public List<Hrac> Hraci { get; set; } //pozor, kdyz tady neni get; set; tak ListBox nefunguje!
        string hraciPath = "hraci.xml";

        public SpravceHracu() {
            Hraci = new List<Hrac>();
            Nacti();
        }
                
        public void Pridej(string jmeno, string skore) {
            Hrac h = new Hrac();
            h.Jmeno = jmeno;
            h.Skore = skore;
            Hraci.Add(h);
            Uloz();
        }
        public void Uloz() {
            XmlSerializer ser = new XmlSerializer(Hraci.GetType());
            using (StreamWriter sw = new StreamWriter(hraciPath)) {
                ser.Serialize(sw, Hraci);
            }
        }
        public void Nacti() {
            if (File.Exists(hraciPath)) {
                XmlSerializer ser = new XmlSerializer(Hraci.GetType());

                using (StreamReader sr = new StreamReader(hraciPath)) {
                    Hraci = (List<Hrac>)ser.Deserialize(sr);
                }
            }
            Hraci.Sort();
        }
    }
}
