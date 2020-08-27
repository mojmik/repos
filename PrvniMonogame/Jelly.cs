using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PrvniMonogame {
    class Jelly : GameObject {
        public bool Delete { get; set; }

        public Jelly(Texture2D texture, Vector2 position)
            : base(texture, position) {
            Delete = false;
        }

        public virtual void Update(Game1 game, GameTime gameTime) {
            position.Y += 3;
        }

        public virtual void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, position, Color.White);
        }

        public virtual void Collision(Rectangle ground, Game1 game, Player player) {
            if (GetRectangle().Intersects(ground))
                Delete = true;
        }
    }
}
