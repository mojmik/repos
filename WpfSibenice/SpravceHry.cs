using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Net.Http.Headers;

namespace WpfSibenice {
    class SpravceHry : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        List<Slovo> SlovaList = new List<Slovo>();
        List<char> Tipy = new List<char>();
        public int Pokusy { get; private set; }
        public int SpatnePokusy { get; private set; }
        int PocetPokusu = 11;
        public int UhodnutaPismena { get; private set; }
        public Slovo HadaneSlovo { get; private set; }

        public string Vysledky {
            get {
                return "Pokusu: " + Pokusy + " spatne pokusy: " + SpatnePokusy;
            }

            private set {
            }
        }

        Random R = new Random();
        public SpravceHry() {
            Restart();
        }
   
        public int AktualniSkore() {
            if (!Vyhra()) return 0;
            int skore = HadaneSlovo.PocetPismen * 10 - SpatnePokusy * 5;
            return (skore) < 0 ? 0 : (skore);
        }


        public void Restart() {
            //SlovaList=new List<Slovo>();           
            SlovaList.RemoveAll((Slovo s) => true);
            string[] slova = { "automat", "robot", "tabule", "lavice", "pikachu", "ruka", "noha", "rameno", "hlava", "rostlina", "poleno" };
            foreach (string s in slova) {
                SlovaList.Add(new Slovo(s));
            }
                        
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
            
            if (HadaneSlovo.Hadane() == HadaneSlovo.TextSlova) return true;
            else return false;
        }
        public bool Prohra() {
            if (SpatnePokusy >= PocetPokusu) return true;
            return false;
        }

    }
}
