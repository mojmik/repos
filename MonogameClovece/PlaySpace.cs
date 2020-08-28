using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.PlayTo;

namespace MonogameClovece {
    class PlaySpace {
        public int X { get; set; }
        public int Y { get; set; }
        
        int cisloHrace;
        public PlaySpace(int x, int y, int cisloHrace) {
            X = x;
            Y = y;
            this.cisloHrace = cisloHrace;
        }
        public override string ToString() {
            return $" x:{X} y:{Y} player:{cisloHrace}";
        }
    }
}
