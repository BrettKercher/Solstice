using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Solstice
{
    class Particles
    {

        public Texture2D texture;       // The texture that will be drawn to represent the particle
        public Vector2 position;       // The current position of the particle        
        public Vector2 velocity;       // The speed of the particle at the current instance
        public float angle;           // The current angle of rotation of the particle
        public float angularVelocity;   // The speed that the angle is changing
        public Color color;           // The color of the particle
        public float size;               // The size of the particle
        public TimeSpan lifeSpan;               // The 'time to live' of the particle
        Rectangle sourceRectangle;
        Vector2 origin;

        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
        }



        public Particles(Texture2D tex, Vector2 pos, Vector2 vel,
            float ang, float angVel, Color col, float sz, TimeSpan life)
        {
            texture = tex;
            position = pos;
            velocity = vel;
            angle = ang;
            angularVelocity = angVel;
            color = col;
            size = sz;
            lifeSpan = life;
        }

        public void Update(GameTime gameTime, Map map, HUD hud, Player player)
        {
            lifeSpan -= gameTime.ElapsedGameTime;
            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            angle += angularVelocity;// * (float)gameTime.ElapsedGameTime.TotalSeconds;
            sourceRectangle = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            origin = new Vector2(position.X + (texture.Width / 2), position.Y + (texture.Height / 2));

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(texture, sourceRectangle, null, color, angle, Vector2.Zero, SpriteEffects.None, 0f);

            //spriteBatch.Draw(texture, sourceRectangle, color);
            spriteBatch.Draw(texture, position, null, color, angle, Vector2.Zero, size, SpriteEffects.None, 0f);
        }


    }
}
