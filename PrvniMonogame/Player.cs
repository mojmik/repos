﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PrvniMonogame {
    class Player : GameObject {
        private Game1 game;
        public int Lives { get; set; }
        public int Speed { get; set; }
        public int EatedJellies { get; set; }

        public Player(Texture2D texture, Vector2 position, int Lives, int Speed, Game1 game)
            : base(texture, position) {
            this.texture = texture;
            this.position = position;
            this.Lives = Lives;
            this.Speed = Speed;
            this.game = game;
            EatedJellies = 0;
        }

        public void Update(KeyboardState ks, GameTime gameTime) {
            if (ks.IsKeyDown(Keys.Left) && position.X > 0)
                position.X -= Speed;
            else if (ks.IsKeyDown(Keys.Right) && position.X < game.GetScreenSize().X - texture.Width)
                position.X += Speed;
        }


        public void Collision(Jelly obj) {
            if (GetRectangle().Intersects(obj.GetRectangle())) {
                if (obj is ToxicJelly) {
                    obj.Delete = true;
                    Lives--;
                    game.ScoreMultiply = 1;
                }
                else if (obj is SugarJelly) {
                    obj.Delete = true;
                    Lives++;
                    game.ScoreMultiply++;
                }
                else {
                    obj.Delete = true;
                    EatedJellies++;
                    game.AddScore(5, obj.GetCenterPosition());
                }
            }
            
        }


        /// <summary>
        /// tohle se muze odstranit, vykresuljeme ve draw
        /// 
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.Draw(texture, position, Color.White);
        }
    }
}
