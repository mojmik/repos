using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Numerics;
using Microsoft.Xna.Framework;
using System.Security.Cryptography;

namespace MonogameClovece {
    class GameBoard {
        public List<PlaySpace> playSpaces = new List<PlaySpace>();
        public List<PlaySpace> startSpaces = new List<PlaySpace>();
        public List<PlaySpace> homeSpaces = new List<PlaySpace>();

        int xPos, yPos;
        int xVzdalenost = 63;
        int yVzdalenost = 63;
        int initX = 71;
        int initY = 323;
        int homeAfter = 10;
        public int SpacesCount { get; private set; }
        string initString = "4x,-4y,2x,4y,4x,2y,-4x,4y,-2x,-4y,-4x,-1y";

        public void InitBoard() {
            string[] initFields = initString.Split(',');
            int player = 0;
            int currentPlayer = 0;
            int x = initX;
            int y = initY;
            int spaces = 0;
            int[] playerOrder = { 0, 1, 3, 2 };
            bool isFirst = true;
            player = playerOrder[currentPlayer];
            playSpaces.Add(new PlaySpace(x, y, player, false, isFirst, false));
            currentPlayer++;


            foreach (string s in initFields) {

                string sNum = Regex.Match(s, @"-?\d+").Value;
                string rest = s.Replace(sNum, "");
                int cnt = int.Parse(sNum);
                int cntNum = Math.Abs(cnt);
                for (int n = 0; n < cntNum; n++) {
                    if (rest == "x" && cnt < 0) x -= xVzdalenost;
                    if (rest == "x" && cnt > 0) x += xVzdalenost;
                    if (rest == "y" && cnt < 0) y -= yVzdalenost;
                    if (rest == "y" && cnt > 0) y += yVzdalenost;
                    spaces++;
                    if (spaces % homeAfter == 0) {
                        player = playerOrder[currentPlayer];
                        currentPlayer++;
                        isFirst = true;
                    }
                    else {
                        player = 0;
                        isFirst = false;
                    }
                    playSpaces.Add(new PlaySpace(x, y, player, false, isFirst, false));
                }
            }

            currentPlayer = 1;
            for (int n = 0; n < playSpaces.Count; n++) {
                if (playSpaces[n].IsFirst && n > 0) {
                    playSpaces[n - 1].IsLast = true;
                    playSpaces[n - 1].Player = playerOrder[currentPlayer];
                    currentPlayer++;
                }
            }
            playSpaces.Last().IsLast = true;
            playSpaces.Last().Player = playerOrder[0];

            SpacesCount = spaces;

            bool isStart = true;
            startSpaces.Add(new PlaySpace(116, 116, 0, isStart, false, false));
            startSpaces.Add(new PlaySpace(182, 116, 0, isStart, false, false));
            startSpaces.Add(new PlaySpace(116, 183, 0, isStart, false, false));
            startSpaces.Add(new PlaySpace(182, 183, 0, isStart, false, false));

            startSpaces.Add(new PlaySpace(585, 116, 1, isStart, false, false));
            startSpaces.Add(new PlaySpace(651, 116, 1, isStart, false, false));
            startSpaces.Add(new PlaySpace(585, 183, 1, isStart, false, false));
            startSpaces.Add(new PlaySpace(651, 183, 1, isStart, false, false));

            startSpaces.Add(new PlaySpace(116, 585, 2, isStart, false, false));
            startSpaces.Add(new PlaySpace(182, 585, 2, isStart, false, false));
            startSpaces.Add(new PlaySpace(116, 652, 2, isStart, false, false));
            startSpaces.Add(new PlaySpace(182, 652, 2, isStart, false, false));

            startSpaces.Add(new PlaySpace(585, 585, 3, isStart, false, false));
            startSpaces.Add(new PlaySpace(651, 585, 3, isStart, false, false));
            startSpaces.Add(new PlaySpace(585, 652, 3, isStart, false, false));
            startSpaces.Add(new PlaySpace(651, 652, 3, isStart, false, false));

            isStart = false;
            bool isHome = true;
            homeSpaces.Add(new PlaySpace(132, 384, 0, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(186, 384, 0, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(260, 384, 0, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(322, 384, 0, isStart, false, isHome));

            homeSpaces.Add(new PlaySpace(385, 134, 1, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(385, 195, 1, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(385, 259, 1, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(385, 324, 1, isStart, false, isHome));

            homeSpaces.Add(new PlaySpace(636, 384, 3, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(573, 384, 3, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(511, 384, 3, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(448, 384, 3, isStart, false, isHome));

            homeSpaces.Add(new PlaySpace(385, 636, 2, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(385, 571, 2, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(385, 509, 2, isStart, false, isHome));
            homeSpaces.Add(new PlaySpace(385, 447, 2, isStart, false, isHome));
        }

        public string PrintBoard() {
            string s = "";
            foreach (PlaySpace p in playSpaces) {
                s += p.ToString() + ", ";
            }
            return s;
        }
        public Rectangle GetSpaceRectangle(int space) {
            int x, y, x2, y2;
            int velikostPoleX = 30, velikostPoleY = 30;
            x = playSpaces[space].X - velikostPoleX / 2;
            y = playSpaces[space].Y - velikostPoleY / 2;
            x2 = playSpaces[space].X + velikostPoleX / 2;
            y2 = playSpaces[space].Y + velikostPoleY / 2;
            return new Rectangle(x, y, x2 - x, y2 - y);
        }
        public Rectangle GetGameBoardRectangle() {
            int x = 0, y = 0, x2 = 768, y2 = 768;
            return new Rectangle(x, y, x2 - x, y2 - y);
        }
        public PlaySpace GetFirstPos(int player) {
            var l = (from p in playSpaces where ((p.Player == player) && (p.IsFirst)) select p).First();
            //var l = (from p in playSpaces where ((p.Player == player) && (p.IsFirst)) select p);
            PlaySpace ps = (PlaySpace)l;
            return ps;
        }
        public List<PlaySpace> GetStartPlaySpaces(int player) {
            List<PlaySpace> l = (List<PlaySpace>)(from p in startSpaces where p.Player == player && p.IsStart select p).ToList();
            return l;
        }
        public PlaySpace GetNextPiece(PlaySpace ps) {
            for (int n = 0; n < playSpaces.Count; n++) {
                if (ps == playSpaces[n] && n < playSpaces.Count - 1) return playSpaces[n + 1];
            }
            return playSpaces[0];
        }
        public PlaySpace GetNextHomePiece(PlaySpace ps, int playerId) {
            PlaySpace psOut;
            int player = ps.Player;

            if (ps.IsLast) { //vstupuje do domecku 
                var l = (from p in homeSpaces where (p.Player == player) select p).First();
                PlaySpace psNew = (PlaySpace)l;
                return psNew;

            }
            else { //posun v domecku 
                var l = (from p in homeSpaces where (p.Player == player) select p).ToList();
                for (int n = 0; n < l.Count; n++) {
                    if (ps == l[n] && n < l.Count - 1) {
                        psOut = (PlaySpace)l[n + 1];
                        return psOut;
                    }
                }
                psOut = (PlaySpace)l[0];
                return psOut;
            }
        }
        public PlaySpace FindPlaySpaceByPos(int x, int y) {
            PlaySpace selP = null;
            foreach (PlaySpace p in playSpaces) {
                if (p.X <= x + 10 && p.X >= x - 10 && p.Y <= y + 10 && p.Y >= y - 10) {
                    selP = p;
                }
            }
            return selP;
        }
    }
}
