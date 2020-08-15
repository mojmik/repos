using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace WpfSibenice {
    class SpravceHry : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        List<Slovo> SlovaList = new List<Slovo>();
        List<char> Tipy = new List<char>();
        public int Pokusy { get; private set; }
        public int SpatnePokusy { get; private set; }
        public int UhodnutaPismena { get; private set; }
        public Slovo HadaneSlovo { get; private set; }

        public string Vysledky {
            get {
                return "Pokusu " + Pokusy;
            }

            private set {
            }
        }

        Random R = new Random();
        public SpravceHry() {
            Restart();
        }
        public bool TestSlovo(Slovo s) {
            return true;
        }


        public void Restart() {
            //SlovaList=new List<Slovo>();           
            SlovaList.RemoveAll((Slovo s) => true);
            SlovaList.Add(new Slovo("babička"));
            SlovaList.Add(new Slovo("babočka"));
            HadaneSlovo = LosujSlovo();
            Pokusy = 0;
            SpatnePokusy = 0;
            UhodnutaPismena = 0;
            VyvolejZmenu(nameof(Vysledky));
            VyvolejZmenu(nameof(HadaneSlovo));
        }
        public Slovo LosujSlovo() {
            return (Slovo)SlovaList[R.Next(SlovaList.Count)];
        }
        public void Hadej(string tip) {
            if (tip=="") return;
            Pokusy++;
            VyvolejZmenu(nameof(Vysledky));
            if (tip.Length > 1) {
                throw new ArgumentException("To je moc pismen");                
            }
            else {
                int vysledekTipu = HadaneSlovo.OdmaskujSlovo(tip[0]);
                VyvolejZmenu(nameof(HadaneSlovo)); //textbox dole ma sice nastavenej binding, ale timhle mu rekneme, ze se property HadaneSlovo zmenila a ma se aktualizovat
                if (vysledekTipu > 0) {
                    UhodnutaPismena += vysledekTipu;                    
                }
                else {
                    SpatnePokusy++;                    
                }
            }
        }
        protected void VyvolejZmenu(string vlastnost) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(vlastnost));
        }
        public bool Vyhra() {
            if (UhodnutaPismena == HadaneSlovo.PocetPismen) return true;
            else return false;
        }

    }
}
