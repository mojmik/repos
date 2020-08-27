using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PrvniMonogame {
    class Logic {
        private Random random;
        private Game1 game;
        public List<Jelly> jellies;
        public List<Bullet> bullets;
        private Texture2D greenJelly, brownJelly, jellyBullet;
        private List<Texture2D> textures;
        private Texture2D toxicBox;
        public int level = 1;
        private Texture2D sugarBox;



        private bool keyPressed;

        public Logic(Game1 game, Texture2D greenJelly, Texture2D brownJelly,
            Texture2D jellyBullet, Texture2D toxicBox, Texture2D sugarBox) {
            this.game = game;        
            this.greenJelly = greenJelly;
            this.brownJelly = brownJelly;
            this.jellyBullet = jellyBullet;
            this.toxicBox = toxicBox;
            this.sugarBox = sugarBox;
            jellies = new List<Jelly>();
            bullets = new List<Bullet>();
            keyPressed = false;
            random = new Random();

            textures = new List<Texture2D>();
            textures.Add(this.greenJelly);
            textures.Add(this.brownJelly);
        }
        public void SpawnJelly() {
            int t = random.Next(0, textures.Count);
            int x = random.Next(0, (int)game.GetScreenSize().X - textures[t].Width);

            jellies.Add(new Jelly(textures[t], new Vector2(x, -100)));
        }
        public void SpawnToxicBox() {
            Jelly box = new ToxicJelly(toxicBox, Vector2.Zero, 100);
            int x = random.Next(0, (int)game.GetScreenSize().X - (int)(box.GetSize().X));
            ((ToxicJelly)box).SetPosition(x, -100);

            jellies.Add(box);
        }
        public void SpawnSugarBox() {
            Jelly box = new SugarJelly(sugarBox, Vector2.Zero);
            int x = random.Next(0,
                (int)game.GetScreenSize().X - (int)box.GetSize().X - (((SugarJelly)box).Scale * 2));
            ((SugarJelly)box).SetPosition((int)x, -100);

            jellies.Add(box);
        }
        public void Update(KeyboardState ks, Player player, GameTime gameTime) {
            // střelba
            if (ks.IsKeyDown(Keys.Space) && !keyPressed) {
                bullets.Add(new Bullet(jellyBullet,
                    new Vector2(player.GetPosition().X + player.GetRectangle().Width / 2 - jellyBullet.Width / 2,
                    player.GetPosition().Y - jellyBullet.Height / 2), 3));

                keyPressed = true;
            }
            if (ks.IsKeyUp(Keys.Space) && keyPressed)
                keyPressed = false;

            // updatování střel, kontrola kolize střel a Jellyů
            foreach (Bullet bullet in bullets) {
                // update střel
                bullet.Update(gameTime);
                // kolize střel
                foreach (Jelly j in jellies)
                    bullet.Collision(j, game);
            }

            // updatování Jellyů a kontrola jejich kolize se zemí
            foreach (Jelly j in jellies) {
                j.Update(game, gameTime);
                j.Collision(game.GetGroundRect(), game, player);
            }

            // kontrola kolize hráče a Jellyů
            foreach (Jelly j in jellies)
                player.Collision(j);

            // mazaní Jellyů
            for (int i = 0; i < jellies.Count; i++) {
                if (jellies[i].Delete)
                    jellies.RemoveAt(i);
            }

            // mazání střel
            for (int i = 0; i < bullets.Count; i++) {
                if (bullets[i].Delete)
                    bullets.RemoveAt(i);
            }

            // mazání střel, jakmile vyjedou z hrací plochy
            for (int i = 0; i < bullets.Count; i++) {
                if (bullets[i].GetPosition().Y < 0 - bullets[i].GetRectangle().Height)
                    bullets.RemoveAt(i);
            }

            for (int e = 0; e < jellies.Count; e++) {
                for (int i = 0; i < jellies.Count; i++) {
                    if (e != i) {
                        if (jellies[e].GetRectangle().Intersects(jellies[i].GetRectangle())) {
                            if (jellies[e].GetType() != typeof(SugarJelly) &&
                                jellies[i].GetType() != typeof(SugarJelly)) {
                                if (jellies[e].GetType() == typeof(ToxicJelly) &&
                                    jellies[i].GetType() != typeof(ToxicJelly))
                                    jellies[i].Delete = true;
                                else if (jellies[i].GetType() != typeof(ToxicJelly))
                                    jellies[i].Delete = true;
                            }
                        }
                    }
                }
            }
        }




    }
}
