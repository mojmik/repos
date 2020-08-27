using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrvniMonogame {
    class Bullet : GameObject {
        private int speed;
        public bool Delete { get; set; }

        public Bullet(Texture2D texture, Vector2 position, int speed)
            : base(texture, position) {
            this.texture = texture;
            this.position = position;
            this.speed = speed;
            this.Delete = false;
        }

        public void Update(GameTime gameTime) {
            position.Y -= speed;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, position, Color.White);
        }

        public void Collision(Jelly obj, Game1 game) {
            if (GetRectangle().Intersects(obj.GetRectangle())) {
                obj.Delete = true;
                this.Delete = true;
                game.AddScore(2, obj.GetCenterPosition());
            }

        }
    }
}
