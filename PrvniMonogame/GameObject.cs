using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrvniMonogame {
    class GameObject {
        protected Texture2D texture;
        protected Vector2 position;

        public GameObject(Texture2D texture, Vector2 position) {
            this.texture = texture;
            this.position = position;
        }

        public Vector2 GetPosition() {
            return position;
        }

        public Texture2D GetTexture() {
            return texture;
        }

        public Rectangle GetRectangle() {
            return new Rectangle((int)position.X, (int)position.Y, (int)texture.Width,
                  (int)texture.Height);
        }

        public Vector2 GetSize() {
            return new Vector2(texture.Width, texture.Height);
        }
    }
}
