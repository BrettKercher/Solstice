using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solstice
{
    class Items
    {

        String type;
        Texture2D texture;
        Rectangle rectangle;
        Vector2 position;
        Vector2 origin;
        Vector2 velocity;
        float speed;
        bool collected;

        const int size = 32;

        public bool Collected
        {
            get { return collected; }
        }
        public String Type
        {
            get { return type; }
        }


        public Items(Texture2D spriteSheet, String type, Vector2 position)
        {
            this.type = type;

            Rectangle ssArea;
            switch (type)
            {
                case "Soul":
                    ssArea = new Rectangle(0, 0, size, size);
                    speed = 100;
                    break;

                default:
                    ssArea = new Rectangle(0, 0, size, size);
                    speed = 0;
                    break;
            }

            texture = CropImage(ssArea, spriteSheet);
            velocity = Vector2.Zero;
            this.position = position;
            rectangle = new Rectangle((int)position.X, (int)position.Y, size, size);
        }

        private Texture2D CropImage(Rectangle itemArea, Texture2D spriteSheet)
        {
            Texture2D croppedImage = new Texture2D(spriteSheet.GraphicsDevice, itemArea.Width, itemArea.Height);
            Color[] spriteSheetData = new Color[spriteSheet.Width * spriteSheet.Height];
            Color[] croppedImageData = new Color[croppedImage.Width * croppedImage.Height];

            spriteSheet.GetData<Color>(spriteSheetData);

            int index = 0;
            for (int i = itemArea.Y; i < itemArea.Y + itemArea.Height; i++)
            {
                for (int j = itemArea.X; j < itemArea.X + itemArea.Width; j++)
                {
                    croppedImageData[index] = spriteSheetData[i * spriteSheet.Width + j];
                    index++;
                }
            }
            croppedImage.SetData<Color>(croppedImageData);

            return croppedImage;
        }

        public bool PerPixelCollision(Player player)
        {
            //First check basic rect collision
            if (!rectangle.Intersects(player.PlayerRectangle))
                return false;

            // Get Color data of each Texture
            Color[] bitsA = new Color[texture.Width * texture.Height];
            texture.GetData(bitsA);
            Color[] bitsB = new Color[player.Texture.Width * player.Texture.Height];
            player.Texture.GetData(bitsB);

            // Calculate the intersecting rectangle
            int x1 = Math.Max(rectangle.X, player.PlayerRectangle.X);
            int x2 = Math.Min(rectangle.X + rectangle.Width, player.PlayerRectangle.X + player.PlayerRectangle.Width);

            int y1 = Math.Max(rectangle.Y, player.PlayerRectangle.Y);
            int y2 = Math.Min(rectangle.Y + rectangle.Height, player.PlayerRectangle.Y + player.PlayerRectangle.Height);

            // For each single pixel in the intersecting rectangle
            for (int y = y1; y < y2; ++y)
            {
                for (int x = x1; x < x2; ++x)
                {
                    // Get the color from each texture
                    Color a = bitsA[(x - rectangle.X) + (y - rectangle.Y) * texture.Width];
                    Color b = bitsB[(x - player.PlayerRectangle.X) + (y - player.PlayerRectangle.Y) * player.Texture.Width];

                    if (a.A != 0 && b.A != 0) // If both colors are not transparent (the alpha channel is not 0), then there is a collision
                    {
                        return true;
                    }
                }
            }
            // If no collision occurred by now, we're clear.
            return false;
        }

        public void Update(GameTime gameTime, Player player)
        {
            origin = new Vector2(position.X + texture.Width / 2, position.Y + texture.Height / 2);

            switch (type)
            {
                case "Soul":
                    if ((player.Origin - origin).Length() < 300)
                    {
                        Vector2 direction = player.Origin - origin;
                        if (direction != Vector2.Zero)
                            direction.Normalize();
                        velocity = direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                        velocity = Vector2.Zero;
                break;
            }

            position += velocity;
            rectangle.X = (int)position.X;
            rectangle.Y = (int)position.Y;

            if (PerPixelCollision(player))
            {
                collected = true;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(!collected)
                spriteBatch.Draw(texture, rectangle, Color.White);
        }
    }
}
