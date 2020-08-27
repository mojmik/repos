using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrvniMonogame {
    class ToxicJelly : Jelly {
        private float timeCounter;
        private Random random;
        private int range;

        public ToxicJelly(Texture2D texture, Vector2 position, int range)
            : base(texture, position) {
            timeCounter = 0;
            random = new Random();
            this.range = range;
        }

        public override void Update(Game1 game, GameTime gameTime) {
            timeCounter += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (timeCounter > 2) {
                timeCounter = 0;
                int xMin = 0;
                int xMax = 0;

                // výpočet posunu
                if (game.GetScreenSize().X - position.X - texture.Width < range) {
                    xMin = -range;
                    xMax = (int)game.GetScreenSize().X - (int)position.X - texture.Width;
                }
                if (position.X - range < 0) {
                    xMin = -(int)position.X;
                    xMax = range;
                }
                else if (game.GetScreenSize().X - position.X - texture.Width > range && position.X - range > 0) {
                    xMin = -range;
                    xMax = range;
                }

                int x = random.Next(xMin, xMax);
                position = new Vector2(position.X + x, position.Y);
            }

            position.Y += 2;
        }
        public override void Collision(Rectangle ground, Game1 game, Player player) {
            if (GetRectangle().Intersects(ground) && !Delete) {
                game.AddScore(-5, new Vector2(position.X + texture.Width / 2, position.Y + texture.Height / 2));
                player.Lives--;
                game.ScoreMultiply = 1;
            }

            base.Collision(ground, game, player);
        }
        public void SetPosition(int x, int y) {
            position = new Vector2(x, y);
        }

        public override void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }
}
