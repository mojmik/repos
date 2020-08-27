using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrvniMonogame.Menu {
    class Button {
        private Texture2D texture;
        private Vector2 position;
        private bool isActive;
        public Button(Texture2D texture, Vector2 position) {
            this.texture = texture;
            this.position = position;
            isActive = false;
        }
        public void Activate() {
            isActive = true;
        }

        public void Deactivate() {
            isActive = false;
        }
        public void Draw(SpriteBatch spriteBatch) {
            if (isActive)
                spriteBatch.Draw(texture, position, Color.White);
            else
                spriteBatch.Draw(texture, position, Color.White * 0.5f);
        }
    }
}
