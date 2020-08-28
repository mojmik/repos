using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonogameClovece
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Screen screen;
        Dictionary <string,Texture2D> textures = new Dictionary<string,Texture2D>();
        FadeTextManager textManager;
        List<string> textureNames;
        GameBoard gameBoard;
        PlayerManager playerManager;
        Random r = new Random();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
                        
        
        }
      

        protected override void Initialize()
        {
            screen = new Screen(_graphics);
            // TODO: Add your initialization logic here
            gameBoard = new GameBoard();
            gameBoard.InitBoard();
            playerManager = new PlayerManager();            
            textManager = new FadeTextManager();            
            _graphics.PreferredBackBufferWidth = 768;
            _graphics.PreferredBackBufferHeight = 800;
            _graphics.ApplyChanges();
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            base.Initialize();

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //nahrani textur
            textureNames = new List<string>() { "zelena", "modra", "zluta", "cervena", "clovece" }; 
            //textureNames.AddRange( { "clovece","zelena"}     );
            //textureNames.Add("clovece");
            foreach (string s in textureNames) {
                textures.Add(s, Content.Load<Texture2D>(s));
            }
            Texture2D[] tempTextures = new Texture2D[5];
            textures.Values.CopyTo(tempTextures, 0);
            this.playerManager.AddPlayers(tempTextures.ToList());

            textManager.AddFont(Content.Load<SpriteFont>("font"));
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            // TODO: Add your update logic here            
            int hod = r.Next(5) + 1;
            playerManager.TestMove(gameBoard,hod);
            playerManager.NextPlayer();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            _spriteBatch.DrawString(textManager.GetDefaultFont(), "Ahoj " + gameBoard.PrintBoard(), new Vector2(10, 10), Color.DarkRed);
            _spriteBatch.Draw(textures["clovece"], gameBoard.GetGameBoardRectangle(), Color.White);
            playerManager.DrawPlayers(_spriteBatch, gameBoard);
            _spriteBatch.End();            
            base.Draw(gameTime);
        }
    }
}
