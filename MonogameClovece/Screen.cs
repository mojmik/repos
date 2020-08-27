using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonogameClovece {
    

    class Screen {
        GraphicsDeviceManager _graphics;

        public Screen(GraphicsDeviceManager _graphics) {
            _graphics.PreferredBackBufferHeight = 1920;
            _graphics.PreferredBackBufferWidth = 1440;
            _graphics.ApplyChanges();
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
        }

        public Vector2 GetScreenSize() {
            return new Vector2(_graphics.PreferredBackBufferWidth,
                 _graphics.PreferredBackBufferHeight);
        }

        /*
        public Rectangle GetMikRectange(int x, int y, int x2, int y2) {
            return new Rectangle(x, y, x2 - x, y2 - y); //rectange x,y,sizeW,sizeH
        }
        */
        public Rectangle GetMikRectagleLambda(int x, int y, int x2, int y2) => new Rectangle(x, y, x2 - x, y2 - y);
    }
}
