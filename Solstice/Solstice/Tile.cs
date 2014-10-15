using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Solstice
{
    class Tile
    {

        Texture2D texture;
        Rectangle rectangle;
        Boolean solid;


        public Texture2D Texture
        {
            get { return texture; }
        }


        public Boolean checkCollision(Rectangle playerRect)
        {
            if(rectangle.Intersects(playerRect) && solid)
                return true;
            else
                return false;
        }


        public Tile(Texture2D texture, Boolean solid, int x, int y, int size)
        {
            this.texture = texture;
            this.solid = solid;
            rectangle = new Rectangle(x * size, y * size, size, size);
        }

        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, rectangle, Color.White);
        }

    }
}
