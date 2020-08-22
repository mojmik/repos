using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlovickaZkouseni {
    class Hrac {
        public string Jmeno { get; set; }
        public int Skore { get; private set; }
        public Hrac() {
            Skore = 0;
            Jmeno = "anonym";
        }
        public void addScore() {
            Skore += 50;
        }
    }
}
