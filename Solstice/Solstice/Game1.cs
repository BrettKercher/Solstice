using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Krypton;
using Krypton.Lights;

namespace Solstice
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        enum GameState
        {
            inSplash,
            inMenu,
            inGame,
            inTransition,
            inSettings,
            inTutorial,
            inScores,
            inOther,
            Paused
        }
        GameState gameState = GameState.inSplash;

        enum MenuSelection
        {
            Play,
            Tutorial,
            Settings,
            Scores,
            Exit
        }
        MenuSelection menuSelection = MenuSelection.Play;


        #region GlobalVariables

        public KeyboardState currentKey, previousKey;
        public MouseState currentMouse, previousMouse;
        public Vector2 mousePosition;
        Vector2 dimensions;
        Random random = new Random();

        #endregion GlobalVariables

        #region SplashVariables

        Texture2D tutScreen;
        Texture2D splashScreen;
        TimeSpan splashCounter = TimeSpan.Zero;

        #endregion SplashVariables

        #region MenuVariables

        SpriteFont menuFont;

        #endregion MenuVariables

        #region GameVariables


        KryptonEngine LightEngine;
        Texture2D lightTexture;

        Map map;
        Player player;
        Camera camera;
        HUD hud;
        List<Enemies> enemies;
        List<Items> items;
        int numEnemies;
        int level = 1;
        int world = 1;
        bool nextLevel = false;

        #endregion GameVariables

        #region SFXs

        SoundEffect gunShot;

        #endregion SFXs


        #region SpriteSheets

        Texture2D EnemySheet;
        Texture2D LargeTileSheet;
        Texture2D PlayerSheet;
        Texture2D ProjectileSheet;
        Texture2D SmallTileSheet;
        Texture2D TileSheet;
        Texture2D ItemSheet;
        Texture2D ParticleSheet;

        #endregion SpriteSheets

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = false;
            IsMouseVisible = true;

            // Create Krypton
            this.LightEngine = new KryptonEngine(this, "KryptonEffect");

            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            dimensions = new Vector2(960, 640);
            currentKey = Keyboard.GetState();
            previousKey = currentKey;
            currentMouse = Mouse.GetState();
            previousMouse = currentMouse;
            mousePosition = new Vector2(currentMouse.X, currentMouse.Y);

            LightEngine.Initialize();
            LightEngine.AmbientColor = new Color(196, 196, 196);
            LightEngine.SpriteBatchCompatablityEnabled = true;
            LightEngine.CullMode = CullMode.None;
            // Create a new simple point light texture to use for the lights
            lightTexture = LightTextureBuilder.CreatePointLight(this.GraphicsDevice, 512);

            graphics.PreferredBackBufferWidth = (int)dimensions.X;
            graphics.PreferredBackBufferHeight = (int)dimensions.Y;
            graphics.ApplyChanges();

           MediaPlayer.Volume = .3f;

            base.Initialize();
        }

        public void initializeGame()
        {
            camera = new Camera(GraphicsDevice);
            map = new Map(random);
            map.Load(LargeTileSheet);
            player = new Player();
            player.Load(PlayerSheet, ProjectileSheet, map, random);
            enemies = new List<Enemies>();
            hud = new HUD();
            hud.Load(Content, numEnemies);
            items = new List<Items>();
            generateEnemies(level);
            CreateLight(LightEngine, lightTexture, new Color(96, 96, 96), 1f, 100, new Vector2(1000,1000));
            CreateLight(LightEngine, lightTexture, new Color(255, 255, 255), 1f, 100, new Vector2(1200, 1000));
            CreateHull(10, 10);
        }


        public void CreateLight(KryptonEngine engine, Texture2D texture, Color color, float intensity, float range, Vector2 position)
        {
            Light2D light = new Light2D()
            {
                Texture = texture,
                Range = range,
                Color = color,
                Intensity = intensity,
                Angle = MathHelper.TwoPi * 2,
                X = position.X,
                Y = position.Y,
            };

            light.Fov = MathHelper.TwoPi;
            engine.Lights.Add(light);
        }

        private void CreateHull(int x, int y)
        {
            float w = 50;
            float h = 50;

            // Make lines of lines of hulls!
            for (int j = 0; j < y; j++)
            {
                // Make lines of hulls!
                for (int i = 0; i < x; i++)
                {
                    var posX = ((i * w) / x) - w / 2 + (j % 2 == 0 ? w / x / 2 : 0);
                    var posY = ((j * h) / y) - h / 2 + (i % 2 == 0 ? h / y / 4 : 0);

                    var hull = ShadowHull.CreateRectangle(Vector2.One);
                    hull.Position.X = posX;
                    hull.Position.Y = posY;
                    hull.Scale.X = (float)(this.random.NextDouble() * 0.75f + 0.25f);
                    hull.Scale.Y = (float)(this.random.NextDouble() * 0.75f + 0.25f);

                    LightEngine.Hulls.Add(hull);
                }
            }
        }

        private void DebugDraw()
        {
            this.LightEngine.RenderHelper.Effect.CurrentTechnique = this.LightEngine.RenderHelper.Effect.Techniques["DebugDraw"];
            this.GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                FillMode = FillMode.WireFrame,
            };
            if (Keyboard.GetState().IsKeyDown(Keys.H))
            {
                // Clear the helpers vertices
                this.LightEngine.RenderHelper.ShadowHullVertices.Clear();
                this.LightEngine.RenderHelper.ShadowHullIndicies.Clear();

                foreach (var hull in LightEngine.Hulls)
                {
                    this.LightEngine.RenderHelper.BufferAddShadowHull(hull);
                }


                foreach (var effectPass in LightEngine.RenderHelper.Effect.CurrentTechnique.Passes)
                {
                    effectPass.Apply();
                    this.LightEngine.RenderHelper.BufferDraw();
                }
            }

            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {
                this.LightEngine.RenderHelper.ShadowHullVertices.Clear();
                this.LightEngine.RenderHelper.ShadowHullIndicies.Clear();

                foreach (Light2D light in LightEngine.Lights)
                {
                    this.LightEngine.RenderHelper.BufferAddBoundOutline(light.Bounds);
                }

                foreach (var effectPass in LightEngine.RenderHelper.Effect.CurrentTechnique.Passes)
                {
                    effectPass.Apply();
                    this.LightEngine.RenderHelper.BufferDraw();
                }
            }
        }

        public void generateEnemies(int curLevel)
        {
            Enemies tempEnemy;
            numEnemies = random.Next(5 + (curLevel * curLevel), 8 + (curLevel * curLevel));
            for (int i = 0; i < numEnemies; i++)
            {
                float enemyType = (float)random.NextDouble();

                if(enemyType < .2)
                    tempEnemy = new Enemies("Slug", map, random, player);
                else if(enemyType < .6)
                    tempEnemy = new Enemies("Moth", map, random, player);
                else if(enemyType < .8)
                    tempEnemy = new Enemies("Worm", map, random, player);
                else if (enemyType < .9)
                    tempEnemy = new Enemies("Monkey", map, random, player);
                else
                    tempEnemy = new Enemies("Gorilla", map, random, player);

                tempEnemy.Load(EnemySheet, ProjectileSheet, ParticleSheet);

                if ((tempEnemy.Position - player.Position).Length() > 300)
                    enemies.Add(tempEnemy);
                else
                    i--;
            }

            hud.initHUD(numEnemies);
        }

        public void NextLevel()
        {
            if (level == 4)
            {
                level = 1;
                world++;
            }
            else
                level++;

            map = new Map(random);
            map.Load(LargeTileSheet);
            generateEnemies(((world - 1) * 4) + level);
            hud.UpdateEnemies(numEnemies);
            player.initPosition(map, random);
            nextLevel = false;
            hud.ResetProgress();
            player.RemoveProjectiles();
            foreach (Enemies enemy in enemies)
                enemy.RemoveProjectiles();
            for (int i = 0; i < items.Count(); i++)
                items.Remove(items[i]);
            gameState = GameState.inTransition;
        }

        public void AddDrops(int i)
        {
            Items soul = new Items(ItemSheet, "Soul", enemies[i].Position);

            items.Add(soul);
        }

        public void UseItem(String type)
        {
            switch (type)
            {
                case "Soul":
                    nextLevel = hud.UpdateProgress();
                break;
            }
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);


            EnemySheet = Content.Load<Texture2D>("SpriteSheets/EnemySheet");
            LargeTileSheet = Content.Load<Texture2D>("SpriteSheets/LargeTileSheet");
            PlayerSheet = Content.Load<Texture2D>("SpriteSheets/PlayerSheet");
            ProjectileSheet = Content.Load<Texture2D>("SpriteSheets/ProjectileSheet");
            SmallTileSheet = Content.Load<Texture2D>("SpriteSheets/SmallTileSheet");
            TileSheet = Content.Load<Texture2D>("SpriteSheets/TileSheet");
            ItemSheet = Content.Load<Texture2D>("SpriteSheets/ItemSheet");
            ParticleSheet = Content.Load<Texture2D>("particle_base");

            splashScreen = Content.Load<Texture2D>("Splash");
            tutScreen = Content.Load<Texture2D>("Tutorial");

            gunShot = Content.Load<SoundEffect>("SFX/gunShot");

            menuFont = Content.Load<SpriteFont>("SpriteFonts/MenuFont");
            
        }


        protected override void UnloadContent()
        {

        }

        #region UpdateMethods

        private void UpdateSplash(GameTime gameTime)
        {
            splashCounter += gameTime.ElapsedGameTime;

            if ((currentKey.IsKeyDown(Keys.Enter) && previousKey.IsKeyUp(Keys.Enter)) || (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released) || (splashCounter > TimeSpan.FromSeconds(2)))
            {
                gameState = GameState.inMenu;
                splashCounter = TimeSpan.Zero;
            }

        }
        private void UpdateMenu(GameTime gameTime)
        {

            if (currentKey.IsKeyDown(Keys.Down) && previousKey.IsKeyUp(Keys.Down))
                switch(menuSelection)
                {
                    case MenuSelection.Play:
                        menuSelection = MenuSelection.Tutorial;
                    break;
                    case MenuSelection.Tutorial:
                        menuSelection = MenuSelection.Exit;
                    break;
                    //case MenuSelection.Settings:
                    //    menuSelection = MenuSelection.Scores;
                    //break;
                    //case MenuSelection.Scores:
                    //    menuSelection = MenuSelection.Exit;
                    //break;
                    case MenuSelection.Exit:
                        menuSelection = MenuSelection.Play;
                    break;
                }
            else if (currentKey.IsKeyDown(Keys.Up) && previousKey.IsKeyUp(Keys.Up))
                switch(menuSelection)
                {
                    case MenuSelection.Play:
                        menuSelection = MenuSelection.Exit;
                    break;
                    case MenuSelection.Tutorial:
                    menuSelection = MenuSelection.Play;
                    break;
                    //case MenuSelection.Settings:
                    //    menuSelection = MenuSelection.Tutorial;
                    //break;
                    //case MenuSelection.Scores:
                    //    menuSelection = MenuSelection.Settings;
                    //break;
                    case MenuSelection.Exit:
                        menuSelection = MenuSelection.Tutorial;
                    break;
                }
            else if (currentKey.IsKeyDown(Keys.Enter) && previousKey.IsKeyUp(Keys.Enter))
                switch (menuSelection)
                {
                    case MenuSelection.Play:
                        gameState = GameState.inGame;
                        initializeGame();
                        break;
                    case MenuSelection.Tutorial:
                        gameState = GameState.inTutorial;
                        break;
                    //case MenuSelection.Settings:
                    //    gameState = GameState.inSettings;
                    //    break;
                    //case MenuSelection.Scores:
                    //    gameState = GameState.inSettings;
                    //    break;
                    case MenuSelection.Exit:
                        this.Exit();
                        break;
                }
        }
        private void UpdateGame(GameTime gameTime)
        {
            //Update Player
            player.Update(gameTime, currentKey, previousKey, currentMouse, previousMouse, map, camera, enemies, hud, gunShot);


            //check for picked up items
            for (int i = 0; i < items.Count; i++)
            {
                items[i].Update(gameTime, player);
                if (items[i].Collected)
                {
                    UseItem(items[i].Type);
                    items.Remove(items[i]);
                }
            }

            //Check for dead enemies
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(gameTime, map, player, random, hud);
                if (enemies[i].Health <= 0 && enemies[i].Life == Enemies.Living.Alive)
                {
                    enemies[i].Die();
                    AddDrops(i);
                    if (enemies[i].Projectiles.Count == 0 && enemies[i].ParticleEng.ParticleCount == 0)
                        enemies.Remove(enemies[i]);
                    numEnemies--;
                    hud.UpdateEnemies(numEnemies);
                }
                else if(enemies[i].Life == Enemies.Living.Dead)
                    if (enemies[i].Projectiles.Count == 0 && enemies[i].ParticleEng.ParticleCount == 0)
                    {
                        enemies.Remove(enemies[i]);
                    }
            }

            Light2D tempLight = (Light2D)LightEngine.Lights[0];
            tempLight.Position = new Vector2(player.Position.X + (player.Texture.Width / 2),player.Position.Y + (player.Texture.Height / 2));
            LightEngine.Lights[0] = tempLight;

            //Update Camera
            camera.Update(player.PlayerRectangle.X, player.PlayerRectangle.Y, player.Texture.Width, player.Texture.Height, new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), map);


            if (currentKey.IsKeyDown(Keys.F) && nextLevel)
                NextLevel();
        }
        private void UpdateTransition(GameTime gameTime)
        {
            if (currentKey.IsKeyDown(Keys.P) && previousKey.IsKeyUp(Keys.P))
            {
                gameState = GameState.inGame;
            }
        }
        private void UpdateSettings(GameTime gameTime)
        {
            if (currentKey.IsKeyDown(Keys.Escape) && previousKey.IsKeyUp(Keys.Escape))
                gameState = GameState.inMenu;
        }
        private void UpdatePaused(GameTime gameTime)
        {
        }

        private void UpdateTutorial(GameTime gameTime)
        {
            if (currentKey.IsKeyDown(Keys.Enter) && previousKey.IsKeyUp(Keys.Enter))
            {
                gameState = GameState.inMenu;
            }
        }

        protected override void Update(GameTime gameTime)
        {

            currentKey = Keyboard.GetState();
            currentMouse = Mouse.GetState();
            mousePosition = new Vector2(currentMouse.X, currentMouse.Y);

            if (currentKey.IsKeyDown(Keys.Escape) && previousKey.IsKeyUp(Keys.Escape))
                this.Exit();

            if (gameState == GameState.inGame)
            {
                UpdateGame(gameTime);
            }
            else if (gameState == GameState.inTutorial)
            {
                UpdateTutorial(gameTime);
            }
            else if (gameState == GameState.inTransition)
            {
                UpdateTransition(gameTime);
            }
            else if (gameState == GameState.Paused)
            {
                UpdatePaused(gameTime);
            }
            else if (gameState == GameState.inSplash)
            {
                UpdateSplash(gameTime);
            }

            else if (gameState == GameState.inMenu)
            {
                UpdateMenu(gameTime);
            }

            else if (gameState == GameState.inSettings)
            {
                UpdateSettings(gameTime);
            }

            base.Update(gameTime);

            previousKey = currentKey;
            previousMouse = currentMouse;

        }

        #endregion UpdateMethods


        #region DrawMethods

        private void DrawSplash(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(splashScreen, new Rectangle(0, 0, (int)dimensions.X, (int)dimensions.Y), Color.White);

        }
        private void DrawMenu(SpriteBatch spriteBatch)
        {
            String play = "Play";
            String tutorial = "Tutorial";
            //String settings = "Settings";
            //String score = "HighScores";
            String exit = "Quit";


            if(menuSelection == MenuSelection.Play)
                spriteBatch.DrawString(menuFont, play, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(play).X / 2), (graphics.PreferredBackBufferHeight / 2)), Color.Red);
            else
                spriteBatch.DrawString(menuFont, play, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(play).X / 2), (graphics.PreferredBackBufferHeight / 2)), Color.White);

            if (menuSelection == MenuSelection.Tutorial)
                spriteBatch.DrawString(menuFont, tutorial, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(tutorial).X / 2), (graphics.PreferredBackBufferHeight / 2) + 30), Color.Red);
            else
                spriteBatch.DrawString(menuFont, tutorial, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(tutorial).X / 2), (graphics.PreferredBackBufferHeight / 2) + 30), Color.White);
            
            /*
            if (menuSelection == MenuSelection.Settings)
                spriteBatch.DrawString(menuFont, settings, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(settings).X / 2), (graphics.PreferredBackBufferHeight / 2) + 60), Color.Red);
            else
                spriteBatch.DrawString(menuFont, settings, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(settings).X / 2), (graphics.PreferredBackBufferHeight / 2) + 60), Color.White);
            
            if (menuSelection == MenuSelection.Scores)
                spriteBatch.DrawString(menuFont, score, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(score).X / 2), (graphics.PreferredBackBufferHeight / 2) + 90), Color.Red);
            else
                spriteBatch.DrawString(menuFont, score, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(score).X / 2), (graphics.PreferredBackBufferHeight / 2) + 90), Color.White);
            */

            if (menuSelection == MenuSelection.Exit)
                spriteBatch.DrawString(menuFont, exit, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(exit).X / 2), (graphics.PreferredBackBufferHeight / 2) + 60), Color.Red);
            else
                spriteBatch.DrawString(menuFont, exit, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(exit).X / 2), (graphics.PreferredBackBufferHeight / 2) + 60), Color.White);
        }
        private void DrawGame(SpriteBatch spriteBatch)
        {
            map.DrawFloor(spriteBatch);
            player.Draw(spriteBatch);

            foreach (Enemies enemy in enemies)
                enemy.Draw(spriteBatch);
            foreach (Items item in items)
                item.Draw(spriteBatch);
            map.DrawWall(spriteBatch);
        }
        private void DrawSettings(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(background4, new Rectangle(0, 0, (int)dimensions.X, (int)dimensions.Y), Color.White);
        }

        private void DrawTutorial(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(tutScreen, new Rectangle(0, 0, (int)dimensions.X, (int)dimensions.Y), Color.White);
        }

        private void DrawTransition(SpriteBatch spriteBatch)
        {
            String trans = "Press P to continue to the next level!";
            String levelstr = "" + world + " - " + level;

            spriteBatch.DrawString(menuFont, levelstr, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(levelstr).X / 2), (graphics.PreferredBackBufferHeight / 3)), Color.Red);
            spriteBatch.DrawString(menuFont, trans, new Vector2((graphics.PreferredBackBufferWidth / 2) - (menuFont.MeasureString(trans).X / 2), (graphics.PreferredBackBufferHeight / 2)), Color.Red);
        }

        protected override void Draw(GameTime gameTime)
        {

            spriteBatch.Begin();
            if (gameState == GameState.inSplash)
            {
                DrawSplash(spriteBatch);
            }
            else if (gameState == GameState.inTutorial)
            {
                DrawTutorial(spriteBatch);
            }

            else if (gameState == GameState.inMenu)
            {
                DrawMenu(spriteBatch);
            }
            else if (gameState == GameState.inSettings)
            {
                DrawSettings(spriteBatch);
            }
            else if (gameState == GameState.inTransition)
            {
                DrawTransition(spriteBatch);
            }
            spriteBatch.End();
            
            if (gameState == GameState.inGame)
            {
                //// Assign the matrix and pre-render the lightmap.
                //// Make sure not to change the position of any lights or shadow hulls after this call, as it won't take effect till the next frame!

                this.LightEngine.Matrix = camera.get_transformation(graphics.GraphicsDevice);
                this.LightEngine.Bluriness = 3;
                this.LightEngine.LightMapPrepare();

                // Make sure we clear the backbuffer *after* Krypton is done pre-rendering
                this.GraphicsDevice.Clear(Color.White);

                // ----- DRAW STUFF HERE ----- //
                // By drawing here, you ensure that your scene is properly lit by krypton.
                // Drawing after KryptonEngine.Draw will cause you objects to be drawn on top of the lightmap (can be useful, fyi)

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, camera.get_transformation(graphics.GraphicsDevice));

                DrawGame(spriteBatch);

                spriteBatch.End();

                // ----- DRAW STUFF HERE ----- //

                // Draw krypton (This can be omited if krypton is in the Component list. It will simply draw krypton when base.Draw is called
                this.LightEngine.Draw(gameTime);

                spriteBatch.Begin();
                hud.Draw(spriteBatch);
                spriteBatch.End();

                // Draw the shadow hulls as-is (no shadow stretching) in pure white on top of the shadows
                // You can omit this line if you want to see what the light-map looks like :)
                this.DebugDraw();

            }
            


            base.Draw(gameTime);
        }

        #endregion DrawMethods
    }
}
