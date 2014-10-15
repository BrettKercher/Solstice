using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Solstice
{

    class Player
    {

        Texture2D playerSheet;
        Texture2D projectileSheet;
        const int projSize = 32;
        Texture2D texture;
        Vector2 position;
        Vector2 origin;
        Vector2 velocity;
        Vector2 toPosition;
        Rectangle animRect;
        Rectangle playerRect;
        Rectangle hitBox;
        Vector2 direction;
        int health;
        int maxHealth;

        TimeSpan poisonTimer;

        int[] batteryLife = {100, 100, 100, 100};
        int[] maxBatteryLife = { 100, 100, 100, 100 };
        int currentWeapon = 0; //0 = Fire, 1 = Ice, 2 = Lightning, 3 = Earth
        Texture2D[] bulletTextures = new Texture2D[5];
        Rectangle bulletArea;

        Vector2 playerSize = new Vector2(56, 56);

        List<Projectile> projectiles;

        const float speed = 325.0f;

        public Vector2 Position
        {
            get { return position; }
        }
        public Texture2D Texture
        {
            get { return texture; }
        }
        public Rectangle PlayerRectangle
        {
            get { return playerRect; }
        }
        public Rectangle HitBox
        {
            get { return hitBox; }
        }
        public List<Projectile> Projectiles
        {
            get { return projectiles; }
        }
        public int Health
        {
            get { return health; }
        }
        public int MaxHealth
        {
            get { return maxHealth; }
        }
        public Vector2 Origin
        {
            get { return origin; }
        }


        public Player()
        {
            maxHealth = 10;
            health = 10;
        }

        public void Load(Texture2D spriteSheet1, Texture2D spriteSheet2, Map map, Random random)
        {
            playerSheet = spriteSheet1;
            projectileSheet = spriteSheet2;

            for (int i = 0; i < bulletTextures.Length; i++)
            {
                bulletArea = new Rectangle(i * projSize, 0, projSize, projSize);
                bulletTextures[i] = CropAnimation(bulletArea, projectileSheet);
            }

            direction = Vector2.Zero;
            animRect = new Rectangle(0, 0, (int)playerSize.X, (int)playerSize.Y);
            texture = CropAnimation(animRect, playerSheet);
            projectiles = new List<Projectile>();
            poisonTimer = new TimeSpan(0, 0, 1);

            initPosition(map, random);
        }

        public void initPosition(Map map, Random random)
        {
            int posX = (int)random.Next(3, (int)map.Dimensions.X - 3);
            int posY = (int)random.Next(3, (int)map.Dimensions.X - 3);

            while (map.IntMap[(int)posX, (int)posY] != 1)
            {
                posX = (int)random.Next(3, (int)map.Dimensions.X - 3);
                posY = (int)random.Next(3, (int)map.Dimensions.X - 3);
            }

            position = new Vector2(posX * map.TileSize, posY * map.TileSize);
            origin = new Vector2(position.X + texture.Width / 2, position.Y + texture.Height / 2);

            velocity = Vector2.Zero;
            playerRect = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            hitBox = new Rectangle((int)position.X + 3, (int)position.Y + 3, texture.Width - 3, texture.Height - 3);
        }

        public void TakeDamage(int damage, HUD hud)
        {
            health -= damage;
            //Update the HUD when you get hit
            hud.UpdateHealth(this);
        }
        public void Poisoned(int damage, HUD hud, GameTime gameTime)
        {
            poisonTimer += gameTime.ElapsedGameTime;

            if (poisonTimer >= new TimeSpan(0,0,1))
            {
                poisonTimer = TimeSpan.Zero;
                health -= damage;
                hud.UpdateHealth(this);
            }
        }

        private Texture2D CropAnimation(Rectangle animArea, Texture2D spriteSheet)
        {
            Texture2D croppedImage = new Texture2D(spriteSheet.GraphicsDevice, animArea.Width, animArea.Height);
            Color[] spriteSheetData = new Color[spriteSheet.Width * spriteSheet.Height];
            Color[] croppedImageData = new Color[croppedImage.Width * croppedImage.Height];

            spriteSheet.GetData<Color>(spriteSheetData);

            int index = 0;
            for (int i = animArea.Y; i < animArea.Y + animArea.Height; i++)
            {
                for (int j = animArea.X; j < animArea.X + animArea.Width; j++)
                {
                    croppedImageData[index] = spriteSheetData[i * spriteSheet.Width + j];
                    index++;
                }
            }
            croppedImage.SetData<Color>(croppedImageData);

            return croppedImage;
        }

        public void RemoveProjectiles()
        {
            for (int i = 0; i < projectiles.Count(); i++)
            {
                projectiles[i].Alive = false;
            }
        }

        public void UpdateProjectiles(GameTime gameTime, Map map, List<Enemies> enemies)
        {
            foreach (Projectile proj in projectiles)
                proj.Update(gameTime, map);

            foreach (Projectile proj in projectiles)
            {
                foreach (Enemies enemy in enemies)
                {
                    if (enemy.Life == Enemies.Living.Alive)
                    {
                        if (proj.checkCollision(enemy.Texture, enemy.EnemyRect))
                        {
                            proj.Alive = false;
                            enemy.TakeDamage(proj.Damage, currentWeapon, batteryLife[currentWeapon]);
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < projectiles.Count; i++)
                if (!projectiles[i].Alive)
                    projectiles.Remove(projectiles[i]);
        }

        public void Update(GameTime gameTime, KeyboardState currentKey, KeyboardState previousKey, MouseState currentMouse,
            MouseState previousMouse, Map map, Camera camera, List<Enemies> enemies, HUD hud, SoundEffect gunShot)
        {
            SoundEffectInstance shot = gunShot.CreateInstance();
            shot.Volume = .3f;

            direction = Vector2.Zero;

            if (currentKey.IsKeyDown(Keys.W))
                direction += new Vector2(0, -1);
            if (currentKey.IsKeyDown(Keys.S))
                direction += new Vector2(0, 1);
            if (currentKey.IsKeyDown(Keys.A))
                direction += new Vector2(-1, 0);
            if (currentKey.IsKeyDown(Keys.D))
                direction += new Vector2(1, 0);

            velocity = direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;


            toPosition = position + velocity;
            //To position is where we are trying to move, position is our current possition

            if(!map.checkCollision(new Rectangle((int)toPosition.X, (int)position.Y, texture.Width, texture.Height)))
                position.X += velocity.X;
            if (!map.checkCollision(new Rectangle((int)position.X, (int)toPosition.Y, texture.Width, texture.Height)))
                position.Y += velocity.Y;
            origin = new Vector2(position.X + (texture.Width / 2), position.Y + (texture.Height / 2));
            playerRect.X = (int)position.X;
            playerRect.Y = (int)position.Y;
            hitBox.X = (int)position.X + 3;
            hitBox.Y = (int)position.Y + 3;


            UpdateProjectiles(gameTime, map, enemies);

            //Change Weapons
            if (currentKey.IsKeyDown(Keys.E) && previousKey.IsKeyUp(Keys.E))
            {
                if (currentWeapon != bulletTextures.Length - 2)
                    currentWeapon++;
                else
                    currentWeapon = 0;
                hud.changeWeapon(currentWeapon);
            }
            else if (currentKey.IsKeyDown(Keys.Q) && previousKey.IsKeyUp(Keys.Q))
            {
                if (currentWeapon != 0)
                    currentWeapon--;
                else
                    currentWeapon = bulletTextures.Length - 2;
                hud.changeWeapon(currentWeapon);
            }


            //Create a new bullet on click
            if (currentMouse.LeftButton == ButtonState.Pressed)
            {
                Vector2 projDirection = new Vector2(currentMouse.X + camera.Center.X, currentMouse.Y + camera.Center.Y) - origin;

                if (projDirection != Vector2.Zero)
                    projDirection.Normalize();
                Projectile tempProj;

                if (previousMouse.LeftButton == ButtonState.Released)
                {
                    shot.Play();
                    switch (currentWeapon)
                    {
                        case 0:
                            if (batteryLife[currentWeapon] > 0)
                            {
                                tempProj = new Projectile("Fire", origin, projDirection, bulletTextures[currentWeapon], (texture.Width + texture.Height) / 2);
                                batteryLife[currentWeapon]--;
                                hud.UpdateBattery(batteryLife[currentWeapon], maxBatteryLife[currentWeapon], currentWeapon);
                            }
                            else
                            {
                                tempProj = new Projectile("Bullet", origin, projDirection, bulletTextures[4], (texture.Width + texture.Height) / 2);
                            }
                            break;
                        case 1:
                            if (batteryLife[currentWeapon] > 0)
                            {
                                tempProj = new Projectile("Ice", origin, projDirection, bulletTextures[currentWeapon], (texture.Width + texture.Height) / 2);
                                batteryLife[currentWeapon]--;
                                hud.UpdateBattery(batteryLife[currentWeapon], maxBatteryLife[currentWeapon], currentWeapon);
                            }
                            else
                            {
                                tempProj = new Projectile("Bullet", origin, projDirection, bulletTextures[4], (texture.Width + texture.Height) / 2);
                            }
                            break;
                        case 2:
                            if (batteryLife[currentWeapon] > 0)
                            {
                                tempProj = new Projectile("Zap", origin, projDirection, bulletTextures[currentWeapon], (texture.Width + texture.Height) / 2);
                                batteryLife[currentWeapon]--;
                                hud.UpdateBattery(batteryLife[currentWeapon], maxBatteryLife[currentWeapon], currentWeapon);
                            }
                            else
                            {
                                tempProj = new Projectile("Bullet", origin, projDirection, bulletTextures[4], (texture.Width + texture.Height) / 2);
                            }
                            break;
                        case 3:
                            if (batteryLife[currentWeapon] > 0)
                            {
                                tempProj = new Projectile("Earth", origin, projDirection, bulletTextures[currentWeapon], (texture.Width + texture.Height) / 2);
                                batteryLife[currentWeapon]--;
                                hud.UpdateBattery(batteryLife[currentWeapon], maxBatteryLife[currentWeapon], currentWeapon);
                            }
                            else
                            {
                                tempProj = new Projectile("Bullet", origin, projDirection, bulletTextures[4], (texture.Width + texture.Height) / 2);
                            }
                            break;
                        case 4:
                            tempProj = new Projectile("Bullet", origin, projDirection, bulletTextures[currentWeapon], (texture.Width + texture.Height) / 2);
                            break;
                        default:
                            tempProj = new Projectile("Bullet", origin, projDirection, bulletTextures[4], (texture.Width + texture.Height) / 2);
                            break;
                    }
                    if (!map.checkCollision(tempProj.ProjRect))
                        projectiles.Add(tempProj);
                }
                else
                {

                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(health > 0)
            spriteBatch.Draw(texture, playerRect, Color.White);

            foreach (Projectile proj in projectiles)
                proj.Draw(spriteBatch);
        }

    }

}
