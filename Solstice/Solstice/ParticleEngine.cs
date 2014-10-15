using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Solstice
{
    class ParticleEngine
    {
        private Random random;
        public Vector2 EmitterLocation { get; set; }
        private List<Particles> particles;
        private List<Texture2D> textures;

        public int ParticleCount
        {
            get { return particles.Count; }
        }

        public ParticleEngine(List<Texture2D> textures, Vector2 location)
        {
            EmitterLocation = location;
            this.textures = textures;
            this.particles = new List<Particles>();
            random = new Random();
        }

        
        public void Update(GameTime gameTime, Map map, HUD hud, Player player, Boolean alive)
        {
            int total = 15;

            if (alive)
            {
                for (int i = 0; i < total; i++)
                {
                    particles.Add(GenerateNewParticle());
                }
            }
            foreach (Particles particle in particles)
            {
                if (particle.SourceRectangle.Intersects(player.PlayerRectangle))
                {
                    player.Poisoned(1, hud, gameTime);
                    break;
                }
            }

            for (int particle = 0; particle < particles.Count; particle++)
            {
                particles[particle].Update(gameTime, map, hud, player);
                if (particles[particle].lifeSpan <= TimeSpan.Zero)
                {
                    particles.RemoveAt(particle);
                    particle--;
                }
            }
        }

        private Particles GenerateNewParticle()
        {
            Color color;
            Texture2D texture = textures[random.Next(textures.Count)];
            Vector2 position = EmitterLocation;
            Vector2 velocity = new Vector2(-10 + (float)(random.NextDouble() * 20), -10 +(float)(random.NextDouble() * 20));
            float angle = 0;
            float angularVelocity = 0.1f * (float)(random.NextDouble() * 2 - 1);

            double rng = random.NextDouble();
            /* Fire 
            if(rng < .5)
                color = new Color(200 + random.Next(55),0, 0);
            else if (rng < .85)
                color = new Color(235 + random.Next(20), 69, 0);
            else
                color = new Color(235 + random.Next(20), 235 + random.Next(20), 0);
            */

            /* Poison */
            if(random.NextDouble() < .2)
                color = new Color(0, 200 + random.Next(55), random.Next(100));
            else
                color = new Color(150 + random.Next(20),27 + random.Next(10), 230 + random.Next(25));

            float size = (float)random.NextDouble();
            TimeSpan ttl = new TimeSpan(0, 0, 0, 0, random.Next(5000));

            return new Particles(texture, position, velocity, angle, angularVelocity, color, size, ttl);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int index = 0; index < particles.Count; index++)
            {
                particles[index].Draw(spriteBatch);
            }
        }
    }
}
