using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrvniMonogame.Menu {
    class MenuScreen {
        private Texture2D background, newGame, exit;
        private List<Button> buttons = new List<Button>();
        private Game1 game;
        private bool upPressed, downPressed;
        private int index;
        public bool NewGame { get; set; }
    }
}
