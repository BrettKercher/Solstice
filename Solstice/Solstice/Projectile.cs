using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Solstice
{
    class Projectile
    {
        Texture2D texture;
        Vector2 position;
        Vector2 origin;
        float speed;
        Vector2 velocity;
        String type;
        int damage;
        int maxDamage;
        Rectangle projRect;
        Boolean alive;
        Vector2 direction;
        float maxSpeed;

        const int projWidth = 32;
        const int projHeight = 32;

        public Boolean Alive
        {
            get { return alive; }
            set { alive = value; }
        }
        public Rectangle ProjRect
        {
            get { return projRect; }
        }
        public int Damage
        {
            get { return damage; }
        }

        public String Type
        {
            get { return type; }
        }

        public Projectile(String type, Vector2 origin, Vector2 direction, Texture2D texture, int size)
        {
            alive = true;
            this.texture = texture;
            this.type = type;
            this.direction = direction;
            this.origin = origin + (direction * size);
            this.position = new Vector2(this.origin.X - texture.Width / 2, this.origin.Y - texture.Height / 2);
            projRect = new Rectangle((int)this.position.X, (int)this.position.Y, projWidth, projHeight);
            
            switch (type)
            {
                case "Bullet":
                    speed = 1500f;
                    maxSpeed = 1500f;
                    damage = 3;
                break;

                case "Fire":
                    speed = 1500f;
                    maxSpeed = 1500f;
                    damage = 5;
                break;
                case "Zap":
                    speed = 2500f;
                    maxSpeed = 2500f;
                    damage = 4;
                break;
                case "Earth":
                    speed = 500f;
                    maxSpeed = 500f;
                    damage = 6;
                    maxDamage = 6;
                break;
                case "Ice":
                    speed = 1500f;
                    maxSpeed = 1500f;
                    damage = 5;
                break;
                case "Enemy":
                    speed = 400f;
                    damage = 2;
                break;
                default:
                    speed = 500f;
                    damage = 2;
                    break;
            }
        }

        public Boolean checkCollision(Texture2D text, Rectangle rect)
        {
            /*if (projRect.Intersects(entityRect))
                return true;
            else
                return false;*/
            //return projRect.Intersects(rect);
            return PerPixelCollision(text, rect);
        }

        public bool PerPixelCollision(Texture2D text, Rectangle rect)
        {
            //First check basic rect collision
            if (!projRect.Intersects(rect))
                return false;

            // Get Color data of each Texture
            Color[] bitsA = new Color[texture.Width * texture.Height];
            texture.GetData(bitsA);
            Color[] bitsB = new Color[text.Width * text.Height];
            text.GetData(bitsB);

            // Calculate the intersecting projRect
            int x1 = Math.Max(projRect.X, rect.X);
            int x2 = Math.Min(projRect.X + projRect.Width, rect.X + rect.Width);

            int y1 = Math.Max(projRect.Y, rect.Y);
            int y2 = Math.Min(projRect.Y + projRect.Height, rect.Y + rect.Height);

            // For each single pixel in the intersecting projRect
            for (int y = y1; y < y2; ++y)
            {
                for (int x = x1; x < x2; ++x)
                {
                    // Get the color from each texture
                    Color a = bitsA[(x - projRect.X) + (y - projRect.Y) * texture.Width];
                    Color b = bitsB[(x - rect.X) + (y - rect.Y) * text.Width];

                    if (a.A != 0 && b.A != 0) // If both colors are not transparent (the alpha channel is not 0), then there is a collision
                    {
                        return true;
                    }
                }
            }
            // If no collision occurred by now, we're clear.
            return false;
        }


        public void Update(GameTime gameTime, Map map)
        {
            float scale;
            if (type == "Earth")
            {
                if (speed > 0)
                {
                    speed -= 4;
                    scale = speed / maxSpeed;
                }
                else
                {
                    speed = 0;
                    scale = 0;
                }

                damage = (int)Math.Floor((float)maxDamage * scale);
            }

            Vector2 toPosition;
            velocity = direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            toPosition = position + velocity;

            if (type != "Earth")
            {
                if (map.checkCollision(new Rectangle((int)toPosition.X, (int)position.Y, texture.Width, texture.Height)))
                    alive = false;
                position += velocity;
            }
            else
            {
                if (map.checkCollision(new Rectangle((int)toPosition.X, (int)position.Y, texture.Width, texture.Height)))
                {
                    direction.X *= -1;
                }
                if (map.checkCollision(new Rectangle((int)position.X, (int)toPosition.Y, texture.Width, texture.Height)))
                {
                    direction.Y *= -1;
                }

                velocity = direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                position += velocity;
            }

            projRect.X = (int)position.X;
            projRect.Y = (int)position.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, projRect, Color.White);
        }



    }
}
