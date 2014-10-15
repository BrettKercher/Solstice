using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Solstice
{
    class Map
    {
        Random random;
        const float chanceToStartAlive = .33f;
        const int birthLimit = 4;
        const int deathLimit = 3;
        const int numSimulations = 3;

        int wallTile = 5;
        int floorTile = 1;
        int tempTile = 9;

        const int mapWidth = 30;
        const int mapHeight = 30;
        const int tileSize = 64;

        Texture2D tileSheet;
        Texture2D nullTile;
        Texture2D groundTile01, groundTile02;
        Texture2D wallTile01, wallTile02, wallTile03;

        int[,] intMap;
        Tile[,] map;
        Tile[,] wallMap;
        Tile[,] floorMap;


        public Vector2 Dimensions
        {
            get { return new Vector2(mapWidth, mapHeight); }
        }
        public int TileSize
        {
            get { return tileSize; }
        }
        public int[,] IntMap
        {
            get { return intMap; }
        }

        public Map(Random random)
        {
            this.random = random;
            intMap = new int[mapHeight, mapWidth];
            map = new Tile[mapHeight, mapWidth];
            floorMap = new Tile[mapHeight, mapWidth];
            wallMap = new Tile[mapHeight, mapWidth];
            int isGood = 0;

            while (isGood == 0)
            {
                //Initial a random map based on chanceToStartAlive
                initialize();

                //Run the simulation according to the rules of growing
                //A cell is alive if it is a 1, dead if it is a 3
                //If an alive cell is surrounded by less than "deathLimit" cells, it dies
                //If a dead cell is surrounded by more than "birthLimit" cells, is becomes alive

                for (int i = 0; i < numSimulations; i++)
                    simulate();

                FloodFill(new Vector2(15, 15), tempTile, floorTile);
                int count = CountOpenSpace();
                if ((count >= (mapWidth * mapHeight) / 3))
                {
                    isGood = 1;
                    FillHoles();
                    FloodFill(new Vector2(15, 15), floorTile, tempTile);
                }
            }

            beautify();
            
        }

        public void beautify()
        {
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (intMap[i, j] == 5)
                    {
                        if (checkBelow(i, j) == 1 && checkAbove(i, j) == 1) //Wall tile with open space above and below
                        {
                            intMap[i, j] = 1;
                        }
                        else if (checkBelow(i, j) == 1) //Wall tile with open space right below it
                        {
                            intMap[i, j] = 4;
                        }
                        else if (checkAbove(i, j) == 1) //Wall tile with open space right above it
                        {
                            intMap[i, j] = 3;
                        }
                    }
                }
            }
        }

        public void FloodFill(Vector2 curTile, int newTile, int oldTile)
        {
            if (intMap[(int)curTile.X, (int)curTile.Y] != oldTile || intMap[(int)curTile.X, (int)curTile.Y] == newTile)
                return;
            if (curTile.X <= 0 || curTile.Y <= 0 || curTile.X >= mapWidth - 1 || curTile.Y >= mapHeight - 1)
                return;

            intMap[(int)curTile.X, (int)curTile.Y] = newTile;
            FloodFill(new Vector2(curTile.X - 1, curTile.Y), newTile, oldTile);
            FloodFill(new Vector2(curTile.X + 1, curTile.Y), newTile, oldTile);
            FloodFill(new Vector2(curTile.X, curTile.Y - 1), newTile, oldTile);
            FloodFill(new Vector2(curTile.X, curTile.Y + 1), newTile, oldTile);

            return;
        }

        public int CountOpenSpace()
        {
            int count = 0;

            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (intMap[i, j] == tempTile)
                        count++;
                }
            }

            return count;
        }

        public void FillHoles()
        {
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (intMap[i, j] == floorTile)
                        intMap[i, j] = wallTile;
                }
            }
        }

        public int checkBelow(int x, int y)
        {
            int nX = x ;
            int nY = y + 1; //Coordinates of the tile right below x, y

            if (nY < mapHeight)
                return intMap[nX, nY];
            else
                return 0;
        }

        public int checkAbove(int x, int y)
        {
            int nX = x;
            int nY = y - 1; //Coordinates of the tile right below x, y

            if (nY >= 0)
                return intMap[nX, nY];
            else
                return 0;
        }

        public void transfer()
        {
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (intMap[i, j] == 1)
                    {
                        floorMap[i, j] = new Tile(groundTile01, false, i, j, tileSize);
                        wallMap[i,j] = new Tile(nullTile, false, i, j, tileSize);
                        map[i, j] = new Tile(groundTile01, false, i, j, tileSize);
                    }
                    else if (intMap[i, j] == 2)
                    {
                        floorMap[i, j] = new Tile(groundTile02, false, i, j, tileSize);
                        wallMap[i, j] = new Tile(nullTile, false, i, j, tileSize);
                        map[i, j] = new Tile(groundTile02, false, i, j, tileSize);
                    }
                    else if (intMap[i, j] == 3)
                    {
                        floorMap[i, j] = new Tile(groundTile01, false, i, j, tileSize);
                        wallMap[i, j] = new Tile(wallTile01, false, i, j, tileSize);
                        map[i, j] = new Tile(wallTile01, false, i, j, tileSize);
                    }
                    else if (intMap[i, j] == 4)
                    {
                        floorMap[i, j] = new Tile(groundTile02, false, i, j, tileSize);
                        wallMap[i, j] = new Tile(wallTile02, true, i, j, tileSize);
                        map[i, j] = new Tile(wallTile02, true, i, j, tileSize);
                    }
                    else if (intMap[i, j] == 5)
                    {
                        floorMap[i, j] = new Tile(groundTile02, false, i, j, tileSize);
                        wallMap[i, j] = new Tile(wallTile03, true, i, j, tileSize);
                        map[i, j] = new Tile(wallTile03, true, i, j, tileSize);
                    }
                    else
                    {
                        floorMap[i, j] = new Tile(nullTile, false, i, j, tileSize);
                        wallMap[i, j] = new Tile(nullTile, false, i, j, tileSize);
                        map[i, j] = new Tile(nullTile, false, i, j, tileSize);
                    }
                }
            }
        }

        public Boolean checkCollision(Rectangle playerRect)
        {
            Boolean collision = false;
            Vector2 playerPos = new Vector2(playerRect.X, playerRect.Y);
            int playerX = (int)Math.Floor(playerPos.X / tileSize);
            int playerY = (int)Math.Floor(playerPos.Y / tileSize);
            for (int i = playerX - 2; i <= playerX + 2; i++)
            {
                for (int j = playerY - 2; j <= playerY + 2; j++)
                {
                    if(i >= 0 && j >= 0 && i < mapHeight && j < mapWidth)
                        collision = map[i, j].checkCollision(playerRect);
                    if (collision == true)
                        return collision;
                }
            }
            
            return collision;
        }

        public void initialize()
        {

            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (i == 0 || j == 0 || i == mapHeight - 1 || j == mapWidth - 1 || i == 1 ||  j == 1 || i == mapHeight - 2 || j == mapWidth - 2)
                        intMap[i, j] = wallTile;
                    else
                    {
                        if (random.NextDouble() < chanceToStartAlive)
                            intMap[i, j] = wallTile;
                        else
                            intMap[i, j] = floorTile;
                    }
                }
            }
        }

        public void simulate()
        {
            int[,] tempMap = new int[mapHeight, mapWidth];

            for(int i = 0; i < mapHeight; i++)
                for (int j = 0; j < mapWidth; j++)
                {
                    int numNeighbors = aliveNeighbors(j, i);

                    if (intMap[i, j] == wallTile)
                    {
                        if (numNeighbors < deathLimit)
                            tempMap[i, j] = floorTile;
                        else
                            tempMap[i, j] = wallTile;

                    }
                    else
                    {
                        if (numNeighbors > birthLimit)
                            tempMap[i, j] = wallTile;
                        else
                            tempMap[i, j] = floorTile;
                    }
                }

            for(int i= 0; i < mapHeight; i++)
                for (int j = 0; j < mapWidth; j++)
                    intMap[i,j] = tempMap[i,j];
        }

        public int aliveNeighbors(int x, int y)
        {
            int count = 0;

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int nX = x + j; //Neighbor x
                    int nY = y + i; //Neighbor y

                    if (i == 0 && j == 0) continue; //Dont count yourself
                    else if (nX < 0 || nY < 0 || nX >= mapWidth || nY >= mapHeight) count++; //If neighbor is out of bounds, go ahead and count it
                    else if (intMap[nY, nX] == wallTile) count++; //If neighbor is alive, count++
                }
            }


                return count;
        }

        public void Load(Texture2D spriteSheet)
        {
            tileSheet = spriteSheet;

            Rectangle tileArea;

            tileArea = new Rectangle(1 * tileSize, 0, tileSize, tileSize);
            groundTile01 = CropTile(tileSheet, tileArea);
            tileArea = new Rectangle(2 * tileSize, 0, tileSize, tileSize);
            groundTile02 = CropTile(tileSheet, tileArea);
            tileArea = new Rectangle(3 * tileSize, 0, tileSize, tileSize);
            wallTile01 = CropTile(tileSheet, tileArea);
            tileArea = new Rectangle(4 * tileSize, 0, tileSize, tileSize);
            wallTile02 = CropTile(tileSheet, tileArea);
            tileArea = new Rectangle(5 * tileSize, 0, tileSize, tileSize);
            wallTile03 = CropTile(tileSheet, tileArea);
            tileArea = new Rectangle(0, 0, tileSize, tileSize);
            nullTile = CropTile(tileSheet, tileArea);

            transfer();
            
        }

        private Texture2D CropTile(Texture2D spriteSheet, Rectangle tileArea)
        {
            Texture2D croppedImage = new Texture2D(tileSheet.GraphicsDevice, tileArea.Width, tileArea.Height);
            Color[] tileSheetData = new Color[tileSheet.Width * tileSheet.Height];
            Color[] croppedImageData = new Color[croppedImage.Width * croppedImage.Height];

            tileSheet.GetData<Color>(tileSheetData);

            int index = 0;
            for (int i = tileArea.Y; i < tileArea.Y + tileArea.Height; i++)
            {
                for (int j = tileArea.X; j < tileArea.X + tileArea.Width; j++)
                {
                    croppedImageData[index] = tileSheetData[i * tileSheet.Width + j];
                    index++;
                }
            }
            croppedImage.SetData<Color>(croppedImageData);

            return croppedImage;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    map[i, j].draw(spriteBatch);
                }
            }
        }

        public void DrawFloor(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    floorMap[i, j].draw(spriteBatch);
                }
            }
        }

        public void DrawWall(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < mapHeight; i++)
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    wallMap[i, j].draw(spriteBatch);
                }
            }
        }

    }
}
