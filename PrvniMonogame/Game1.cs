﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Security.Cryptography.X509Certificates;

namespace PrvniMonogame {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D ground, background;
        private Player player;
        public int score;
        FadeTextManager textManager;
        private Logic logic;
        private float boxSpawnTime;
        private float toxicBoxSpawnTime;
        private float sugarBoxSpawnTime;
        public enum eStavHry {
            Hra,
            Pauza,
            GameOver,
            Menu,
        }
        public eStavHry stavHry;


        public int ScoreMultiply { get; set; }

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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
        public Rectangle GetGroundRect() {
            //return new Rectangle(0, (int)GetScreenSize().Y - 179, 800,(int)GetScreenSize().Y);
            return GetMikRectagleLambda(0, (int)GetScreenSize().Y - 179, (int)GetScreenSize().X, (int)GetScreenSize().Y);
        }
        public Rectangle GetBackgroundRect() {
            return new Rectangle(0, 0, (int)GetScreenSize().X, (int)GetScreenSize().Y - 179);
        }

        protected override void Initialize() {
            // TODO: Add your initialization logic here
            textManager = new FadeTextManager();
            score = 0;
            ScoreMultiply = 1;
            toxicBoxSpawnTime = 0;
            sugarBoxSpawnTime = 0;
            stavHry = eStavHry.Menu;
            base.Initialize();
        }

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here            
            ground = Content.Load<Texture2D>("ground");
            background = Content.Load<Texture2D>("background");
            textManager.AddFont(Content.Load<SpriteFont>("Font"));
            player = new Player(Content.Load<Texture2D>("player"), new Vector2(GetScreenSize().X / 2, GetGroundRect().Y - 70), 5, 5, this);
            logic = new Logic(this, Content.Load<Texture2D>("zelenyzele"), Content.Load<Texture2D>("hnedyzele"), Content.Load<Texture2D>("strela"), Content.Load<Texture2D>("toxic"), Content.Load<Texture2D>("sugar")); 
        }

        protected override void Update(GameTime gameTime) {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            KeyboardState ks = Keyboard.GetState();
            player.Update(ks, gameTime);
            textManager.UpdateTexts(gameTime);
            logic.Update(ks, player, gameTime);
            boxSpawnTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (boxSpawnTime > 2) {
                boxSpawnTime = 0;
                logic.SpawnJelly();
            }

            toxicBoxSpawnTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (toxicBoxSpawnTime > 10) {
                toxicBoxSpawnTime = 0;
                logic.SpawnToxicBox();
            }

            sugarBoxSpawnTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (sugarBoxSpawnTime > 22) {
                sugarBoxSpawnTime = 0;
                logic.SpawnSugarBox();
            }

            base.Update(gameTime);
        }
        public void AddScore(int value, Vector2 position) {
            int addScore;
            if (value > 0)
                addScore = value * ScoreMultiply;
            else
                addScore = value;

            // grafické zobrazení přidání/odebrání score
            textManager.AddText(addScore.ToString(), position);
            this.score += addScore;
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            Vector2 backgroundPosition = new Vector2(0, 0);
            Color whiteColor = Color.White;

            //SpriteBatch.Draw(Texture2D texture, Vector2 position, Rectangle ? sourceRectangle, Color color, float rotation, Vector2 origin, **float scale * *, SpriteEffects effects, float layerDepth)            

            //_spriteBatch.Draw(background, backgroundPosition,GetBackgroundRect(), whiteColor,0f,Vector2.Zero,1f,SpriteEffects.None,0f);
            _spriteBatch.Draw(ground, GetGroundRect(), whiteColor);
            _spriteBatch.Draw(background, GetBackgroundRect(), whiteColor);
            //player.Draw(_spriteBatch);
            _spriteBatch.Draw(player.GetTexture(), player.GetRectangle(), whiteColor);

            foreach (Jelly j in logic.jellies)
                j.Draw(_spriteBatch);

            foreach (Bullet bullet in logic.bullets)
                bullet.Draw(_spriteBatch);

            // vykreslení grafiky score
            foreach (FadeText fe in textManager.fades)
                fe.Draw(_spriteBatch);

            _spriteBatch.DrawString(textManager.GetDefaultFont(), "" +score, new Vector2(95, GetGroundRect().Y + 35), Color.DarkRed);
            _spriteBatch.DrawString(textManager.GetDefaultFont(), "" + player.Lives, new Vector2(230, GetGroundRect().Y + 35), Color.DarkRed);
            _spriteBatch.DrawString(textManager.GetDefaultFont(), "" + ScoreMultiply, new Vector2(380, GetGroundRect().Y + 35), Color.DarkRed);
            _spriteBatch.DrawString(textManager.GetDefaultFont(), "" + player.EatedJellies, new Vector2(560, GetGroundRect().Y + 35), Color.DarkRed);
            
            _spriteBatch.End();

            base.Draw(gameTime);
            
        }
    }
}
