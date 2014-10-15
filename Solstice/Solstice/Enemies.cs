using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Solstice
{
    class Enemies
    {

        String type;
        Vector2 position;
        Vector2 toPosition;
        Vector2 origin;
        Vector2 velocity;
        Vector2 direction;
        Rectangle enemyRect;
        Rectangle sourceRect;
        Texture2D texture;
        float speed;
        float maxSpeed;
        int health;
        int damage;
        ParticleEngine particleEngine;

        Texture2D EnemySheet;
        Texture2D projectileSheet;

        Texture2D particle;

        TimeSpan wanderDelay;
        int distance = 0;
        Vector2 startPos = Vector2.Zero;

        TimeSpan attackDelay;
        List<Projectile> projectiles;
        Texture2D[] bulletTextures = new Texture2D[4];
        Rectangle[] bulletRects = new Rectangle[4];
        Rectangle bulletArea;
        int projSize = 32;
        int weaponType;
        int aggroRange;

        const int blockSize = 64;

        const int size = 64;

        TimeSpan iFrames = TimeSpan.Zero;
        TimeSpan statusTimer = TimeSpan.Zero;
        TimeSpan perSecond = TimeSpan.Zero;
        Color statusColor = Color.White;

        public enum Status
        {
            Normal,
            Burning,
            Frozen,
            Paralyzed
        }
        Status currentStatus = Status.Normal;

        public enum Living
        {
            Alive,
            Dead
        }
        Living life = Living.Alive;

        public enum Resist
        {
            None,
            Fire,
            Ice,
            Lightning,
            Earth
        }
        Resist resist = Resist.None;

        public enum State
        {
            Aggressive,
            Passive,
            Stationary,
            Attack,
            Defensive,
            Pursue,
            Flee
        }
        State currentState = State.Passive;


        public int Health
        {
            get { return health; }
            set { health = value; }
        }
        public int Damage
        {
            get { return damage; }
        }
        public Rectangle EnemyRect
        {
            get { return enemyRect; }
        }
        public State CurrentState
        {
            get { return currentState; }
            set { currentState = value; }
        }
        public List<Projectile> Projectiles
        {
            get { return projectiles; }
        }
        public Living Life
        {
            get { return life; }
        }
        public Vector2 Position
        {
            get { return position; }
        }
        public Texture2D Texture
        {
            get { return texture; }
        }
        public ParticleEngine ParticleEng
        {
            get { return particleEngine; }
        }


        public Enemies(String type, Map map, Random random, Player player)
        {
            this.type = type;

            int randomMiliseconds = random.Next(0,5000);
            wanderDelay = new TimeSpan(0, 0, 0, 0, randomMiliseconds);

            attackDelay = new TimeSpan(0, 0, 0, 0, random.Next(5000,8000));

            projectiles = new List<Projectile>();

            switch (type)
            {
                    //init(sprite.x, sprite.y, size.x, size.y, speed, health, damage, weapon, range, starting state, resistance
                case "Slug":
                    init(0, 0, 1, 1, 100, 10, 2, 0, 300, State.Passive, Resist.None);
                break;
                case "Moth":
                    init(0, 1, 1, 1, 150, 10, 2, 0, 300, State.Passive, Resist.None);
                break;
                case "Worm":
                    init(0, 2, 1, 2, 100, 10, 2, 0, 300, State.Aggressive, Resist.None);
                break;
                case "Monkey":
                    init(0, 4, 1, 1, 100, 10, 2, 0, 300, State.Passive, Resist.None);
                break;
                case "Gorilla":
                    init(0, 5, 1, 1, 100, 10, 2, 0, 300, State.Passive, Resist.None);
                break;
            }


            int posX = (int)random.Next(3, (int)map.Dimensions.X - 3);
            int posY = (int)random.Next(3, (int)map.Dimensions.X - 3);

            while (map.IntMap[(int)posX, (int)posY] != 1)
            {
                posX = (int)random.Next(3, (int)map.Dimensions.X - 3);
                posY = (int)random.Next(3, (int)map.Dimensions.X - 3);
            }

            position = new Vector2(posX * map.TileSize, posY * map.TileSize);

            velocity = Vector2.Zero;
        }

        public void init(int x, int y, int xScale, int yScale, float spd, int hp, int dmg, int weapon, int range, State startState, Resist res)
        {
            sourceRect = new Rectangle((x * blockSize), (y * blockSize), (size * xScale), (size * yScale));
            speed = spd;
            maxSpeed = spd;
            health = hp;
            damage = dmg;
            weaponType = weapon;
            aggroRange = range;
            currentState = startState;
            resist = res;
        }

        public void Load(Texture2D spriteSheet01, Texture2D spriteSheet02, Texture2D particleSheet)
        {
            EnemySheet = spriteSheet01;
            projectileSheet = spriteSheet02;

            particle = particleSheet;

            texture = CropAnimation(sourceRect, EnemySheet);
            enemyRect = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            origin = new Vector2(position.X + texture.Width / 2, position.Y + texture.Height / 2);

            List<Texture2D> textures = new List<Texture2D>();
            textures.Add(particle);
            particleEngine = new ParticleEngine(textures, new Vector2(0, 0));

            for (int i = 0; i < bulletTextures.Length; i++)
            {
                bulletArea = new Rectangle(i * projSize, 0, projSize, projSize);
                bulletTextures[i] = CropAnimation(bulletArea, projectileSheet);
            }
        }

        /*
         * Crop the desired image out of a spritesheet
         */
        private Texture2D CropAnimation(Rectangle enemyArea, Texture2D spriteSheet)
        {
            Texture2D croppedImage = new Texture2D(spriteSheet.GraphicsDevice, enemyArea.Width, enemyArea.Height);
            Color[] spriteSheetData = new Color[spriteSheet.Width * spriteSheet.Height];
            Color[] croppedImageData = new Color[croppedImage.Width * croppedImage.Height];

            spriteSheet.GetData<Color>(spriteSheetData);

            int index = 0;
            for (int i = enemyArea.Y; i < enemyArea.Y + enemyArea.Height; i++)
            {
                for (int j = enemyArea.X; j < enemyArea.X + enemyArea.Width; j++)
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

        /* 
         * Check for collision with walls
         */
        public Boolean checkCollision(Rectangle entityRect)
        {
            if (enemyRect.Intersects(entityRect))
                return true;
            else
                return false;
        }

        public void TakeDamage(int damage, int weaponType, int batLife)
        {
            if (iFrames > TimeSpan.FromSeconds(0))
                return;
            this.health -= damage;

            switch (weaponType) //0 = fire, 1 = ice, 2 = Zap, 3 = Earth
            {
                case 0: //Fire
                    if (batLife > 0 && resist != Resist.Fire)
                    {
                        currentStatus = Status.Burning;
                        statusTimer = new TimeSpan(0, 0, 5);
                    }
                break;
                case 1: //Ice
                if (batLife > 0 && resist != Resist.Ice)
                {
                    currentStatus = Status.Frozen;
                    statusTimer = new TimeSpan(0, 0, 5);
                }
                break;
                case 2: //Zap
                if (batLife > 0 && resist != Resist.Lightning)
                {
                    currentStatus = Status.Paralyzed;
                    statusTimer = new TimeSpan(0, 0, 5);
                }
                break;
                case 3: //Earth
                if (batLife > 0)
                {
                    iFrames = new TimeSpan(0, 0, 0, 0, 200);
                }
                break;
            }
        }


        public void Die()
        {
            life = Living.Dead;
        }

        /*General Update method - called every 60 cycles
         * */
        public void Update(GameTime gameTime, Map map, Player player, Random random, HUD hud)
        {
            if (currentStatus != Status.Normal)
            {
                statusTimer -= gameTime.ElapsedGameTime;
                perSecond += gameTime.ElapsedGameTime;
            }
            if (statusTimer <= TimeSpan.FromSeconds(0))
                currentStatus = Status.Normal;

            if (iFrames > TimeSpan.FromSeconds(0))
                iFrames -= gameTime.ElapsedGameTime;
            
            if(currentStatus != Status.Frozen)
                speed = maxSpeed;

            switch (currentStatus)
            {
                case Status.Normal:
                    break;
                case Status.Burning:
                    if (perSecond > TimeSpan.FromSeconds(1))
                    {
                        perSecond = TimeSpan.Zero;
                        this.health--;
                    }
                break;
                case Status.Frozen:
                if (perSecond > TimeSpan.FromSeconds(1))
                {
                    perSecond = TimeSpan.Zero;
                    if (speed > 0)
                        this.speed -= 50;
                    else
                        this.speed = 0;
                }
                break;
            }

            UpdateColor();

            Move(map);

            UpdateProjectiles(gameTime, map, hud, player);

            if(type == "Moth")
            {
                particleEngine.EmitterLocation = new Vector2(origin.X, origin.Y);
                particleEngine.Update(gameTime, map, hud, player, life == Living.Alive);
            }

            if (life == Living.Alive)
            {
                switch (type)
                {
                    case "Slug":
                        UpdateSlug(gameTime, random, player, map);
                    break;
                    case "Moth":
                        UpdateMoth(gameTime, random, player, map);
                    break;
                    case "Worm":
                        UpdateWorm(gameTime, random, player, map);
                    break;
                    case "Monkey":
                        UpdateMonkey(gameTime, random, player, map);
                    break;
                    case "Gorilla":
                        UpdateSlug(gameTime, random, player, map);
                    break;
                }
                
            }

            
            
        }

        /* Update each enemy's color based on their status
         *      * Normal = White
         *      * Burned = Red
         *      * Freeze = Blue
         *      * Stunned = Yellow
         */
        public void UpdateColor()
        {
            if (currentStatus == Status.Normal)
                statusColor = Color.White;
            if (currentStatus == Status.Burning)
                statusColor = Color.Red;
            if (currentStatus == Status.Frozen)
                statusColor = Color.Cyan;
            if (currentStatus == Status.Paralyzed)
                statusColor = Color.Yellow;
        }

        /* Update each enemy's projectile list
         *      * Update their positions
         *      * Check for collision with the player
         *      * Remove the projectile from the list if alive flag is off
         */
        public void UpdateProjectiles(GameTime gameTime, Map map, HUD hud, Player player)
        {
            foreach (Projectile proj in projectiles)
                proj.Update(gameTime, map);

            foreach (Projectile proj in projectiles)
            {
                if (proj.checkCollision(player.Texture, player.PlayerRectangle))
                {
                    proj.Alive = false;
                    player.TakeDamage(proj.Damage, hud);
                    break;
                }
            }

            for (int i = 0; i < projectiles.Count; i++)
                if (!projectiles[i].Alive)
                    projectiles.Remove(projectiles[i]);
        }


        /* Update this enemy's position
         * Use toposition to get the desired location
         * Check the map to see if that position is solid or not, if not, update position
         * Update the enemy's source rect and origin
         */
        public void Move(Map map)
        {
            toPosition = position + velocity;

            if (!map.checkCollision(new Rectangle((int)toPosition.X, (int)position.Y, texture.Width, texture.Height)))
                position.X += velocity.X;
            else
                velocity.X *= -1;
            if (!map.checkCollision(new Rectangle((int)position.X, (int)toPosition.Y, texture.Width, texture.Height)))
                position.Y += velocity.Y;
            else
                velocity.Y *= -1;
            enemyRect.X = (int)position.X;
            enemyRect.Y = (int)position.Y;
            origin = new Vector2(position.X + (texture.Width / 2), position.Y + (texture.Height / 2));
        }

        /* Draw the Enemy to the screen,
         * As well as each of its projectiles
         */
        public void Draw(SpriteBatch spriteBatch)
        {
            if (type == "Moth")
                particleEngine.Draw(spriteBatch);

            if(life == Living.Alive)
                spriteBatch.Draw(texture, enemyRect, statusColor);

            foreach (Projectile proj in projectiles)
                proj.Draw(spriteBatch);
        }

        /* Specific update method for the Heart enemy
         * Switch based on its current state:
         * Stationary:
         *      The enemy will not move or attack
         * Passive:
         *      The enemy will wander around the map, not attacking
         * Aggressive:
         *      The enemy will wander aimlessly, shooting projectiles towards the player
         * Pursue:
         *      The enemy will follow the player
         * Flee:
         *      The enemy will run away from the player
         * 
         */
        public void UpdateMonkey(GameTime gameTime, Random random, Player player, Map map)
        {
            switch (currentState)
            {
                case State.Passive:
                    if ((player.Position - position).Length() < aggroRange)
                        currentState = State.Aggressive;

                    wanderDelay += gameTime.ElapsedGameTime;

                    if (wanderDelay > TimeSpan.FromMilliseconds(random.Next(0, 5000)))
                    {
                        //Random Angle
                        float angle = (float)random.Next(0, 360);
                        angle = (angle * (float)Math.PI) / 180;

                        //Random Magnitude
                        distance = random.Next(50, 200);

                        //Direction Vector
                        Vector2 dirVector = new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);
                        if (dirVector != Vector2.Zero)
                            dirVector.Normalize();

                        startPos = position;
                        velocity = dirVector * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        toPosition = position + velocity;

                        if (!map.checkCollision(new Rectangle((int)toPosition.X, (int)position.Y, texture.Width, texture.Height)))
                        {
                            wanderDelay = TimeSpan.Zero;
                        }
                    }

                    if ((startPos - position).Length() > distance)
                        velocity = Vector2.Zero;

                    break;

                case State.Aggressive:
                    if ((player.Position - position).Length() > 400)
                        currentState = State.Passive;

                    wanderDelay += gameTime.ElapsedGameTime;
                    attackDelay += gameTime.ElapsedGameTime;

                    if (wanderDelay > TimeSpan.FromMilliseconds(random.Next(1000, 5000)))
                    {
                        //Random Angle
                        float angle = (float)random.Next(0, 360);
                        angle = (angle * (float)Math.PI) / 180;

                        //Random Magnitude
                        distance = random.Next(50, 200);

                        //Direction Vector
                        Vector2 dirVector = new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);
                        if (dirVector != Vector2.Zero)
                            dirVector.Normalize();

                        startPos = position;
                        velocity = dirVector * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        toPosition = position + velocity;

                        if (!map.checkCollision(new Rectangle((int)toPosition.X, (int)position.Y, texture.Width, texture.Height)))
                        {
                            wanderDelay = TimeSpan.Zero;
                        }
                    }

                    if (attackDelay < TimeSpan.Zero)
                    {
                        if (currentStatus != Status.Paralyzed)
                        {
                            attackDelay = new TimeSpan(0,0,0,0,random.Next(5000,8000));
                            Vector2 projDirection = player.Origin - origin;

                            //Plus/Minus angle for accuracy
                            projDirection.X += random.Next(-200, 200);
                            projDirection.Y += random.Next(-200, 200);

                            if (projDirection != Vector2.Zero)
                                projDirection.Normalize();
                            Projectile tempProj;
                            switch (weaponType)
                            {
                                case 0:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[weaponType], (texture.Width + texture.Height) / 2);
                                    break;
                                case 1:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[weaponType], (texture.Width + texture.Height) / 2);
                                    break;
                                case 2:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[weaponType], (texture.Width + texture.Height) / 2);
                                    break;
                                case 3:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[weaponType], (texture.Width + texture.Height) / 2);
                                    break;
                                default:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[4], (texture.Width + texture.Height) / 2);
                                    break;
                            }

                            if (!map.checkCollision(tempProj.ProjRect))
                                projectiles.Add(tempProj);
                        }
                    }

                    if ((startPos - position).Length() > distance)
                        velocity = Vector2.Zero;

                    break;
            }
        }
        public void UpdateSlug(GameTime gameTime, Random random, Player player, Map map)
        {
            switch (currentState)
            {
                case State.Passive:
                    if ((player.Position - position).Length() < aggroRange)
                    {
                        currentState = State.Pursue;
                        speed = 250f;
                        maxSpeed = speed;
                    }

                    wanderDelay += gameTime.ElapsedGameTime;

                    if (wanderDelay > TimeSpan.FromMilliseconds(random.Next(0, 5000)))
                    {
                        //Random Angle
                        float angle = (float)random.Next(0, 360);
                        angle = (angle * (float)Math.PI) / 180;

                        //Random Magnitude
                        distance = random.Next(50, 200);

                        //Direction Vector
                        Vector2 dirVector = new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);
                        if (dirVector != Vector2.Zero)
                            dirVector.Normalize();

                        startPos = position;
                        velocity = dirVector * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        toPosition = position + velocity;

                        if (!map.checkCollision(new Rectangle((int)toPosition.X, (int)position.Y, texture.Width, texture.Height)))
                        {
                            wanderDelay = TimeSpan.Zero;
                        }
                    }

                    if ((startPos - position).Length() > distance)
                        velocity = Vector2.Zero;

                    break;

                case State.Pursue:
                    if ((player.Position - position).Length() >= aggroRange)
                    {
                        currentState = State.Passive;
                        speed = 100f;
                        maxSpeed = speed;
                    }
                    direction = player.Position - position;
                    if (direction != Vector2.Zero)
                        direction.Normalize();
                    velocity = direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    break;
            }
        }

        public void UpdateMoth(GameTime gameTime, Random random, Player player, Map map)
        {
            switch (currentState)
            {
                case State.Passive:

                    wanderDelay += gameTime.ElapsedGameTime;

                    if (wanderDelay > TimeSpan.FromMilliseconds(2000 + random.Next(0, 5000)))
                    {
                        //Random Angle
                        float angle = (float)random.Next(0, 360);
                        angle = (angle * (float)Math.PI) / 180;

                        //Random Magnitude
                        distance = random.Next(50, 200);

                        //Direction Vector
                        Vector2 dirVector = new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);
                        if (dirVector != Vector2.Zero)
                            dirVector.Normalize();

                        startPos = position;
                        velocity = dirVector * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                        toPosition = position + velocity;

                        if (!map.checkCollision(new Rectangle((int)toPosition.X, (int)position.Y, texture.Width, texture.Height)))
                        {
                            wanderDelay = TimeSpan.Zero;
                        }
                    }

                    //if ((startPos - position).Length() > distance)
                    //    velocity = Vector2.Zero;
                break;
            } 
        }

        public void UpdateWorm(GameTime gameTime, Random random, Player player, Map map)
        {
            //override default origin - to mouth of wormbro
            origin = new Vector2(position.X + texture.Width, position.Y + (texture.Width / 4));

            switch (currentState)
            {
                case State.Aggressive:

                    wanderDelay -= gameTime.ElapsedGameTime;

                    if (wanderDelay < TimeSpan.Zero)
                    {
                        wanderDelay = new TimeSpan(0, 0, 0, 0, random.Next(0, 5000));

                    }

                    attackDelay -= gameTime.ElapsedGameTime;

                    if (attackDelay < TimeSpan.Zero)
                    {
                        if (currentStatus != Status.Paralyzed)
                        {
                            attackDelay = new TimeSpan(0,0,0,0,random.Next(5000,8000));
                            Vector2 projDirection = player.Origin - origin;

                            //Plus/Minus angle for accuracy
                            projDirection.X += random.Next(-200, 200);
                            projDirection.Y += random.Next(-200, 200);

                            if (projDirection != Vector2.Zero)
                                projDirection.Normalize();
                            Projectile tempProj;
                            switch (weaponType)
                            {
                                case 0:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[weaponType], 0);
                                    break;
                                case 1:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[weaponType], 0);
                                    break;
                                case 2:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[weaponType], 0);
                                    break;
                                case 3:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[weaponType], 0);
                                    break;
                                default:
                                    tempProj = new Projectile("Enemy", origin, projDirection, bulletTextures[4], 0);
                                    break;
                            }

                            if (!map.checkCollision(tempProj.ProjRect))
                                projectiles.Add(tempProj);
                        }
                    }
                break;
            }
        }
    }
}
