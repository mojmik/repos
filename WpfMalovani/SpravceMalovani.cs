using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfMalovani {
    class SpravceMalovani {
        Platno platno;
        MalovaniWindow mw;
        public SpravceMalovani() {
            
        }
        public bool VytvorPlatno(string sy, string sx) {
            int y = int.Parse(sy);
            int x = int.Parse(sx);
            if (y < 101 && x < 101) {
                this.platno = new Platno(y, x);
                return true;
            }
            else return false;
        }
        public void JdemeMalovat() {
            mw = new MalovaniWindow(platno);
            mw.RoztahniCanvas();
            platno.VykresliSe(mw.malovaniCanvas);
            mw.ShowDialog();            
        }
        
    }
}
