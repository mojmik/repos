using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace MonogameClovece {
    class PlayerPiece {
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int dragPosX=0;
        public int dragPosY=0;
        public enum PieceStatus { OnStart, InGame, Home };
        public PieceStatus Status { get; set; }
        private PlaySpace playSpacePos;
        public int PlayerId { get; set; }
        public bool isSelected=false;
        
       
        public PlayerPiece(PlaySpace ps, int playerId) {
            SetPos(ps);
            PlayerId = playerId;
        }
        public Rectangle GetSpaceRectangle() {
            int x, y, x2, y2;
            int pieceHalfSizeX = 20, pieceHalfSizeY = 32;
            if (dragPosX>0 && dragPosY>0) {
                x = dragPosX - pieceHalfSizeX / 2 + 1;
                y = dragPosY - pieceHalfSizeY / 2;
                x2 = dragPosX + pieceHalfSizeX / 2 + 1;
                y2 = dragPosY + pieceHalfSizeY / 2;
            }
            else {
                x = PosX - pieceHalfSizeX / 2 + 1;
                y = PosY - pieceHalfSizeY / 2;
                x2 = PosX + pieceHalfSizeX / 2 + 1;
                y2 = PosY + pieceHalfSizeY / 2;
            }            
            return new Rectangle(x, y, x2 - x, y2 - y);
        }
        public void SetPos(PlaySpace newPos) {
            if (playSpacePos != null) {
                playSpacePos.OccupiedBy = null;
            }
            
            playSpacePos = newPos;
            /*
            if (newPos.IsHome) Status = PieceStatus.Home;
            else if (newPos.IsStart) Status = PieceStatus.OnStart;
            else Status = PieceStatus.InGame;
            */            
            newPos.OccupiedBy = this;
            if (newPos.IsHome) Status = PieceStatus.Home;
            else if (newPos.IsStart) Status = PieceStatus.OnStart;
            else Status = PieceStatus.InGame;
            PosX = newPos.X;
            PosY = newPos.Y;            
        }
        public PlaySpace GetPos() {
            return playSpacePos;
        }
        public void GoHome(GameBoard gb) {
            var l = (from p in gb.startSpaces where ((p.Player == PlayerId) && (p.OccupiedBy == null)) select p).First();
            PlaySpace ps = (PlaySpace)l;
            SetPos(ps);
        }
        public void Move(int spaces, GameBoard gb) {

        }
    }
}
