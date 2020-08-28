using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonogameClovece {
    class FadeText {
        private string text;
        private Vector2 position;
        private SpriteFont font;
        private Color color;
        private float time, count;
        public bool Faded { get; private set; }

        public FadeText(string text, Vector2 position, SpriteFont font) {
            this.text = text;
            this.position = position;
            this.font = font;
            color = Color.Purple;
            time = 0;
            count = 1;
            Faded = false;
        }

        public void Update(GameTime gameTime) {
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (time > 0.5 && count < 9) {
                color = color * (0.9f - (count / 10));
                count++;
                time = 0;
            }

            if (count == 9)
                Faded = true;
        }

        public void Draw(SpriteBatch spriteBatch) {
            spriteBatch.DrawString(font, text, position, color);
        }
    }
}
