using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MonogameClovece
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Screen screen;
        Dictionary <string,Texture2D> textures = new Dictionary<string,Texture2D>();
        List<string> textureNames;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
                        
        
        }
      

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            screen = new Screen(_graphics);            
            base.Initialize();

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //nahrani textur
            textureNames = new List<string>() { "clovece", "zelena", "modra", "zluta", "cervena" }; 
            //textureNames.AddRange( { "clovece","zelena"}     );
            //textureNames.Add("clovece");
            foreach (string s in textureNames) {
                textures.Add(s, Content.Load<Texture2D>(s));
            }
             

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
