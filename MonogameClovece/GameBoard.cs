using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Numerics;
using Microsoft.Xna.Framework;

namespace MonogameClovece {
    class GameBoard {
        List<PlaySpace> playSpaces = new List<PlaySpace>();

        int xPos, yPos;
        int xVzdalenost = 63;
        int yVzdalenost = 63;
        int initX = 71;
        int initY = 323;
        int homeAfter = 10;
        public int SpacesCount { get; private set; }
        string initString = "4x,-4y,2x,4y,4x,2y,-4x,4y,-2x,-4y,-4x,-2y";

        public void InitBoard() {
            string[] initFields = initString.Split(',');
            int player = 0;
            int currentPlayer = 1;
            int x = initX;
            int y = initY;
            int spaces = 0;
            player = currentPlayer;
            playSpaces.Add(new PlaySpace(x, y, player));
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
                        player = currentPlayer;
                        currentPlayer++;
                    }
                    else {
                        player = 0;
                    }
                    playSpaces.Add(new PlaySpace(x, y, player));                    
                }
            }
            SpacesCount = spaces;
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
            int x=0, y=0, x2=768, y2=768;            
            return new Rectangle(x, y, x2 - x, y2 - y);
        }
    }
}
