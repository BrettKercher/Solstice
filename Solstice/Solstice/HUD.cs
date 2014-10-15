using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Solstice
{
    class HUD
    {

        Texture2D[] batTextures = new Texture2D[4];
        Rectangle[] batRects = new Rectangle[4];
        int maxHeight = 25;
        Color[] batColor = new Color[4];
        int[] batLife = new int[4];
        Texture2D hud;
        Rectangle hud_rect = new Rectangle(0, 0, 960, 640);
        Texture2D HP;
        Rectangle HP_RECT = new Rectangle(10, 16, 250, 25);
        Texture2D XP;
        Rectangle XP_RECT = new Rectangle(10, 8, 0, 3);
        int maxWidth = 250;
        float scale;
        SpriteFont font;
        String hString = "10 / 10";
        String enemies = "Enemies Remaining: ";
        Vector2 hsPos;
        int numEnemies;
        int totalEnemies;

        const int toNextLevel = 100;
        float progress = 0;

        int tint = 108;

        public void Load(ContentManager content, int initEnemies)
        {
            hud = content.Load<Texture2D>("HUD/HUD");
            HP = content.Load<Texture2D>("HUD/HealthBar");
            XP = content.Load<Texture2D>("HUD/XPBar");
            font = content.Load<SpriteFont>("SpriteFonts/MenuFont");
            batTextures[0] = content.Load<Texture2D>("HUD/FireBat");
            batTextures[1] = content.Load<Texture2D>("HUD/IceBat");
            batTextures[2] = content.Load<Texture2D>("HUD/ZapBat");
            batTextures[3] = content.Load<Texture2D>("HUD/EarthBat");
            batRects[0] = new Rectangle(28, 48, 17, 25);
            batRects[1] = new Rectangle(92, 48, 17, 25);
            batRects[2] = new Rectangle(156, 48, 17, 25);
            batRects[3] = new Rectangle(220, 48, 17, 25);
            changeWeapon(0);

            for (int i = 0; i < batLife.Length; i++)
                batLife[i] = 100;

            numEnemies = initEnemies;
            enemies = "Enemies Remaining " + initEnemies;
        }

        public void initHUD(int totalEnemies)
        {
            this.totalEnemies = totalEnemies;
        }

        public void UpdateHealth(Player player)
        {
            scale = ((float)player.Health / player.MaxHealth);
            HP_RECT.Width = (int)(maxWidth * scale);

            hString = player.Health.ToString() + " / " + player.MaxHealth.ToString();
        }

        public void UpdateBattery(int current, int max, int weapon)
        {
            scale = ((float)current / max);
            batRects[weapon].Height = (int)(maxHeight * scale);

            batLife[weapon] = current;
        }

        public bool UpdateProgress()
        {
            progress += (float)toNextLevel / (float)totalEnemies;



            XP_RECT.Width = (int)(250f * (progress / (float)toNextLevel));

            return (progress >= toNextLevel);
        }

        public void ResetProgress()
        {
            XP_RECT.Width = 0;
            progress = 0;
        }

        public void UpdateEnemies(int newEnemies)
        {
            numEnemies = newEnemies;
            enemies = "Enemies Remaining " + numEnemies;
        }

        public void changeWeapon(int currentWeapon)
        {
            for (int i = 0; i < batColor.Length; i++)
                batColor[i] = new Color(tint, tint, tint);

            batColor[currentWeapon] = Color.White;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(hud, hud_rect, Color.White);
            spriteBatch.Draw(HP, HP_RECT, Color.White);
            spriteBatch.Draw(XP, XP_RECT, Color.White);

            for (int i = 0; i < batRects.Length; i++)
            {
                spriteBatch.Draw(batTextures[i], batRects[i], batColor[i]);
                spriteBatch.DrawString(font, batLife[i].ToString(), new Vector2(batRects[i].X, 75), Color.Black);
            }

            hsPos = new Vector2(HP_RECT.X + (HP.Width / 2) - (font.MeasureString(hString).X / 2), HP_RECT.Y + (HP.Height / 2) - (font.MeasureString(hString).Y / 2));
            spriteBatch.DrawString(font, hString, hsPos, Color.White);
            spriteBatch.DrawString(font, enemies, new Vector2(400, hsPos.Y), Color.Black);
        }
    }
}
