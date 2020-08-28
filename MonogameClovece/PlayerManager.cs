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
        int playersCnt = 4;
        public int ActivePlayer { get; private set; }
        public void AddPlayers(List<Texture2D> textury) {
            for (int n=0; n<playersCnt;n++) {
                players.Add(new Player(textury[n]));
            }            
        }
        public void MovePlayer(int i, int spaces, GameBoard gb) {
            players[i].PlaySpacePos += spaces;
            if (players[i].PlaySpacePos > gb.SpacesCount) players[i].PlaySpacePos = 0;
        }
        public void TestMove(GameBoard gb, int hodKostkou=1) {
            MovePlayer(ActivePlayer, hodKostkou, gb);
        }
        public void DrawPlayers(SpriteBatch sb, GameBoard gb) {
            foreach (Player p in players) {
                sb.Draw(p.PlayerImg, gb.GetSpaceRectangle(p.PlaySpacePos), Color.White);
            }
        }
        public int NextPlayer() {            
            ActivePlayer++;
            if (ActivePlayer > playersCnt-1) ActivePlayer = 0;
            return ActivePlayer;
        }
    }
}
