using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;

namespace MonogameClovece {
    class PlayerManager {
        List<Player> players=new List<Player>();
        int rankings = 1;
        int playersCnt = 4;
        public int ActivePlayer { get; private set; }
        public void AddPlayers(List<Texture2D> textury, GameBoard gb) {
            for (int n=0; n<playersCnt;n++) {
                players.Add(new Player(textury[n],n));
                players[n].AddPieces(gb.GetStartPlaySpaces(n));
                players[n].IsAI = true;
            }
            players[3].IsAI = false;
            players[0].txtPos = new Vector2(50, 39);
            players[0].statusRect = new Rectangle(40, 39, 20, 40);
            
            players[1].txtPos = new Vector2(618, 39);
            players[1].statusRect = new Rectangle(608, 39, 20, 40);
            
            players[2].txtPos = new Vector2(47, 713);
            players[2].statusRect = new Rectangle(37, 713, 20, 40);
            
            players[3].txtPos = new Vector2(618, 713);
            players[3].statusRect = new Rectangle(608, 713, 20, 40);
        }
        public void MovePlayer(int i, int spaces, GameBoard gb) {
           players[i].MovePiece(spaces,gb);
        }
        public void TestMove(GameBoard gb, int hodKostkou=1) {
            MovePlayer(ActivePlayer, hodKostkou, gb);
            if (players[ActivePlayer].Won && players[ActivePlayer].Rank == 0) {
                players[ActivePlayer].Rank = rankings;
                rankings++;
            }
            
        }
        public void DrawPlayers(SpriteBatch sb, GameBoard gb, FadeTextManager textManager) {
            foreach (Player p in players) {
                p.DrawPieces(sb,gb);       
                if (p.Rank >0) sb.DrawString(textManager.GetDefaultFont(), "Rank " + p.Rank, p.txtPos, Color.DarkRed);
                if (GetActivePlayer() == p) {
                    sb.Draw(p.PlayerImg, p.statusRect, Color.White );
                }
            }
        }
        public int NextPlayer() {
            if ((from PlayerPiece p in players[ActivePlayer].playerPieces where (p.Status == PlayerPiece.PieceStatus.InGame || p.Status == PlayerPiece.PieceStatus.OnStart) select p).Count() < 1) GetActivePlayer().Won = true;
            
            ActivePlayer++;
            if (ActivePlayer > playersCnt-1) ActivePlayer = 0;
            return ActivePlayer;
        }
        public Player GetPlayer(int num) {
            return players[num];
        }
        public Player GetActivePlayer() {      
            return GetPlayer(ActivePlayer);
        }
        public void AddPiece(PlayerPiece p, int player) {
            players[player].AddPiece(p);
        }
        public PlayerPiece findPieceByPos(int x,int y) {
          PlayerPiece selP=null;
          foreach (Player p in players) {
                foreach (PlayerPiece c in p.playerPieces) {
                    c.isSelected = false;
                    if (c.PosX<=x+10 && c.PosX>=x-10 && c.PosY<=y+10 && c.PosY>=y-10) {
                        c.isSelected = true;
                        selP = c;
                    }
                }
          }
            return selP;
        }
    }
}
