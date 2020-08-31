using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonogameClovece {
    class Player {
        public Texture2D PlayerImg { get; set; }
        public List <PlayerPiece> playerPieces = new List<PlayerPiece>();
        public int Id { get; private set; }
        public bool Won { get; set; } = false;
        public bool IsAI { get; set; }
        public int Rank { get; set; }
        public Vector2 txtPos { get; set; }
        public Rectangle statusRect { get; set; }
        //public int PlaySpacePos {get; set; }
        public Player(int id) {
            Id = id;            
        }

        public Player(Texture2D img, int id) {
            PlayerImg = img;
            Id = id;
        }
        public void AddPiece(PlayerPiece p) {
            playerPieces.Add(p);
        }
        public void AddPieces(List<PlaySpace> l) {
            foreach (PlaySpace ps in l) {
                AddPiece(new PlayerPiece(ps, Id));
            }
        }
        public void DrawPieces(SpriteBatch sb, GameBoard gb) {
            foreach (PlayerPiece p in playerPieces) {
                if (p.isSelected) {                    
                    Texture2D selPiece = PlayerImg;
                    Texture2D newPiece = new Texture2D(sb.GraphicsDevice, PlayerImg.Width, PlayerImg.Height);
                    Color[] colorData=new Color[PlayerImg.Width * PlayerImg.Height];
                    selPiece.GetData(colorData);
                    for (int y=0;y<PlayerImg.Height;y++) {
                        for (int x=0;x<PlayerImg.Width;x++) {
                            if ( (y>0 && y<20) || (y>PlayerImg.Height-20)) colorData[(y*PlayerImg.Width) + x] = Color.Red;
                            
                        }
                    }
                    newPiece.SetData(colorData);
                    sb.Draw(newPiece, p.GetSpaceRectangle(), Color.White);
                }
                else sb.Draw(PlayerImg, p.GetSpaceRectangle(), Color.White);
            }            
        }
        public void MovePiece(int spaces, GameBoard gb, PlayerPiece selPiece = null) {
            List<PlayerPiece> pHome = new List<PlayerPiece>();
            List<PlayerPiece> pOnStart = new List<PlayerPiece>();
            List<PlayerPiece> pInGame = new List<PlayerPiece>();
            PlaySpace ps;
            PlaySpace psNew;


            if (selPiece==null) {
                var pHomeV = (from p in playerPieces where (p.Status == PlayerPiece.PieceStatus.Home) select p);
                var pInGameV = (from p in playerPieces where (p.Status == PlayerPiece.PieceStatus.InGame) select p);
                var pOnStartV = (from p in playerPieces where (p.Status == PlayerPiece.PieceStatus.OnStart) select p);

                pHome = (List<PlayerPiece>)pHomeV.ToList();
                pOnStart = (List<PlayerPiece>)pOnStartV.ToList();
                pInGame = (List<PlayerPiece>)pInGameV.ToList();
            }
            else {
                if (selPiece.Status == PlayerPiece.PieceStatus.Home) pHome.Add(selPiece);
                if (selPiece.Status == PlayerPiece.PieceStatus.InGame) pInGame.Add(selPiece);
                if (selPiece.Status == PlayerPiece.PieceStatus.OnStart) pOnStart.Add(selPiece);
            }

            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.Enter)) {
                int n = 0;
            }

                

            if (pOnStart.Count>0 && spaces==6) {
                //nasazeni figurky
                foreach (PlayerPiece piece in pOnStart) {                    
                    piece.Status = PlayerPiece.PieceStatus.InGame;
                    psNew = gb.GetFirstPos(Id);
                    if (psNew.OccupiedBy == null) {
                        piece.SetPos(psNew);
                        return;
                    }
                    else if (psNew.OccupiedBy.PlayerId == Id) {

                    }
                    else {
                        psNew.OccupiedBy.GoHome(gb);
                        piece.SetPos(psNew);
                        return;
                    }
                }                
            }

            bool headingHome = false;
            if (pInGame.Count > 0) {
                //pohyb figurky
                foreach (PlayerPiece piece in pInGame) {                    
                    ps = piece.GetPos();
                    psNew = ps;
                    headingHome = false;
                    for (int n = 0; n < spaces; n++) {
                        if (!headingHome) {
                            if (psNew.IsLast == true && psNew.Player == this.Id) {
                                //jde domu
                                headingHome = true;
                            }
                            else psNew = gb.GetNextPiece(psNew);
                        }
                        else  {
                                 psNew=gb.GetNextHomePiece(psNew,Id);
                        }
                    }
                    if (psNew.OccupiedBy == null) {                            
                        piece.SetPos(psNew);
                        return;
                    }
                    else if (psNew.OccupiedBy.PlayerId == Id) {
                        //je tam jinou svoji figurkou
                    }
                    else {
                        //vyhozeni cizi figurky
                        psNew.OccupiedBy.GoHome(gb);
                        piece.SetPos(psNew);
                        return;
                    }
                }                
            }
            

            /*
            foreach (PlayerPiece p in playerPieces) {

            }
            */
        }

    }
}
