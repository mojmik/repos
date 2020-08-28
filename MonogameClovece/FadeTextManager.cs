using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameClovece {
    /// <summary>
    /// ovlada textiky
    /// </summary>
    class FadeTextManager {
        private SpriteFont font;
        public List<FadeText> fades;
        
        public FadeTextManager () {
            this.fades = new List<FadeText>();
        }
        public void AddFont(SpriteFont font) {
            this.font = font;
        }
        public void AddText(string text, Vector2 pos, SpriteFont whichFont) {
            
            this.fades.Add(new FadeText(text,pos, whichFont));
        }
        public void AddText(string text, Vector2 pos) {

            this.fades.Add(new FadeText(text, pos, font));
        }
        public SpriteFont GetDefaultFont() {
            return font;
        }
        public void UpdateTexts(GameTime gameTime) {
            if (fades.Count < 1) return;
            foreach (FadeText fe in fades)
                fe.Update(gameTime);

            for (int i = 0; i < fades.Count; i++) {
                if (fades[i].Faded)
                    fades.RemoveAt(i);
            }
        }
    }
}
