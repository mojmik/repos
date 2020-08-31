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
        public int Player { get; set; }
        
        public bool IsFirst { get; set; }
        
        public bool IsStart { get; set; }
        public bool IsLast { get; set; }
        public bool IsHome { get; set; }
                

        static int idCounter=0;
        int thisId;
        public PlayerPiece OccupiedBy { get; set; }

        public PlaySpace(int x, int y, int player, bool isStart, bool isFirst, bool isHome) {
            X = x;
            Y = y;
            Player = player;
            IsHome = isHome;
            IsFirst = isFirst;
            IsStart = isStart;
            thisId = idCounter;
            PlaySpace.idCounter++;

        }
        public override string ToString() {
            return $" x:{X} y:{Y} player:{Player}";
        }
        
    }
}
