using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrikladSeznamka
{
    public class Uzivatel
    {
        public String Jmeno { get; set;  }
        public String Prijmeni { get; set; }
        public DateTime DatumNarozeni { get; set; }
        public enum Pohlavi { Muz, Zena };

        public Pohlavi pohlavicko;

        public enum BarvaOci
        {
            Neuvedena=0,
            Modra=1,
            Zelena=2,
            Hneda=4,
            Cerna=8,
            Modrozelena=Modra | Zelena,
            Hnedozelena=Hneda | Zelena
        }

        public Uzivatel(String jm, String prij,DateTime nar,Pohlavi p,BarvaOci o,BarvaVlasu v)
        {
            this.Jmeno = jm;
            this.Prijmeni = prij;
            this.DatumNarozeni = nar;
            this.pohlavicko = p;
            this.oci = o;
            this.vlasy = v;
        }

        public BarvaOci oci;

        public enum BarvaVlasu { Neuvedena, Blond, Hneda, Cerna, Zrzava };
        public BarvaVlasu vlasy;

        public override bool Equals(object obj)
        {
            return (obj is Uzivatel && (this == (Uzivatel)obj));
        }
        public static bool operator ==(Uzivatel a, Uzivatel b)
        {
            return ((a.Jmeno == b.Jmeno) && (a.Prijmeni == b.Prijmeni) && (a.DatumNarozeni == b.DatumNarozeni) && (a.pohlavicko == b.pohlavicko) && (a.oci==b.oci) && (a.vlasy == b.vlasy));
        }
        public static bool operator !=(Uzivatel a, Uzivatel b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return Jmeno.GetHashCode() ^ Prijmeni.GetHashCode() ^ DatumNarozeni.GetHashCode() ^ pohlavicko.GetHashCode() ^ oci.GetHashCode() ^ vlasy.GetHashCode();
        }
        public override string ToString()
        {
            //return this.Jmeno + " " + this.Prijmeni + " " + this.DatumNarozeni.Year + " oci:" + this.oci + " vlasy:" + this.vlasy;
            return String.Format("{0} {1} {2} oci:{3} vlasy:{4} ",Jmeno,Prijmeni,DatumNarozeni.Year,oci,vlasy);
        }
    }
}
