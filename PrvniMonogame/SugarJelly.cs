using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PrvniMonogame {
    class SugarJelly : Jelly {
        public int Scale { get; private set; }
        private float time;

        public SugarJelly(Texture2D texture, Vector2 position)
    : base(texture, position) {
            time = 0;
            Scale = 50;
        }

        public override void Update(Game1 game, GameTime gameTime) {
            // harmonické kmytání kostky
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
            position.X += (Scale / 10) * (float)Math.Sin((2 * Math.PI / 1) * time);

            position.Y += 1;
        }
        public void SetPosition(int x, int y) {
            position = new Vector2(x, y);
        }
    }
}
