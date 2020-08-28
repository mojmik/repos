using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonogameClovece {
    class Player {
        public Texture2D PlayerImg { get; set; }
        public int PlaySpacePos {get; set; }
        public Player() {
            
        }

        public Player(Texture2D img) {
            PlayerImg = img;
        }

    }
}
