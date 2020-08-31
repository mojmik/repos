using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace MonogameClovece {
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Screen screen;
        Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        FadeTextManager textManager;
        List<string> textureNames;
        GameBoard gameBoard;
        PlayerManager playerManager;
        Random r = new Random();
        PlayerPiece selectedPiece;
        double elapsedTime;

        double tick, lastMoveTick;
        bool nextTurn = false;
        int hod;
        string addInfo="";

        public Game1() {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            bool superFast = false;
            if (superFast) {
                TargetElapsedTime = new TimeSpan(10);
            }
            

        }


        protected override void Initialize() {
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

        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //nahrani textur
            textureNames = new List<string>() { "zluta", "modra", "cervena", "zelena", "clovece" };
            //textureNames.AddRange( { "clovece","zelena"}     );
            //textureNames.Add("clovece");
            foreach (string s in textureNames) {
                textures.Add(s, Content.Load<Texture2D>(s));
            }
            Texture2D[] tempTextures = new Texture2D[5];
            textures.Values.CopyTo(tempTextures, 0);
            this.playerManager.AddPlayers(tempTextures.ToList(), gameBoard);

            textManager.AddFont(Content.Load<SpriteFont>("font"));
            // TODO: use this.Content to load your game content here

        }

        protected override void Update(GameTime gameTime) {
            // TODO: Add your update logic here    
            tick = gameTime.TotalGameTime.TotalSeconds;
            addInfo = "";
            if (lastMoveTick == 0) lastMoveTick = tick;
            //double elapsedTime = tick - lastMoveTick;
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            KeyboardState ks = Keyboard.GetState();
            MouseState ms = Mouse.GetState();
            if (ks.IsKeyDown(Keys.F1)) {
                gameBoard = new GameBoard();
                gameBoard.InitBoard();
                playerManager = new PlayerManager();
                textManager = new FadeTextManager();
                Texture2D[] tempTextures = new Texture2D[5];
                textures.Values.CopyTo(tempTextures, 0);
                this.playerManager.AddPlayers(tempTextures.ToList(), gameBoard);

                textManager.AddFont(Content.Load<SpriteFont>("font"));
            }

            if (!nextTurn && (elapsedTime >= 10)) {
                nextTurn = true;
                lastMoveTick = tick;
                elapsedTime = 0;
                hod = r.Next(6) + 1;
            }
            if (nextTurn) {                
                
                if (playerManager.GetActivePlayer().IsAI) {
                    playerManager.TestMove(gameBoard, hod);
                    playerManager.NextPlayer();
                    nextTurn = false;
                }
                else {
                    addInfo = "Hrajes!"; 
                    if (ms.LeftButton==ButtonState.Pressed) {
                        //drzi tlacitko
                       if (selectedPiece != null) {
                            selectedPiece.dragPosX = ms.X;
                            selectedPiece.dragPosY = ms.Y;
                        }
                       else {
                            object o = playerManager.findPieceByPos(ms.X, ms.Y);
                            if (o != null) {
                                selectedPiece = (PlayerPiece)o;
                                if (selectedPiece.PlayerId != playerManager.ActivePlayer) selectedPiece = null;
                            }
                        }                       
                    }
                    if (ms.LeftButton==ButtonState.Released) {
                        if (selectedPiece != null) {
                            //drag&drop
                            bool dragAndDrop = false;
                            if (dragAndDrop) {
                                PlaySpace newPos = gameBoard.FindPlaySpaceByPos(ms.X, ms.Y);
                                if (newPos != null) {
                                    selectedPiece.SetPos(newPos);
                                 
                                }
                            }
                            playerManager.GetPlayer(selectedPiece.PlayerId).MovePiece(hod, gameBoard, selectedPiece);

                            selectedPiece.dragPosX = 0;
                            selectedPiece.dragPosX = 0;
                            selectedPiece.isSelected = false;
                            selectedPiece = null;
                            playerManager.NextPlayer();
                        }
                    }
                    if (ks.IsKeyDown(Keys.Space)) {
                        playerManager.NextPlayer();
                        nextTurn = false;
                    }
                }
                
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();
            //            _spriteBatch.DrawString(textManager.GetDefaultFont(), "Ahoj " + gameBoard.PrintBoard(), new Vector2(10, 10), Color.DarkRed);
            _spriteBatch.Draw(textures["clovece"], gameBoard.GetGameBoardRectangle(), Color.White);
            playerManager.DrawPlayers(_spriteBatch, gameBoard, textManager);
            _spriteBatch.DrawString(textManager.GetDefaultFont(), "Hod: " + hod + " " + addInfo, new Vector2(10, 10), Color.DarkRed);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
